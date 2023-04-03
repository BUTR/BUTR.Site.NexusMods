using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Npgsql;

using Quartz;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Jobs
{
    [DisallowConcurrentExecution]
    public sealed class AutocompleteProcessorProcessorJob : IJob
    {
        private readonly ILogger _logger;
        private readonly AppDbContext _dbContext;

        public AutocompleteProcessorProcessorJob(ILogger<AutocompleteProcessorProcessorJob> logger, AppDbContext dbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        private abstract record AutocompleteEntry(Expression BaseExpression);
        private sealed record AutocompleteEntry<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> Expression) : AutocompleteEntry(Expression) where TEntity : IEntity;

        public async Task Execute(IJobExecutionContext context)
        {
            // TODO: External?
            var toAutocomplete = new AutocompleteEntry[]
            {
                new AutocompleteEntry<CrashReportEntity, string>(e => e.GameVersion),
                new AutocompleteEntry<CrashReportEntity, string[]>(e => e.ModIds),
                new AutocompleteEntry<CrashReportEntity, Dictionary<string, string>>(e => e.ModIdToVersion),
                new AutocompleteEntry<NexusModsExposedModsEntity, string[]>(e => e.ModIds),
                new AutocompleteEntry<NexusModsArticleEntity, string>(e => e.AuthorName),
            };

            var autocompleteEntity = _dbContext.Model.FindEntityType(typeof(AutocompleteEntity));
            if (autocompleteEntity is null) return;
            var autocompleteEntityTable = autocompleteEntity.GetSchemaQualifiedTableName();
            var typePropertyName = autocompleteEntity.GetProperty(nameof(AutocompleteEntity.Type)).GetColumnName();
            var valuesPropertyName = autocompleteEntity.GetProperty(nameof(AutocompleteEntity.Values)).GetColumnName();

            foreach (var value in toAutocomplete)
            {
                if (context.CancellationToken.IsCancellationRequested) break;

                if (value.BaseExpression is not LambdaExpression { Body: MemberExpression { Member: PropertyInfo propertyInfo } }) continue;
                if (!value.BaseExpression.Type.IsGenericType || value.BaseExpression.Type.GenericTypeArguments.Length != 2) continue;
                var entityType = value.BaseExpression.Type.GenericTypeArguments[0];
                var propertyType = value.BaseExpression.Type.GenericTypeArguments[1];
                var name = $"{entityType.Name}.{propertyInfo.Name}";

                var entity = _dbContext.Model.FindEntityType(entityType);
                if (entity is null) continue;

                var entityTable = entity.GetSchemaQualifiedTableName();

                var property = entity.GetProperty(propertyInfo.Name);
                if (property is null) continue;

                var propertyColumnName = property.GetColumnName();

                if (typeof(IDictionary<string, string>).IsAssignableFrom(propertyType))
                {
                    /*
                    var sql = $@"
WITH values AS (SELECT DISTINCT {propertyColumnName} -> @modId as mod_version FROM {entityTable} WHERE exist({modIdToVersionName}, @modId) ORDER BY mod_version)
SELECT
    array_agg(mod_version) as {modIdsName}
FROM
    values
WHERE
    mod_version ILIKE @val || '%'";
                    var valPram = new NpgsqlParameter<string>("val", value.Name);
                    await _dbContext.Database.ExecuteSqlRawAsync(sql, new object[] { valPram }, context.CancellationToken);
                    */
                }
                if (typeof(IEnumerable<string>).IsAssignableFrom(propertyType))
                {
                    var sql = @$"
DELETE FROM {autocompleteEntityTable} WHERE {typePropertyName} = @val;
WITH values AS (SELECT DISTINCT unnest({propertyColumnName}) as props FROM {entityTable} ORDER BY props)
INSERT INTO {autocompleteEntityTable}
SELECT @val as {typePropertyName}, array_agg(props) as {valuesPropertyName} FROM values";
                    var valPram = new NpgsqlParameter<string>("val", name);
                    await _dbContext.Database.ExecuteSqlRawAsync(sql, new object[] { valPram }, context.CancellationToken);
                }
                if (propertyType == typeof(string))
                {
                    var sql = @$"
DELETE FROM {autocompleteEntityTable} WHERE {typePropertyName} = @val;
WITH values AS (SELECT DISTINCT {propertyColumnName} as props FROM {entityTable} ORDER BY props)
INSERT INTO {autocompleteEntityTable}
SELECT @val as {typePropertyName}, array_agg(props) as {valuesPropertyName} FROM values";
                    var valPram = new NpgsqlParameter<string>("val", name);
                    await _dbContext.Database.ExecuteSqlRawAsync(sql, new object[] { valPram }, context.CancellationToken);
                }
            }

            context.Result = "Updated Autocomplete Data";
            context.SetIsSuccess(true);
        }
    }
}
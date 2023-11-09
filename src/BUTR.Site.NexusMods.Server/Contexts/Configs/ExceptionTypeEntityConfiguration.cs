using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class ExceptionTypeEntityConfiguration : BaseEntityConfigurationWithTenant<ExceptionTypeEntity>
{
    public ExceptionTypeEntityConfiguration(ITenantContextAccessor tenantContextAccessor) : base(tenantContextAccessor) { }

    protected override void ConfigureModel(EntityTypeBuilder<ExceptionTypeEntity> builder)
    {
        builder.Property(x => x.ExceptionTypeId).HasColumnName("exception_type_id").HasVogenConversion();
        builder.ToTable("exception_type", "exception").HasKey(x => new { x.TenantId, x.ExceptionTypeId });

        base.ConfigureModel(builder);
    }
}
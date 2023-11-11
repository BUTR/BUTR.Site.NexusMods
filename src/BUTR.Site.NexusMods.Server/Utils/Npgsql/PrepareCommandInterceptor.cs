using Microsoft.EntityFrameworkCore.Diagnostics;

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Utils.Npgsql;

public class PrepareCommandInterceptor : DbCommandInterceptor
{
    public const string Tag = "Prepare";

    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        Prepare(command);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken ct = default)
    {
        Prepare(command);
        return base.ReaderExecutingAsync(command, eventData, result, ct);
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
    {
        Prepare(command);
        return base.NonQueryExecutingAsync(command, eventData, result, ct);
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<object> result, CancellationToken ct = default)
    {
        Prepare(command);
        return base.ScalarExecutingAsync(command, eventData, result, ct);
    }

    public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
    {
        Prepare(command);
        return base.NonQueryExecuting(command, eventData, result);
    }

    public override InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
    {
        Prepare(command);
        return base.ScalarExecuting(command, eventData, result);
    }

    private static void Prepare(IDbCommand command)
    {
        if (!command.CommandText.Contains($"-- {Tag}", StringComparison.Ordinal))
            return;

        command.Prepare();
    }
}
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.Quartz;

using Quartz;

using System;
using System.Globalization;

namespace BUTR.Site.NexusMods.Server.Extensions;

public static class JobExecutionContextExtensions
{
    public static IJobExecutionContext SetReturnCode(this IJobExecutionContext context, string value)
    {
        context.Put(JobDataMapKeys.ReturnCode, value);
        return context;
    }

    public static IJobExecutionContext SetReturnCode(this IJobExecutionContext context, int value)
    {
        context.Put(JobDataMapKeys.ReturnCode, value.ToString());
        return context;
    }

    public static IJobExecutionContext SetExecutionDetails(this IJobExecutionContext context, string execDetails)
    {
        context.Put(JobDataMapKeys.ExecutionDetails, execDetails);
        return context;
    }

    public static IJobExecutionContext SetIsSuccess(this IJobExecutionContext context, bool success)
    {
        context.Put(JobDataMapKeys.IsSuccess, success);
        return context;
    }

    public static string? GetReturnCode(this IJobExecutionContext context)
    {
        var val = context.Get(JobDataMapKeys.ReturnCode);
        return val != null ? Convert.ToString(val, CultureInfo.InvariantCulture) : null;
    }

    public static string? GetExecutionDetails(this IJobExecutionContext context)
    {
        var val = context.Get(JobDataMapKeys.ExecutionDetails);
        return val != null ? Convert.ToString(val, CultureInfo.InvariantCulture) : null;
    }

    public static bool? GetIsSuccess(this IJobExecutionContext context)
    {
        var val = context.Get(JobDataMapKeys.IsSuccess);
        return val == null ? null : Convert.ToBoolean(val);
    }
}
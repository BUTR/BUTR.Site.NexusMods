using BUTR.Site.NexusMods.Server.Models.Database;

namespace BUTR.Site.NexusMods.Server.Repositories;

public interface ICrashReportToFileIdEntityRepositoryRead : IRepositoryRead<CrashReportToFileIdEntity>;
public interface ICrashReportToFileIdEntityRepositoryWrite : IRepositoryWrite<CrashReportToFileIdEntity>, ICrashReportToFileIdEntityRepositoryRead;
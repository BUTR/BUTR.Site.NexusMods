using BUTR.Site.NexusMods.Server.Models.Database;

namespace BUTR.Site.NexusMods.Server.Repositories;

public interface IExceptionTypeRepositoryRead : IRepositoryRead<ExceptionTypeEntity>;
public interface IExceptionTypeRepositoryWrite : IRepositoryWrite<ExceptionTypeEntity>, IExceptionTypeRepositoryRead;
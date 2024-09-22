using BUTR.Site.NexusMods.Server.Models;

namespace BUTR.Site.NexusMods.Server.Repositories;

public interface IUnitOfWorkFactory
{
    IUnitOfRead CreateUnitOfRead();
    IUnitOfRead CreateUnitOfRead(TenantId tenant);

    IUnitOfWrite CreateUnitOfWrite();
    IUnitOfWrite CreateUnitOfWrite(TenantId tenant);
}
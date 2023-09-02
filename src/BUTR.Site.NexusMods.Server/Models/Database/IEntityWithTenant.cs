using BUTR.Site.NexusMods.Shared;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public interface IEntityWithTenant : IEntity
{
    Tenant TenantId { get; }
}
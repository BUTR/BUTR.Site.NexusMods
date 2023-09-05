namespace BUTR.Site.NexusMods.Server.Models.Database;

public interface IEntityWithTenant : IEntity
{
    TenantId TenantId { get; }
}
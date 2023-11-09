namespace BUTR.Site.NexusMods.Server.Models.API;

public sealed record ProfileTenantModel
{
    public required TenantId TenantId { get; init; }
    public required string Name { get; init; }
}
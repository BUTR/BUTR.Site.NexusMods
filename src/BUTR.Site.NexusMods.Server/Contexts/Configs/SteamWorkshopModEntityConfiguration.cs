using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class SteamWorkshopModEntityConfiguration : BaseEntityConfigurationWithTenant<SteamWorkshopModEntity>
{
    public SteamWorkshopModEntityConfiguration(ITenantContextAccessor tenantContextAccessor) : base(tenantContextAccessor) { }

    protected override void ConfigureModel(EntityTypeBuilder<SteamWorkshopModEntity> builder)
    {
        builder.Property(x => x.SteamWorkshopModId).HasColumnName("steamworkshop_mod_id").HasVogenConversion();
        builder.ToTable("steamworkshop_mod", "steamworkshop_mod").HasKey(x => new
        {
            x.TenantId,
            x.SteamWorkshopModId,
        });

        base.ConfigureModel(builder);
    }
}
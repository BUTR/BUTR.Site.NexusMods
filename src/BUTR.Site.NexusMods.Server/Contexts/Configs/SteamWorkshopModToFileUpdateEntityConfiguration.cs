using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class SteamWorkshopModToFileUpdateEntityConfiguration : BaseEntityConfigurationWithTenant<SteamWorkshopModToFileUpdateEntity>
{
    public SteamWorkshopModToFileUpdateEntityConfiguration(ITenantContextAccessor tenantContextAccessor) : base(tenantContextAccessor) { }

    protected override void ConfigureModel(EntityTypeBuilder<SteamWorkshopModToFileUpdateEntity> builder)
    {
        builder.Property(x => x.SteamWorkshopModId).HasColumnName("steamworkshop_mod_file_update_id").HasVogenConversion().ValueGeneratedNever();
        builder.Property(x => x.LastCheckedDate).HasColumnName("date_of_last_check");
        builder.ToTable("steamworkshop_mod_file_update", "steamworkshop_mod").HasKey(x => new
        {
            x.TenantId,
            x.SteamWorkshopModId,
        });

        builder.HasOne(x => x.SteamWorkshopMod)
            .WithOne(x => x.FileUpdate)
            .HasForeignKey<SteamWorkshopModToFileUpdateEntity>(x => new { x.TenantId, x.SteamWorkshopModId })
            .HasPrincipalKey<SteamWorkshopModEntity>(x => new { x.TenantId, x.SteamWorkshopModId })
            .OnDelete(DeleteBehavior.Cascade);

        base.ConfigureModel(builder);
    }
}
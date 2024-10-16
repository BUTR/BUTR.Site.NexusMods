using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class NexusModsUserToSteamWorkshopModEntityConfiguration : BaseEntityConfigurationWithTenant<NexusModsUserToSteamWorkshopModEntity>
{
    public NexusModsUserToSteamWorkshopModEntityConfiguration(ITenantContextAccessor tenantContextAccessor) : base(tenantContextAccessor) { }

    protected override void ConfigureModel(EntityTypeBuilder<NexusModsUserToSteamWorkshopModEntity> builder)
    {
        builder.Property(x => x.NexusModsUserId).HasColumnName("nexusmods_user_steamworkshop_mod_id").HasVogenConversion().ValueGeneratedNever();
        builder.Property(x => x.SteamWorkshopModId).HasColumnName("steamworkshop_mod_id").HasVogenConversion();
        builder.Property(x => x.LinkType).HasColumnName("nexusmods_user_mod_link_type_id");
        builder.ToTable("nexusmods_user_steamworkshop_mod", "nexusmods_user").HasKey(x => new
        {
            x.TenantId,
            x.NexusModsUserId,
            x.SteamWorkshopModId,
            x.LinkType,
        });

        builder.HasOne(x => x.NexusModsUser)
            .WithMany(x => x.ToSteamWorkshopMods)
            .HasForeignKey(x => x.NexusModsUserId)
            .HasPrincipalKey(x => x.NexusModsUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SteamWorkshopMod)
            .WithMany(x => x.ToNexusModsUsers)
            .HasForeignKey(x => new { x.TenantId, x.SteamWorkshopModId })
            .HasPrincipalKey(x => new { x.TenantId, x.SteamWorkshopModId })
            .OnDelete(DeleteBehavior.Cascade);

        base.ConfigureModel(builder);
    }
}
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class NexusModsUserToIntegrationSteamEntityConfiguration : BaseEntityConfiguration<NexusModsUserToIntegrationSteamEntity>
{
    protected override void ConfigureModel(EntityTypeBuilder<NexusModsUserToIntegrationSteamEntity> builder)
    {
        builder.Property<int>(nameof(NexusModsUserEntity.NexusModsUserId)).HasColumnName("nexusmods_user_to_steam_id").ValueGeneratedNever();
        builder.Property(x => x.SteamUserId).HasColumnName("steam_user_id");
        builder.ToTable("nexusmods_user_to_integration_steam", "nexusmods_user").HasKey(nameof(NexusModsUserEntity.NexusModsUserId));

        builder.HasOne(x => x.NexusModsUser)
            .WithOne(x => x.ToSteam)
            .HasForeignKey<NexusModsUserToIntegrationSteamEntity>(nameof(NexusModsUserEntity.NexusModsUserId))
            .HasPrincipalKey<NexusModsUserEntity>(x => x.NexusModsUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ToOwnedTenants)
            .WithOne()
            .HasForeignKey(x => x.SteamUserId)
            .HasPrincipalKey(x => x.SteamUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.NexusModsUser).AutoInclude();

        base.ConfigureModel(builder);
    }
}
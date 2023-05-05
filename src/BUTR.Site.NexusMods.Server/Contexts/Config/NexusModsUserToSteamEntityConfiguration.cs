using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Config;

public class NexusModsUserToSteamEntityConfiguration : BaseEntityConfiguration<NexusModsUserToSteamEntity>
{
    protected override void ConfigureModel(EntityTypeBuilder<NexusModsUserToSteamEntity> builder)
    {
        builder.ToTable("nexusmods_to_steam").HasKey(p => p.NexusModsUserId).HasName("nexusmods_to_steam_pkey");
        builder.Property(p => p.NexusModsUserId).HasColumnName("nexusmods_user_id").ValueGeneratedNever().IsRequired();
        builder.Property(p => p.SteamId).HasColumnName("steam_user_id").IsRequired();
    }
}
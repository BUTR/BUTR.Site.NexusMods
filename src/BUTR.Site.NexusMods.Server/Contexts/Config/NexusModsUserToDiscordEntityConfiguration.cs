using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Config;

public class NexusModsUserToDiscordEntityConfiguration : BaseEntityConfiguration<NexusModsUserToDiscordEntity>
{
    protected override void ConfigureModel(EntityTypeBuilder<NexusModsUserToDiscordEntity> builder)
    {
        builder.ToTable("nexusmods_to_discord").HasKey(p => p.NexusModsId).HasName("nexusmods_to_discord_pkey");
        builder.Property(p => p.NexusModsId).HasColumnName("nexusmods_user_id").ValueGeneratedNever().IsRequired();
        builder.Property(p => p.DiscordId).HasColumnName("discord_user_id").IsRequired();
    }
}
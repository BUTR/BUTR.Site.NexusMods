using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class NexusModsUserToIntegrationDiscordEntityConfiguration : BaseEntityConfiguration<NexusModsUserToIntegrationDiscordEntity>
{
    protected override void ConfigureModel(EntityTypeBuilder<NexusModsUserToIntegrationDiscordEntity> builder)
    {
        builder.Property(x => x.NexusModsUserId).HasColumnName("nexusmods_user_to_discord_id").HasVogenConversion().ValueGeneratedNever();
        builder.Property(x => x.DiscordUserId).HasColumnName("discord_user_id");
        builder.ToTable("nexusmods_user_to_integration_discord", "nexusmods_user").HasKey(new[]
        {
            nameof(NexusModsUserEntity.NexusModsUserId)
        });

        builder.HasOne(x => x.NexusModsUser)
            .WithOne(x => x.ToDiscord)
            .HasForeignKey<NexusModsUserToIntegrationDiscordEntity>(x => x.NexusModsUserId)
            .HasPrincipalKey<NexusModsUserEntity>(x => x.NexusModsUserId)
            .OnDelete(DeleteBehavior.Cascade);

        base.ConfigureModel(builder);
    }
}
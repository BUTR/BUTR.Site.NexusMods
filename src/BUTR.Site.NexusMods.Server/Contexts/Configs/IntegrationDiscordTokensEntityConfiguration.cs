using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class IntegrationDiscordTokensEntityConfiguration : BaseEntityConfiguration<IntegrationDiscordTokensEntity>
{
    protected override void ConfigureModel(EntityTypeBuilder<IntegrationDiscordTokensEntity> builder)
    {
        builder.Property(x => x.NexusModsUserId).HasColumnName("integration_discord_tokens_id").HasVogenConversion().ValueGeneratedNever();
        builder.Property(x => x.DiscordUserId).HasColumnName("discord_user_id");
        builder.Property(x => x.RefreshToken).HasColumnName("refresh_token");
        builder.Property(x => x.AccessToken).HasColumnName("access_token");
        builder.Property(x => x.AccessTokenExpiresAt).HasColumnName("access_token_expires_at");
        builder.ToTable("integration_discord_tokens", "integration").HasKey(x => new
        {
            x.NexusModsUserId,
        });

        builder.HasOne(x => x.NexusModsUser)
            .WithOne()
            .HasForeignKey<IntegrationDiscordTokensEntity>(x => x.NexusModsUserId)
            .HasPrincipalKey<NexusModsUserEntity>(x => x.NexusModsUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.UserToDiscord)
            .WithOne(x => x.ToTokens)
            .HasForeignKey<IntegrationDiscordTokensEntity>(x => x.DiscordUserId)
            .HasPrincipalKey<NexusModsUserToIntegrationDiscordEntity>(x => x.DiscordUserId)
            .OnDelete(DeleteBehavior.Cascade);

        base.ConfigureModel(builder);
    }
}
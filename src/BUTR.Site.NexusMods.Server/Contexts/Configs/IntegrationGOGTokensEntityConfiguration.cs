using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class IntegrationGOGTokensEntityConfiguration : BaseEntityConfiguration<IntegrationGOGTokensEntity>
{
    protected override void ConfigureModel(EntityTypeBuilder<IntegrationGOGTokensEntity> builder)
    {
        builder.Property(x => x.NexusModsUserId).HasColumnName("integration_gog_tokens_id").HasVogenConversion().ValueGeneratedNever();
        builder.Property(x => x.GOGUserId).HasColumnName("gog_user_id");
        builder.Property(x => x.RefreshToken).HasColumnName("refresh_token");
        builder.Property(x => x.AccessToken).HasColumnName("access_token");
        builder.Property(x => x.AccessTokenExpiresAt).HasColumnName("access_token_expires_at");
        builder.ToTable("integration_gog_tokens", "integration").HasKey(x => new
        {
            x.NexusModsUserId,
        });

        builder.HasOne(x => x.NexusModsUser)
            .WithOne()
            .HasForeignKey<IntegrationGOGTokensEntity>(x => x.NexusModsUserId)
            .HasPrincipalKey<NexusModsUserEntity>(x => x.NexusModsUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.UserToGOG)
            .WithOne(x => x.ToTokens)
            .HasForeignKey<IntegrationGOGTokensEntity>(x => x.GOGUserId)
            .HasPrincipalKey<NexusModsUserToIntegrationGOGEntity>(x => x.GOGUserId)
            .OnDelete(DeleteBehavior.Cascade);

        base.ConfigureModel(builder);
    }
}
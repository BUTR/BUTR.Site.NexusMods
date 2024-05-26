using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class IntegrationGitHubTokensEntityConfiguration : BaseEntityConfiguration<IntegrationGitHubTokensEntity>
{
    protected override void ConfigureModel(EntityTypeBuilder<IntegrationGitHubTokensEntity> builder)
    {
        builder.Property(x => x.NexusModsUserId).HasColumnName("integration_github_tokens_id").HasVogenConversion().ValueGeneratedNever();
        builder.Property(x => x.GitHubUserId).HasColumnName("github_user_id");
        builder.Property(x => x.AccessToken).HasColumnName("access_token");
        builder.ToTable("integration_github_tokens", "integration").HasKey(x => new
        {
            x.NexusModsUserId,
        });

        builder.HasOne(x => x.NexusModsUser)
            .WithOne()
            .HasForeignKey<IntegrationGitHubTokensEntity>(x => x.NexusModsUserId)
            .HasPrincipalKey<NexusModsUserEntity>(x => x.NexusModsUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.UserToGitHub)
            .WithOne(x => x.ToTokens)
            .HasForeignKey<IntegrationGitHubTokensEntity>(x => x.GitHubUserId)
            .HasPrincipalKey<NexusModsUserToIntegrationGitHubEntity>(x => x.GitHubUserId)
            .OnDelete(DeleteBehavior.Cascade);

        base.ConfigureModel(builder);
    }
}
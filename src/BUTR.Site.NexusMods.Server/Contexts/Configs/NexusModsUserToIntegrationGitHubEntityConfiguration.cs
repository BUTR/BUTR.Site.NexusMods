using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class NexusModsUserToIntegrationGitHubEntityConfiguration : BaseEntityConfiguration<NexusModsUserToIntegrationGitHubEntity>
{
    protected override void ConfigureModel(EntityTypeBuilder<NexusModsUserToIntegrationGitHubEntity> builder)
    {
        builder.Property(x => x.NexusModsUserId).HasColumnName("nexusmods_user_to_github_id").HasVogenConversion().ValueGeneratedNever();
        builder.Property(x => x.GitHubUserId).HasColumnName("github_user_id");
        builder.ToTable("nexusmods_user_to_integration_github", "nexusmods_user").HasKey(x => new
        {
            x.NexusModsUserId,
        });

        builder.HasOne(x => x.NexusModsUser)
            .WithOne(x => x.ToGitHub)
            .HasForeignKey<NexusModsUserToIntegrationGitHubEntity>(x => x.NexusModsUserId)
            .HasPrincipalKey<NexusModsUserEntity>(x => x.NexusModsUserId)
            .OnDelete(DeleteBehavior.Cascade);

        base.ConfigureModel(builder);
    }
}
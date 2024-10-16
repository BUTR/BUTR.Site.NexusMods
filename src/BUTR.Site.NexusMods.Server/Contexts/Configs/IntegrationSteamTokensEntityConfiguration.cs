using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public sealed class IntegrationSteamTokensEntityConfiguration : BaseEntityConfiguration<IntegrationSteamTokensEntity>
{
    protected override void ConfigureModel(EntityTypeBuilder<IntegrationSteamTokensEntity> builder)
    {
        builder.Property(x => x.NexusModsUserId).HasColumnName("integration_steam_tokens_id").HasVogenConversion().ValueGeneratedNever();
        builder.Property(x => x.SteamUserId).HasColumnName("steam_user_id").HasVogenConversion();
        builder.Property(x => x.Data).HasColumnName("data").HasColumnType("hstore");
        builder.ToTable("integration_steam_tokens", "integration").HasKey(x => new
        {
            x.NexusModsUserId,
        });

        builder.HasOne(x => x.NexusModsUser)
            .WithOne()
            .HasForeignKey<IntegrationSteamTokensEntity>(x => x.NexusModsUserId)
            .HasPrincipalKey<NexusModsUserEntity>(x => x.NexusModsUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.UserToSteam)
            .WithOne(x => x.ToTokens)
            .HasForeignKey<IntegrationSteamTokensEntity>(x => x.SteamUserId)
            .HasPrincipalKey<NexusModsUserToIntegrationSteamEntity>(x => x.SteamUserId)
            .OnDelete(DeleteBehavior.Cascade);

        base.ConfigureModel(builder);
    }
}
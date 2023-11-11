using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public sealed class IntegrationSteamTokensEntityConfiguration : BaseEntityConfiguration<IntegrationSteamTokensEntity>
{
    protected override void ConfigureModel(EntityTypeBuilder<IntegrationSteamTokensEntity> builder)
    {
        builder.Property<NexusModsUserId>(nameof(NexusModsUserEntity.NexusModsUserId)).HasColumnName("integration_steam_tokens_id").HasVogenConversion().ValueGeneratedNever();
        builder.Property(x => x.SteamUserId).HasColumnName("steam_user_id");
        builder.Property(x => x.Data).HasColumnName("data").HasColumnType("hstore");
        builder.ToTable("integration_steam_tokens", "integration").HasKey(nameof(NexusModsUserEntity.NexusModsUserId));

        builder.HasOne(x => x.NexusModsUser)
            .WithOne()
            .HasForeignKey<IntegrationSteamTokensEntity>(nameof(NexusModsUserEntity.NexusModsUserId))
            .HasPrincipalKey<NexusModsUserEntity>(x => x.NexusModsUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.UserToSteam)
            .WithOne(x => x.ToTokens)
            .HasForeignKey<IntegrationSteamTokensEntity>(x => x.SteamUserId)
            .HasPrincipalKey<NexusModsUserToIntegrationSteamEntity>(x => x.SteamUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.NexusModsUser).AutoInclude();

        base.ConfigureModel(builder);
    }
}
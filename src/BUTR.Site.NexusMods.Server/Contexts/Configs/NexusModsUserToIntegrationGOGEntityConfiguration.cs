using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class NexusModsUserToIntegrationGOGEntityConfiguration : BaseEntityConfiguration<NexusModsUserToIntegrationGOGEntity>
{
    protected override void ConfigureModel(EntityTypeBuilder<NexusModsUserToIntegrationGOGEntity> builder)
    {
        builder.Property(x => x.NexusModsUserId).HasColumnName("nexusmods_user_to_gog_id").HasVogenConversion().ValueGeneratedNever();
        builder.Property(x => x.GOGUserId).HasColumnName("gog_user_id");
        builder.ToTable("nexusmods_user_to_integration_gog", "nexusmods_user").HasKey(x => new
        {
            x.NexusModsUserId,
        });

        builder.HasOne(x => x.NexusModsUser)
            .WithOne(x => x.ToGOG)
            .HasForeignKey<NexusModsUserToIntegrationGOGEntity>(x => x.NexusModsUserId)
            .HasPrincipalKey<NexusModsUserEntity>(x => x.NexusModsUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ToOwnedTenants)
            .WithOne()
            .HasForeignKey(x => x.GOGUserId)
            .HasPrincipalKey(x => x.GOGUserId)
            .OnDelete(DeleteBehavior.Cascade);

        base.ConfigureModel(builder);
    }
}
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class NexusModsUserToNameEntityConfiguration : BaseEntityConfiguration<NexusModsUserToNameEntity>
{
    protected override void ConfigureModel(EntityTypeBuilder<NexusModsUserToNameEntity> builder)
    {
        builder.Property(x => x.NexusModsUserId).HasColumnName("nexusmods_user_name_id").HasVogenConversion().ValueGeneratedNever();
        builder.Property(x => x.Name).HasColumnName("name").HasVogenConversion();
        builder.ToTable("nexusmods_user_name", "nexusmods_user").HasKey(x => new
        {
            x.NexusModsUserId,
        });

        builder.HasOne(x => x.NexusModsUser)
            .WithOne(x => x.Name)
            .HasForeignKey<NexusModsUserToNameEntity>(x => x.NexusModsUserId)
            .HasPrincipalKey<NexusModsUserEntity>(x => x.NexusModsUserId)
            .OnDelete(DeleteBehavior.Cascade);

        base.ConfigureModel(builder);
    }
}
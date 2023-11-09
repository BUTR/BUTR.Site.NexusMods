using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class NexusModsUserEntityConfiguration : BaseEntityConfiguration<NexusModsUserEntity>
{
    protected override void ConfigureModel(EntityTypeBuilder<NexusModsUserEntity> builder)
    {
        builder.Property(x => x.NexusModsUserId).HasColumnName("nexusmods_user_id").HasVogenConversion().ValueGeneratedNever();
        builder.ToTable("nexusmods_user", "nexusmods_user").HasKey(x => x.NexusModsUserId);

        base.ConfigureModel(builder);
    }
}
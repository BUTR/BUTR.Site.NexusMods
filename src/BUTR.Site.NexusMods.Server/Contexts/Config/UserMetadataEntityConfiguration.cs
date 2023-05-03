using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Config;

public class UserMetadataEntityConfiguration : BaseEntityConfiguration<NexusModsUserMetadataEntity>
{
    protected override void ConfigureModel(EntityTypeBuilder<NexusModsUserMetadataEntity> builder)
    {
        builder.ToTable("user_metadata").HasKey(p => p.NexusModsUserId).HasName("user_metadata_pkey");
        builder.Property(p => p.NexusModsUserId).HasColumnName("user_id").ValueGeneratedNever().IsRequired();
        builder.Property(p => p.Metadata).HasColumnName("metadata").IsRequired();
    }
}
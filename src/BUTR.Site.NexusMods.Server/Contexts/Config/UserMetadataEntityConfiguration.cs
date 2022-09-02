using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Config
{
    public class UserMetadataEntityConfiguration : BaseEntityConfiguration<UserMetadataEntity>
    {
        protected override void ConfigureModel(EntityTypeBuilder<UserMetadataEntity> builder)
        {
            builder.ToTable("user_metadata").HasKey(p => p.UserId).HasName("user_metadata_pkey");
            builder.Property(p => p.UserId).HasColumnName("user_id").ValueGeneratedNever().IsRequired();
            builder.Property(p => p.Metadata).HasColumnName("metadata").IsRequired();
        }
    }
}
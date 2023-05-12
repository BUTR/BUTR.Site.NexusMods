using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Config
{
    public class NexusModsUserToGOGEntityConfiguration : BaseEntityConfiguration<NexusModsUserToGOGEntity>
    {
        protected override void ConfigureModel(EntityTypeBuilder<NexusModsUserToGOGEntity> builder)
        {
            builder.ToTable("nexusmods_to_gog").HasKey(p => p.NexusModsUserId).HasName("nexusmods_to_gog_pkey");
            builder.Property(p => p.NexusModsUserId).HasColumnName("nexusmods_user_id").ValueGeneratedNever().IsRequired();
            builder.Property(p => p.UserId).HasColumnName("gog_user_id").IsRequired();
        }
    }
}
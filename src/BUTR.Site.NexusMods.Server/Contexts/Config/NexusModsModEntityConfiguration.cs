using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Utils;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Config
{
    public class NexusModsModEntityConfiguration : BaseEntityConfiguration<NexusModsModEntity>
    {
        protected override void ConfigureModel(EntityTypeBuilder<NexusModsModEntity> builder)
        {
            builder.ToTable("nexusmods_mod_entity").HasKey(p => p.ModId).HasName("nexusmods_mod_entity_pkey");
            builder.Property(p => p.ModId).HasColumnName("mod_id").ValueGeneratedNever().IsRequired();
            builder.Property(p => p.Name).HasColumnName("name").IsRequired();
            builder.Property(p => p.UserIds).HasColumnName("user_ids")/*.HasConversion<ImmutableArrayToArrayConverter<int>>()*/.IsRequired();
        }
    }
}
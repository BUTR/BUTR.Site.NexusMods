using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Config
{
    public class ModNexusModsManualLinkEntityConfiguration : BaseEntityConfiguration<ModNexusModsManualLinkEntity>
    {
        protected override void ConfigureModel(EntityTypeBuilder<ModNexusModsManualLinkEntity> builder)
        {
            builder.ToTable("mod_nexusmods_manual_link").HasKey(p => p.ModId).HasName("mod_nexus_mods_manual_link_pkey");
            builder.Property(p => p.ModId).HasColumnName("mod_id").ValueGeneratedNever().IsRequired();
            builder.Property(p => p.NexusModsId).HasColumnName("nexusmods_id").IsRequired();
        }
    }
}
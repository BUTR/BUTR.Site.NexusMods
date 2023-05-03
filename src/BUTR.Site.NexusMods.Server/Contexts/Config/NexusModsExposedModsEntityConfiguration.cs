using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Config;

public class NexusModsExposedModsEntityConfiguration : BaseEntityConfiguration<NexusModsExposedModsEntity>
{
    protected override void ConfigureModel(EntityTypeBuilder<NexusModsExposedModsEntity> builder)
    {
        builder.ToTable("nexusmods_exposed_mods_entity").HasKey(p => p.NexusModsModId).HasName("nexusmods_exposed_mods_entity_pkey");
        builder.Property(p => p.NexusModsModId).HasColumnName("nexusmods_mod_id").ValueGeneratedNever().IsRequired();
        builder.Property(p => p.ModuleIds).HasColumnName("mod_ids").IsRequired();
        builder.Property(p => p.LastCheckedDate).HasColumnName("last_checked_date").IsRequired();
    }
}
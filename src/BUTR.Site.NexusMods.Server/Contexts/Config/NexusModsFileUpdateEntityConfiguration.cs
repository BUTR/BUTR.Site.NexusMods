using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Config;

public class NexusModsFileUpdateEntityConfiguration : BaseEntityConfiguration<NexusModsFileUpdateEntity>
{
    protected override void ConfigureModel(EntityTypeBuilder<NexusModsFileUpdateEntity> builder)
    {
        builder.ToTable("nexusmods_file_update_entity").HasKey(p => p.NexusModsModId).HasName("nexusmods_file_update_entity_pkey");
        builder.Property(p => p.NexusModsModId).HasColumnName("nexusmods_mod_id").ValueGeneratedNever().IsRequired();
        builder.Property(p => p.LastCheckedDate).HasColumnName("last_checked_date").IsRequired();
    }
}
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Config;

public class NexusModsModManualLinkedNexusModsUsersEntityConfiguration : BaseEntityConfiguration<NexusModsModManualLinkedNexusModsUsersEntity>
{
    protected override void ConfigureModel(EntityTypeBuilder<NexusModsModManualLinkedNexusModsUsersEntity> builder)
    {
        builder.ToTable("nexusmods_mod_manual_link_nexusmods_users").HasKey(p => p.NexusModsModId).HasName("nexusmods_mod_manual_link_nexusmods_users_pkey");
        builder.Property(p => p.NexusModsModId).HasColumnName("nexusmods_mod_id").ValueGeneratedNever().IsRequired();
        builder.Property(p => p.AllowedNexusModsUserIds).HasColumnName("allowed_nexusmods_user_ids").IsRequired();
    }
}
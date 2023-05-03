using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Config;

public class UserAllowedModsEntityConfiguration : BaseEntityConfiguration<NexusModsUserAllowedModuleIdsEntity>
{
    protected override void ConfigureModel(EntityTypeBuilder<NexusModsUserAllowedModuleIdsEntity> builder)
    {
        builder.ToTable("user_allowed_mods").HasKey(p => p.NexusModsUserId).HasName("user_allowed_mods_pkey");
        builder.Property(p => p.NexusModsUserId).HasColumnName("user_id").ValueGeneratedNever().IsRequired();
        builder.Property(p => p.AllowedModuleIds).HasColumnName("allowed_mod_ids")/*.HasConversion<ImmutableArrayToArrayConverter<string>>()*/.IsRequired();
    }
}
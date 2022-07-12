using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Config
{
    public class UserAllowedModsEntityConfiguration : BaseEntityConfiguration<UserAllowedModsEntity>
    {
        protected override void ConfigureModel(EntityTypeBuilder<UserAllowedModsEntity> builder)
        {
            builder.ToTable("user_allowed_mods").HasKey(p => p.UserId).HasName("user_allowed_mods_pkey");
            builder.Property(p => p.UserId).HasColumnName("user_id").ValueGeneratedNever().IsRequired();
            builder.Property(p => p.AllowedModIds).HasColumnName("allowed_mod_ids")/*.HasConversion<ImmutableArrayToArrayConverter<string>>()*/.IsRequired();
        }
    }
}
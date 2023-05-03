using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Config;

public class UserRoleEntityConfiguration : BaseEntityConfiguration<NexusModsUserRoleEntity>
{
    protected override void ConfigureModel(EntityTypeBuilder<NexusModsUserRoleEntity> builder)
    {
        builder.ToTable("user_role").HasKey(p => p.NexusModsUserId).HasName("user_role_pkey");
        builder.Property(p => p.NexusModsUserId).HasColumnName("user_id").ValueGeneratedNever().IsRequired();
        builder.Property(p => p.Role).HasColumnName("role").IsRequired();
    }
}
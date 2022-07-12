using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Config
{
    public class UserRoleEntityConfiguration : BaseEntityConfiguration<UserRoleEntity>
    {
        protected override void ConfigureModel(EntityTypeBuilder<UserRoleEntity> builder)
        {
            builder.ToTable("user_role").HasKey(p => p.UserId).HasName("user_role_pkey");
            builder.Property(p => p.UserId).HasColumnName("user_id").ValueGeneratedNever().IsRequired();
            builder.Property(p => p.Role).HasColumnName("role").IsRequired();
        }
    }
}
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class NexusModsUserToRoleEntityConfiguration : BaseEntityConfigurationWithTenant<NexusModsUserToRoleEntity>
{
    public NexusModsUserToRoleEntityConfiguration(ITenantContextAccessor tenantContextAccessor) : base(tenantContextAccessor) { }

    protected override void ConfigureModel(EntityTypeBuilder<NexusModsUserToRoleEntity> builder)
    {
        builder.Property<int>(nameof(NexusModsUserEntity.NexusModsUserId)).HasColumnName("nexusmods_user_role_id").ValueGeneratedNever();
        builder.Property(x => x.Role).HasColumnName("role");
        builder.Property(x => x.TenantId).HasColumnName("tenant");
        builder.ToTable("nexusmods_user_role", "nexusmods_user").HasKey(nameof(NexusModsUserToRoleEntity.TenantId), nameof(NexusModsUserEntity.NexusModsUserId));

        builder.HasOne(x => x.NexusModsUser)
            .WithMany(x => x.ToRoles)
            .HasForeignKey(nameof(NexusModsUserEntity.NexusModsUserId))
            .HasPrincipalKey(x => x.NexusModsUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.NexusModsUser).AutoInclude();
    }
}
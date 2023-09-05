using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class NexusModsUserToRoleEntityConfiguration : BaseEntityConfigurationWithTenant<NexusModsUserToRoleEntity>
{
    public NexusModsUserToRoleEntityConfiguration(ITenantContextAccessor tenantContextAccessor) : base(tenantContextAccessor) { }

    protected override void ConfigureModel(EntityTypeBuilder<NexusModsUserToRoleEntity> builder)
    {
        builder.Property<NexusModsUserId>(nameof(NexusModsUserEntity.NexusModsUserId)).HasColumnName("nexusmods_user_role_id").HasConversion<NexusModsUserId.EfCoreValueConverter>().ValueGeneratedNever();
        builder.Property(x => x.Role).HasColumnName("role").HasConversion<ApplicationRole.EfCoreValueConverter>();
        builder.Property(x => x.TenantId).HasColumnName("tenant").HasConversion<TenantId.EfCoreValueConverter>();
        builder.ToTable("nexusmods_user_role", "nexusmods_user").HasKey(nameof(NexusModsUserToRoleEntity.TenantId), nameof(NexusModsUserEntity.NexusModsUserId));

        builder.HasOne(x => x.NexusModsUser)
            .WithMany(x => x.ToRoles)
            .HasForeignKey(nameof(NexusModsUserEntity.NexusModsUserId))
            .HasPrincipalKey(x => x.NexusModsUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.NexusModsUser).AutoInclude();
    }
}
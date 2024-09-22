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
        builder.Property(x => x.NexusModsUserId).HasColumnName("nexusmods_user_role_id").HasVogenConversion().ValueGeneratedNever();
        builder.Property(x => x.Role).HasColumnName("role").HasVogenConversion();
        builder.ToTable("nexusmods_user_role", "nexusmods_user").HasKey(x => new
        {
            x.TenantId,
            x.NexusModsUserId,
        });

        builder.HasOne(x => x.NexusModsUser)
            .WithMany(x => x.ToRoles)
            .HasForeignKey(x => x.NexusModsUserId)
            .HasPrincipalKey(x => x.NexusModsUserId)
            .OnDelete(DeleteBehavior.Cascade);

        base.ConfigureModel(builder);
    }
}
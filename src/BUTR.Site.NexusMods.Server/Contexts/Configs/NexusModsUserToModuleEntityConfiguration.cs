using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class NexusModsUserToModuleEntityConfiguration : BaseEntityConfigurationWithTenant<NexusModsUserToModuleEntity>
{
    public NexusModsUserToModuleEntityConfiguration(ITenantContextAccessor tenantContextAccessor) : base(tenantContextAccessor) { }

    protected override void ConfigureModel(EntityTypeBuilder<NexusModsUserToModuleEntity> builder)
    {
        builder.Property(x => x.NexusModsUserId).HasColumnName("nexusmods_user_module_id").HasVogenConversion().ValueGeneratedNever();
        builder.Property(x => x.ModuleId).HasColumnName("module_id").HasVogenConversion();
        builder.Property(x => x.LinkType).HasColumnName("nexusmods_user_module_link_type_id");
        builder.ToTable("nexusmods_user_module", "nexusmods_user").HasKey(x => new
        {
            x.TenantId,
            x.NexusModsUserId,
            x.ModuleId,
            x.LinkType,
        });

        builder.HasOne(x => x.NexusModsUser)
            .WithMany(x => x.ToModules)
            .HasForeignKey(x => x.NexusModsUserId)
            .HasPrincipalKey(x => x.NexusModsUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Module)
            .WithMany(x => x.ToNexusModsUsers)
            .HasForeignKey(x => new { x.TenantId, x.ModuleId })
            .HasPrincipalKey(x => new { x.TenantId, x.ModuleId })
            .OnDelete(DeleteBehavior.Cascade);

        base.ConfigureModel(builder);
    }
}
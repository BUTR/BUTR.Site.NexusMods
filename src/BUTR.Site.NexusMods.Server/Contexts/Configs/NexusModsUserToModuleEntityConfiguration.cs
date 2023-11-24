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
        builder.Property<NexusModsUserId>(nameof(NexusModsUserEntity.NexusModsUserId)).HasColumnName("nexusmods_user_module_id").HasValueObjectConversion().ValueGeneratedNever();
        builder.Property<ModuleId>(nameof(ModuleEntity.ModuleId)).HasColumnName("module_id").HasValueObjectConversion();
        builder.Property(x => x.LinkType).HasColumnName("nexusmods_user_module_link_type_id");
        builder.ToTable("nexusmods_user_module", "nexusmods_user").HasKey(nameof(NexusModsUserToModuleEntity.TenantId), nameof(NexusModsUserEntity.NexusModsUserId), nameof(ModuleEntity.ModuleId), nameof(NexusModsUserToModuleEntity.LinkType));

        builder.HasOne(x => x.NexusModsUser)
            .WithMany(x => x.ToModules)
            .HasForeignKey(nameof(NexusModsUserEntity.NexusModsUserId))
            .HasPrincipalKey(x => x.NexusModsUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Module)
            .WithMany(x => x.ToNexusModsUsers)
            .HasForeignKey(nameof(NexusModsUserToModuleEntity.TenantId), nameof(ModuleEntity.ModuleId))
            .HasPrincipalKey(x => new { x.TenantId, x.ModuleId })
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.NexusModsUser).AutoInclude();
        builder.Navigation(x => x.Module).AutoInclude();

        base.ConfigureModel(builder);
    }
}
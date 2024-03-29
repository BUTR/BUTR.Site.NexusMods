using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class NexusModsModToNameEntityConfiguration : BaseEntityConfigurationWithTenant<NexusModsModToNameEntity>
{
    public NexusModsModToNameEntityConfiguration(ITenantContextAccessor tenantContextAccessor) : base(tenantContextAccessor) { }

    protected override void ConfigureModel(EntityTypeBuilder<NexusModsModToNameEntity> builder)
    {
        builder.Property<NexusModsModId>(nameof(NexusModsModEntity.NexusModsModId)).HasColumnName("nexusmods_mod_name_id").HasValueObjectConversion().ValueGeneratedNever();
        builder.Property(x => x.Name).HasColumnName("name");
        builder.ToTable("nexusmods_mod_name", "nexusmods_mod").HasKey(nameof(NexusModsModToNameEntity.TenantId), nameof(NexusModsModEntity.NexusModsModId));

        builder.HasOne(x => x.NexusModsMod)
            .WithOne(x => x.Name)
            .HasForeignKey<NexusModsModToNameEntity>(nameof(NexusModsModToNameEntity.TenantId), nameof(NexusModsModEntity.NexusModsModId))
            .HasPrincipalKey<NexusModsModEntity>(x => new { x.TenantId, x.NexusModsModId })
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.NexusModsMod).AutoInclude();

        base.ConfigureModel(builder);
    }
}
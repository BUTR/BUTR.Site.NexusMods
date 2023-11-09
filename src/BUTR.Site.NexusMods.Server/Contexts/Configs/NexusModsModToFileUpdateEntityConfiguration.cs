using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class NexusModsModToFileUpdateEntityConfiguration : BaseEntityConfigurationWithTenant<NexusModsModToFileUpdateEntity>
{
    public NexusModsModToFileUpdateEntityConfiguration(ITenantContextAccessor tenantContextAccessor) : base(tenantContextAccessor) { }

    protected override void ConfigureModel(EntityTypeBuilder<NexusModsModToFileUpdateEntity> builder)
    {
        builder.Property<NexusModsModId>(nameof(NexusModsModEntity.NexusModsModId)).HasColumnName("nexusmods_mod_file_update_id").HasVogenConversion().ValueGeneratedNever();
        builder.Property(x => x.LastCheckedDate).HasColumnName("date_of_last_check");
        builder.ToTable("nexusmods_mod_file_update", "nexusmods_mod").HasKey(nameof(NexusModsModToFileUpdateEntity.TenantId), nameof(NexusModsModEntity.NexusModsModId));

        builder.HasOne(x => x.NexusModsMod)
            .WithOne(x => x.FileUpdate)
            .HasForeignKey<NexusModsModToFileUpdateEntity>(nameof(NexusModsModToFileUpdateEntity.TenantId), nameof(NexusModsModEntity.NexusModsModId))
            .HasPrincipalKey<NexusModsModEntity>(x => new { x.TenantId, x.NexusModsModId })
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.NexusModsMod).AutoInclude();

        base.ConfigureModel(builder);
    }
}
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
        builder.Property(x => x.NexusModsModId).HasColumnName("nexusmods_mod_file_update_id").HasVogenConversion().ValueGeneratedNever();
        builder.Property(x => x.LastCheckedDate).HasColumnName("date_of_last_check");
        builder.ToTable("nexusmods_mod_file_update", "nexusmods_mod").HasKey(x => new
        {
            x.TenantId,
            x.NexusModsModId,
        });

        builder.HasOne(x => x.NexusModsMod)
            .WithOne(x => x.FileUpdate)
            .HasForeignKey<NexusModsModToFileUpdateEntity>(x => new { x.TenantId, x.NexusModsModId })
            .HasPrincipalKey<NexusModsModEntity>(x => new { x.TenantId, x.NexusModsModId })
            .OnDelete(DeleteBehavior.Cascade);

        base.ConfigureModel(builder);
    }
}
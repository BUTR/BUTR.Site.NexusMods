using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class NexusModsModEntityConfiguration : BaseEntityConfigurationWithTenant<NexusModsModEntity>
{
    public NexusModsModEntityConfiguration(ITenantContextAccessor tenantContextAccessor) : base(tenantContextAccessor) { }

    protected override void ConfigureModel(EntityTypeBuilder<NexusModsModEntity> builder)
    {
        builder.Property(x => x.NexusModsModId).HasColumnName("nexusmods_mod_id").HasConversion<NexusModsModId.EfCoreValueConverter>();
        builder.ToTable("nexusmods_mod", "nexusmods_mod").HasKey(x => new { x.TenantId, x.NexusModsModId });

        base.ConfigureModel(builder);
    }
}
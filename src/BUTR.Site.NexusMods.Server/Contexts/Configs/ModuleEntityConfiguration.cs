using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class ModuleEntityConfiguration : BaseEntityConfigurationWithTenant<ModuleEntity>
{
    public ModuleEntityConfiguration(ITenantContextAccessor tenantContextAccessor) : base(tenantContextAccessor) { }

    protected override void ConfigureModel(EntityTypeBuilder<ModuleEntity> builder)
    {
        builder.Property(x => x.ModuleId).HasColumnName("module_id").HasConversion<ModuleId.EfCoreValueConverter>().ValueGeneratedNever();
        builder.ToTable("module", "module").HasKey(x => new { x.TenantId, x.ModuleId });

        base.ConfigureModel(builder);
    }
}
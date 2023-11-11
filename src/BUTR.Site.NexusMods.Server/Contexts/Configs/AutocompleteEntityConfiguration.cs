using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class AutocompleteEntityConfiguration : BaseEntityConfigurationWithTenant<AutocompleteEntity>
{
    public AutocompleteEntityConfiguration(ITenantContextAccessor tenantContextAccessor) : base(tenantContextAccessor) { }

    protected override void ConfigureModel(EntityTypeBuilder<AutocompleteEntity> builder)
    {
        builder.Property<int>("AutocompleteId").HasColumnName("autocomplete_id").ValueGeneratedOnAdd();
        builder.Property(x => x.Type).HasColumnName("type");
        builder.Property(x => x.Value).HasColumnName("value");
        builder.ToTable("autocomplete", "autocomplete").HasKey(nameof(AutocompleteEntity.TenantId), "AutocompleteId");

        builder.HasIndex(x => x.Type);

        base.ConfigureModel(builder);
    }
}
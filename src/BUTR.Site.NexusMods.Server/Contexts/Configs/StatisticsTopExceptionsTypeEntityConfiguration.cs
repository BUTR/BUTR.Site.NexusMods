using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class StatisticsTopExceptionsTypeEntityConfiguration : BaseEntityConfigurationWithTenant<StatisticsTopExceptionsTypeEntity>
{
    public StatisticsTopExceptionsTypeEntityConfiguration(ITenantContextAccessor tenantContextAccessor) : base(tenantContextAccessor) { }

    protected override void ConfigureModel(EntityTypeBuilder<StatisticsTopExceptionsTypeEntity> builder)
    {
        builder.Property(x => x.ExceptionTypeId).HasColumnName("top_exceptions_type_id").HasVogenConversion().ValueGeneratedNever();
        builder.Property(x => x.ExceptionCount).HasColumnName("count");
        builder.ToTable("top_exceptions_type", "statistics").HasKey(x => new
        {
            x.TenantId,
            x.ExceptionTypeId,
        });

        builder.HasOne(x => x.ExceptionType)
            .WithMany(x => x.ToTopExceptionsTypes)
            .HasForeignKey(x => new { x.TenantId, x.ExceptionTypeId })
            .HasPrincipalKey(x => new { x.TenantId, x.ExceptionTypeId })
            .OnDelete(DeleteBehavior.Cascade);

        base.ConfigureModel(builder);
    }
}
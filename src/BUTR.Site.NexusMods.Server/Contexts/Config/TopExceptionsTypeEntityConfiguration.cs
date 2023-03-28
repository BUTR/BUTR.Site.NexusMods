using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Config
{
    public class TopExceptionsTypeEntityConfiguration : BaseEntityConfiguration<TopExceptionsTypeEntity>
    {
        protected override void ConfigureModel(EntityTypeBuilder<TopExceptionsTypeEntity> builder)
        {
            builder.ToTable("top_exceptions_type_entity").HasNoKey();
            builder.Property(p => p.Type).HasColumnName("type").IsRequired();
            builder.Property(p => p.Count).HasColumnName("count").IsRequired();
        }
    }
}
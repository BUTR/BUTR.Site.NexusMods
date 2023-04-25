using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Config
{
    public class AutocompleteEntityConfiguration : BaseEntityConfiguration<AutocompleteEntity>
    {
        protected override void ConfigureModel(EntityTypeBuilder<AutocompleteEntity> builder)
        {
            builder.ToTable("autocomplete_entity").HasKey(p => p.Type).HasName("autocomplete_entity_pkey");
            builder.Property(p => p.Type).HasColumnName("type").IsRequired();
            builder.Property(p => p.Values).HasColumnName("values").IsRequired();
        }
    }
}
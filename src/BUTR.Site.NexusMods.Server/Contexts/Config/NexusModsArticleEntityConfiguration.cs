using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Config
{
    public class NexusModsArticleEntityConfiguration : BaseEntityConfiguration<NexusModsArticleEntity>
    {
        protected override void ConfigureModel(EntityTypeBuilder<NexusModsArticleEntity> builder)
        {
            builder.ToTable("nexusmods_article_entity").HasKey(p => p.ArticleId).HasName("nexusmods_article_entity_pkey");
            builder.Property(p => p.ArticleId).HasColumnName("id").IsRequired();
            builder.Property(p => p.Title).HasColumnName("title").IsRequired();
            builder.Property(p => p.AuthorId).HasColumnName("author_id").IsRequired();
            builder.Property(p => p.AuthorName).HasColumnName("author_name").IsRequired();
            builder.Property(p => p.CreateDate).HasColumnName("create_date").IsRequired();
        }
    }
}
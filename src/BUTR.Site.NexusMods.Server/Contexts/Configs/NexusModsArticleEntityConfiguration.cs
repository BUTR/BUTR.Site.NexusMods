using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class NexusModsArticleEntityConfiguration : BaseEntityConfigurationWithTenant<NexusModsArticleEntity>
{
    public NexusModsArticleEntityConfiguration(ITenantContextAccessor tenantContextAccessor) : base(tenantContextAccessor) { }

    protected override void ConfigureModel(EntityTypeBuilder<NexusModsArticleEntity> builder)
    {
        builder.Property(x => x.NexusModsArticleId).HasColumnName("nexusmods_article_entity_id").HasVogenConversion().ValueGeneratedNever();
        builder.Property(x => x.Title).HasColumnName("title");
        builder.Property(x => x.NexusModsUserId).HasColumnName("author_id").HasVogenConversion();
        builder.Property(x => x.CreateDate).HasColumnName("create_date");
        builder.ToTable("nexusmods_article_entity", "nexusmods_article").HasKey(x => new
        {
            x.TenantId,
            x.NexusModsArticleId
        });

        builder.HasOne(x => x.NexusModsUser)
            .WithMany(x => x.ToArticles)
            .HasForeignKey(x => x.NexusModsUserId)
            .HasPrincipalKey(x => x.NexusModsUserId)
            .OnDelete(DeleteBehavior.Cascade);

        base.ConfigureModel(builder);
    }
}
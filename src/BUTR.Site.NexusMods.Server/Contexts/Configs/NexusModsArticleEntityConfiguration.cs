using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class NexusModsArticleEntityConfiguration : BaseEntityConfigurationWithTenant<NexusModsArticleEntity>
{
    public NexusModsArticleEntityConfiguration(ITenantContextAccessor tenantContextAccessor) : base(tenantContextAccessor) { }

    protected override void ConfigureModel(EntityTypeBuilder<NexusModsArticleEntity> builder)
    {
        builder.Property(x => x.NexusModsArticleId).HasColumnName("nexusmods_article_entity_id").HasUShortType().ValueGeneratedNever();
        builder.Property(x => x.Title).HasColumnName("title");
        builder.Property<int>(nameof(NexusModsUserEntity.NexusModsUserId)).HasColumnName("author_id");
        builder.Property(x => x.CreateDate).HasColumnName("create_date");
        builder.ToTable("nexusmods_article_entity", "nexusmods_article").HasKey(x => new { x.TenantId, x.NexusModsArticleId });

        builder.HasOne(x => x.NexusModsUser)
            .WithMany(x => x.ToArticles)
            .HasForeignKey(nameof(NexusModsUserEntity.NexusModsUserId))
            .HasPrincipalKey(x => x.NexusModsUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.NexusModsUser).AutoInclude();

        base.ConfigureModel(builder);
    }
}
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Config;

public class SteamLinkedRoleTokensEntityConfiguration : BaseEntityConfiguration<SteamLinkedRoleTokensEntity>
{
    protected override void ConfigureModel(EntityTypeBuilder<SteamLinkedRoleTokensEntity> builder)
    {
        builder.ToTable("steam_linked_role_tokens").HasKey(p => p.UserId).HasName("steam_linked_role_tokens_pkey");
        builder.Property(p => p.UserId).HasColumnName("user_id").ValueGeneratedNever().IsRequired();
        builder.Property(p => p.Data).HasColumnName("data").HasColumnType("hstore").IsRequired();
    }
}
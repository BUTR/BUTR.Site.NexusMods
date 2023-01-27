using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Config
{
    public class DiscordLinkedRoleTokensEntityConfiguration : BaseEntityConfiguration<DiscordLinkedRoleTokensEntity>
    {
        protected override void ConfigureModel(EntityTypeBuilder<DiscordLinkedRoleTokensEntity> builder)
        {
            builder.ToTable("discord_linked_role_tokens").HasKey(p => p.UserId).HasName("discord_linked_role_tokens_pkey");
            builder.Property(p => p.UserId).HasColumnName("user_id").ValueGeneratedNever().IsRequired();
            builder.Property(p => p.RefreshToken).HasColumnName("refresh_token").IsRequired();
            builder.Property(p => p.AccessToken).HasColumnName("access_token").IsRequired();
            builder.Property(p => p.AccessTokenExpiresAt).HasColumnName("access_token_expires_at").IsRequired();
        }
    }
}
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Config;

public class DiscordUserEntityConfiguration : BaseEntityConfiguration<DiscordUserEntity>
{
    protected override void ConfigureModel(EntityTypeBuilder<DiscordUserEntity> builder)
    {
        builder.ToTable("discord_user").HasKey(p => p.UserId).HasName("discord_user_pkey");
        builder.Property(p => p.UserId).HasColumnName("user_id").ValueGeneratedNever().IsRequired();
        builder.Property(p => p.AccessToken).HasColumnName("access_token").IsRequired();
        builder.Property(p => p.RefreshToken).HasColumnName("refresh_token").IsRequired();
        builder.Property(p => p.ExpiresAt).HasColumnName("expires_at").IsRequired();
    }
}
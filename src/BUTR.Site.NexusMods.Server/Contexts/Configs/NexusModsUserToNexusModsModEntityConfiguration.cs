using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class NexusModsUserToNexusModsModEntityConfiguration : BaseEntityConfigurationWithTenant<NexusModsUserToNexusModsModEntity>
{
    public NexusModsUserToNexusModsModEntityConfiguration(ITenantContextAccessor tenantContextAccessor) : base(tenantContextAccessor) { }

    protected override void ConfigureModel(EntityTypeBuilder<NexusModsUserToNexusModsModEntity> builder)
    {
        builder.Property(x => x.NexusModsUserId).HasColumnName("nexusmods_user_nexusmods_mod_id").HasVogenConversion();
        builder.Property(x => x.NexusModsModId).HasColumnName("nexusmods_mod_id").HasVogenConversion();
        builder.Property(x => x.LinkType).HasColumnName("nexusmods_user_mod_link_type_id");
        builder.ToTable("nexusmods_user_nexusmods_mod", "nexusmods_user").HasKey(x => new
        {
            x.TenantId,
            x.NexusModsUserId,
            x.NexusModsModId,
            x.LinkType,
        });

        builder.HasOne(x => x.NexusModsUser)
            .WithMany(x => x.ToNexusModsMods)
            .HasForeignKey(x => x.NexusModsUserId)
            .HasPrincipalKey(x => x.NexusModsUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.NexusModsMod)
            .WithMany(x => x.ToNexusModsUsers)
            .HasForeignKey(x => new { x.TenantId, x.NexusModsModId })
            .HasPrincipalKey(x => new { x.TenantId, x.NexusModsModId })
            .OnDelete(DeleteBehavior.Cascade);

        base.ConfigureModel(builder);
    }
}
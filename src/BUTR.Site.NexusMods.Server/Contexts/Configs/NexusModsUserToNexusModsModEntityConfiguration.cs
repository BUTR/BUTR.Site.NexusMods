using BUTR.Site.NexusMods.Server.Extensions;
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
        builder.Property<NexusModsUserId>(nameof(NexusModsUserEntity.NexusModsUserId)).HasColumnName("nexusmods_user_nexusmods_mod_id").HasVogenConversion();
        builder.Property<NexusModsModId>(nameof(NexusModsModEntity.NexusModsModId)).HasColumnName("nexusmods_mod_id").HasVogenConversion();
        builder.Property(x => x.LinkType).HasColumnName("nexusmods_user_nexusmods_mod_link_type_id");
        builder.ToTable("nexusmods_user_nexusmods_mod", "nexusmods_user").HasKey(nameof(NexusModsUserToNexusModsModEntity.TenantId), nameof(NexusModsUserEntity.NexusModsUserId), nameof(NexusModsModEntity.NexusModsModId), nameof(NexusModsUserToNexusModsModEntity.LinkType));

        builder.HasOne(x => x.NexusModsUser)
            .WithMany(x => x.ToNexusModsMods)
            .HasForeignKey(nameof(NexusModsUserEntity.NexusModsUserId))
            .HasPrincipalKey(x => x.NexusModsUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.NexusModsMod)
            .WithMany(x => x.ToNexusModsUsers)
            .HasForeignKey(nameof(NexusModsUserToNexusModsModEntity.TenantId),nameof(NexusModsModEntity.NexusModsModId))
            .HasPrincipalKey(x => new { x.TenantId, x.NexusModsModId })
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.NexusModsUser).AutoInclude();
        builder.Navigation(x => x.NexusModsMod).AutoInclude();

        base.ConfigureModel(builder);
    }
}
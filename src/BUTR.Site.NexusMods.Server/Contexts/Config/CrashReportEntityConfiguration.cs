using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Config
{
    public class CrashReportEntityConfiguration : BaseEntityConfiguration<CrashReportEntity>
    {
        protected override void ConfigureModel(EntityTypeBuilder<CrashReportEntity> builder)
        {
            builder.ToTable("crash_report_entity").HasKey(p => p.Id).HasName("crash_report_entity_pkey");
            builder.Property(p => p.Id).HasColumnName("id").ValueGeneratedOnAdd().IsRequired();
            builder.Property(p => p.GameVersion).HasColumnName("game_version").IsRequired();
            builder.Property(p => p.Exception).HasColumnName("exception").IsRequired();
            builder.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();
            builder.Property(p => p.ModIds).HasColumnName("mod_ids") /*.HasConversion<ImmutableArrayToArrayConverter<string>>()*/.IsRequired();
            builder.Property(p => p.InvolvedModIds).HasColumnName("involved_mod_ids") /*.HasConversion<ImmutableArrayToArrayConverter<string>>()*/.IsRequired();
            builder.Property(p => p.ModNexusModsIds).HasColumnName("mod_nexusmods_ids") /*.HasConversion<ImmutableArrayToArrayConverter<int>>()*/.IsRequired();
            builder.Property(p => p.Url).HasColumnName("url").IsRequired();
        }
    }
}
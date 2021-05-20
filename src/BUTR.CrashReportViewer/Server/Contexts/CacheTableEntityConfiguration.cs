using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using System;

namespace BUTR.CrashReportViewer.Server.Contexts
{
    public sealed class CacheTableEntityConfiguration : IEntityTypeConfiguration<object>
    {
        public void Configure(EntityTypeBuilder<object> builder)
        {
            builder.Property<string>("Id");
            builder.Property<byte[]>("Value");
            builder.Property<DateTimeOffset>("ExpiresAtTime");
            builder.Property<long?>("SlidingExpirationInSeconds");
            builder.Property<DateTimeOffset?>("AbsoluteExpiration");
            builder.ToTable("nexusmods_cache_entry").HasKey("Id");
        }
    }
}
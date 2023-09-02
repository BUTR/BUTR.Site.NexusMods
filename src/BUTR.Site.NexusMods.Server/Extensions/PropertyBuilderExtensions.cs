using BUTR.Site.NexusMods.Server.Contexts;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Extensions;

public static class PropertyBuilderExtensions
{
    public static PropertyBuilder<TProperty> HasUShortType<TProperty>(this PropertyBuilder<TProperty> propertyBuilder) =>
        propertyBuilder.HasColumnType("smallint").HasConversion<ShortConverter>();
}
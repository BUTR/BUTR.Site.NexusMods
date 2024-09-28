using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BUTR.Site.NexusMods.Client.Models;

public record ModStatsRootModel(
    [property: JsonPropertyName("last_updated")] DateTime LastUpdated,
    [property: JsonPropertyName("total_downloads")] int TotalDownloads,
    [property: JsonPropertyName("mod_page_views")] Dictionary<DateOnly, int> ModPageViews,
    [property: JsonPropertyName("mod_daily_counts")] Dictionary<DateOnly, int> ModDailyCounts,
    [property: JsonPropertyName("mod_monthly_unique_downloads")] Dictionary<DateOnly, int> ModMonthlyUniqueDownloads);
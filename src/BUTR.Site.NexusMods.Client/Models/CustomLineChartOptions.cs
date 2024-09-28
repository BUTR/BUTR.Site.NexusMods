using Blazorise.Charts;

using System.Text.Json.Serialization;

namespace BUTR.Site.NexusMods.Client.Models;

public class CustomLineChartOptions : LineChartOptions
{
    [JsonPropertyName("scales"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public new CustomChartScales Scales { get; set; } = default!;
}

public class CustomChartAxis : ChartAxis
{
    [JsonPropertyName("position"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Position { get; set; } = default!;
}
public class CustomChartScales : ChartScales
{
    [JsonPropertyName("y"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public new CustomChartAxis Y { get; set; } = default!;

    [JsonPropertyName("y1"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CustomChartAxis Y1 { get; set; } = default!;
}
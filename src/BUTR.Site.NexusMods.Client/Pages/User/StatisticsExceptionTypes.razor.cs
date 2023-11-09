using Blazorise.Charts;

using BUTR.Site.NexusMods.Client.Components.Grid;
using BUTR.Site.NexusMods.Client.Utils;
using BUTR.Site.NexusMods.ServerClient;

using Microsoft.AspNetCore.Components;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Pages.User;

public partial class StatisticsExceptionTypes
{
    [Inject]
    private IStatisticsClient StatisticsClient { get; set; } = default!;

    private DoughnutChart<double> doughnutChart = default!;
    private readonly DoughnutChartOptions doughnutChartOptions = new()
    {
        Responsive = true,
    };

    private DataGridInMemory<TopExceptionsEntry> _dataGridRef = default!;

    protected override async Task OnInitializedAsync()
    {
        var data = (await StatisticsClient.TopExceptionsTypesAsync()).Value ?? Array.Empty<TopExceptionsEntry>();

        _dataGridRef.Values = data;

        var labels = data.OrderByDescending(x => x.Percentage).Select(x => x.Type).ToList();
        var values = data.OrderByDescending(x => x.Percentage).Select(x => x.Percentage).ToList();
        await doughnutChart.Clear();
        await doughnutChart.AddLabelsDatasetsAndUpdate(labels, new DoughnutChartDataset<double> { Label = "Top Exception Types", Data = values, BackgroundColor = ChartUtiities.GetColors(data.Count, 0.2f).ToList(), BorderColor = ChartUtiities.GetColors(data.Count, 1f).ToList(), BorderWidth = 1 });

        await base.OnInitializedAsync();
    }
}
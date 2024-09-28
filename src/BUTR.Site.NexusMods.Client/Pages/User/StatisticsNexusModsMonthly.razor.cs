using Blazorise.Charts;
using Blazorise.Components;

using BUTR.Site.NexusMods.Client.Models;
using BUTR.Site.NexusMods.Client.Services;
using BUTR.Site.NexusMods.ServerClient;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.AspNetCore.Components;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Pages.User;

public partial class StatisticsNexusModsMonthly
{
    [Inject]
    private TenantProvider TenantProvider { get; set; } = default!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    [Inject]
    private IStatisticsClient StatisticsClient { get; set; } = default!;
    [Inject]
    private IModStatisticsClient ModStatisticsClient { get; set; } = default!;


    private int _modId;
    private string _moduleId;
    private DateOnly _from;
    private DateOnly _to;
    private bool _onlyLinkedToNexusMods;

    private ICollection<string> _modIdsAutocompleteValues = default!;

    private LineChart<double?> _lineChart = default!;
    private readonly LineChartOptions _options = new CustomLineChartOptions
    {
        Responsive = true,
        Interaction = new()
        {
            Mode = "index",
            Intersect = false,
        },
        SpanGaps = true,
        Scales = new CustomChartScales
        {
            Y = new()
            {
                Title = new()
                {
                    Display = true,
                    Text = "Monthly Count",
                },
                Position = "left",
            },
            Y1 = new()
            {
                Title = new()
                {
                    Display = true,
                    Text = "Crash Report Count",
                },
                Position = "right",
            },
        },
    };

    private async Task HandleRedraw()
    {
        if (_lineChart is null) return;
        if (_from == default || _to == default) return;
        if (_modId == 0) return;
        if (string.IsNullOrEmpty(_moduleId)) return;

        await _lineChart.Clear();

        var tenant = await TenantProvider.GetTenantAsync();
        var gameId = TenantUtils.FromTenantToNexusModsId(tenant)!.Value;
        var modStats = await ModStatisticsClient.GetModStatisticsAsync(gameId, _modId, CancellationToken.None);
        var crashReportStatistics = await StatisticsClient.GetCrashReportsPerMonthAsync(
            from: new DateTimeOffset(_from, TimeOnly.MinValue, TimeSpan.Zero),
            to: new DateTimeOffset(_to, TimeOnly.MinValue, TimeSpan.Zero),
            gameVersions: null,
            modIds: _onlyLinkedToNexusMods ? [_modId] : null,
            moduleIds: [_moduleId],
            moduleVersions: null,
            cancellationToken: CancellationToken.None);

        if (modStats is null) return;
        if (crashReportStatistics is null) return;

        var data = new Dictionary<DateOnly, (double?, double?, double?, double?)>();
        var montlhyUniqueDownloads = modStats.ModMonthlyUniqueDownloads.Select((x, i) =>
        {
            var previous = i > 0 ? modStats.ModMonthlyUniqueDownloads.ElementAt(i - 1).Value : 0;
            return new KeyValuePair<DateOnly, int>(x.Key, x.Value - previous);
        }).ToDictionary();
        var montlhyCounts = modStats.ModDailyCounts.GroupBy(x => new DateOnly(x.Key.Year, x.Key.Month, 1)).ToDictionary(x => x.Key, x => x.Sum(y => y.Value));
        var montlhyPageViews = modStats.ModPageViews.GroupBy(x => new DateOnly(x.Key.Year, x.Key.Month, 1)).ToDictionary(x => x.Key, x => x.Sum(y => y.Value));
        var crashReportCount = crashReportStatistics?.Value?.GroupBy(x => new DateOnly(x.Date.Year, x.Date.Month, 1)).ToDictionary(x => x.Key, x => x.Sum(y => y.Count)) ?? new();
        foreach (var month in GetMonthsBetween(_from, _to).OrderBy(x => x))
        {
            var uniqueDownloads = montlhyUniqueDownloads.TryGetValue(month, out var uniqueDownloadsValue) ? (double?) uniqueDownloadsValue : null;
            var counts = montlhyCounts.TryGetValue(month, out var countsValue) ? (double?) countsValue : null;
            var pageViews = montlhyPageViews.TryGetValue(month, out var pageViewsValue) ? (double?) pageViewsValue : null;
            var crashCount = crashReportCount.TryGetValue(month, out var crashCountValue) ? (double?) crashCountValue : null;
            data.Add(month, (uniqueDownloads, counts, pageViews, crashCount));
        }

        var dataSets = new List<LineChartDataset<double?>>
        {
            new()
            {
                Label = "Monthly Unique Downloads",
                Data = data.Select(x => x.Value.Item1).ToList(),
                BackgroundColor = Enumerable.Range(0, data.Count).Select(_ => ChartColor.FromRgba(255, 160, 86, 0.2f).ToString()).ToList(),
                BorderColor = Enumerable.Range(0, data.Count).Select(_ => ChartColor.FromRgba(255, 160, 86, 1f).ToString()).ToList(),
                Fill = false,
                PointRadius = 3,
                CubicInterpolationMode = "monotone",
            },
            new()
            {
                Label = "Monthly Downloads",
                Data = data.Select(x => x.Value.Item2).ToList(),
                BackgroundColor = Enumerable.Range(0, data.Count).Select(_ => ChartColor.FromRgba(157, 216, 102, 0.2f).ToString()).ToList(),
                BorderColor = Enumerable.Range(0, data.Count).Select(_ => ChartColor.FromRgba(157, 216, 102, 1f).ToString()).ToList(),
                Fill = false,
                PointRadius = 3,
                CubicInterpolationMode = "monotone",
            },
            new()
            {
                Label = "Monthly Page Views",
                Data = data.Select(x => x.Value.Item3).ToList(),
                BackgroundColor = Enumerable.Range(0, data.Count).Select(_ => ChartColor.FromRgba(246, 200, 95, 0.2f).ToString()).ToList(),
                BorderColor = Enumerable.Range(0, data.Count).Select(_ => ChartColor.FromRgba(246, 200, 95, 1f).ToString()).ToList(),
                Fill = false,
                PointRadius = 3,
                CubicInterpolationMode = "monotone",
            },
            new()
            {
                Label = "Crash Report Count",
                Data = data.Select(x => x.Value.Item4).ToList(),
                BackgroundColor = Enumerable.Range(0, data.Count).Select(_ => ChartColor.FromRgba(111, 78, 124, 0.2f).ToString()).ToList(),
                BorderColor = Enumerable.Range(0, data.Count).Select(_ => ChartColor.FromRgba(111, 78, 124, 1f).ToString()).ToList(),
                Fill = false,
                PointRadius = 3,
                CubicInterpolationMode = "monotone",
                YAxisID = "y1"
            },
        };

        await _lineChart.AddLabelsDatasetsAndUpdate(data.Select(x => x.Key.ToString("Y")).ToArray(), dataSets.ToArray());
    }

    private async Task OnHandleModuleIdReadData(AutocompleteReadDataEventArgs autocompleteReadDataEventArgs)
    {
        if (!autocompleteReadDataEventArgs.CancellationToken.IsCancellationRequested && autocompleteReadDataEventArgs.SearchValue.Length >= 3)
        {
            _modIdsAutocompleteValues = (await StatisticsClient.GetAutocompleteModuleIdsAsync(autocompleteReadDataEventArgs.SearchValue)).Value ?? Array.Empty<string>();
        }
    }

    private async void OnDatesChanged(IReadOnlyList<DateOnly?> obj)
    {
        if (obj.Count != 2) return;

        _from = obj[0] ?? DateOnly.MinValue;
        _to = obj[1] ?? DateOnly.MinValue;
        await HandleRedraw();
    }

    private async void OnModUrlChanged(string modUrl)
    {
        _modId = NexusModsUtils.TryParseModUrl(modUrl, out _, out var modId) ? (int) modId : 0;
        await HandleRedraw();
    }

    private async void OnModuleIdChanged(string moduleId)
    {
        _moduleId = moduleId;
        await HandleRedraw();
    }

    private async void OnlyLinkedToNexusModsChanged(bool onlyLinkedToNexusMods)
    {
        _onlyLinkedToNexusMods = onlyLinkedToNexusMods;
        await HandleRedraw();
    }

    private static List<DateOnly> GetMonthsBetween(DateOnly from, DateOnly to)
    {
        var months = new List<DateOnly>();
        var current = new DateOnly(from.Year, from.Month, 1);

        while (current <= to)
        {
            months.Add(current);
            current = current.AddMonths(1);
        }

        return months;
    }
}
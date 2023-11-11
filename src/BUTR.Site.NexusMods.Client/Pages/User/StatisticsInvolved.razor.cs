using Blazorise.Charts;
using Blazorise.Components;

using BUTR.Site.NexusMods.Client.Extensions;
using BUTR.Site.NexusMods.Client.Utils;
using BUTR.Site.NexusMods.ServerClient;
using BUTR.Site.NexusMods.Shared.Utils;

using Microsoft.AspNetCore.Components;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Pages.User;

public partial class StatisticsInvolved
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    [Inject]
    private IStatisticsClient StatisticsClient { get; set; } = default!;

    private ICollection<string> _gameVersionsAutocompleteValues = default!;
    private ICollection<string> _modIdsAutocompleteValues = default!;

    private List<string> _gameVersions = new();
    private List<string> _modIds = new();

    private LineChart<double?> _lineChart = default!;
    private readonly LineChartOptions _options = new()
    {
        Responsive = true,
        SpanGaps = true,
        Scales = new()
        {
            X = new()
            {
                Display = true,
                Title = new()
                {
                    Text = "Mod Versions",
                    Display = true,
                },
            },
            Y = new()
            {
                Display = true,
                Title = new()
                {
                    Text = "Stability Score in %",
                    Display = true,
                },
            },
        },
    };

    protected override async Task OnInitializedAsync()
    {
        var queries = NavigationManager.QueryString();
        _gameVersions = queries.GetValues("gameVersions")?.ToList() ?? _gameVersions;
        _modIds = queries.GetValues("modIds")?.ToList() ?? _modIds;

        await Refresh();

        await base.OnInitializedAsync();
    }

    private async Task Refresh()
    {
        if (_lineChart is null) return;

        await _lineChart.Clear();
        if (_modIds.Count == 0) return;

        var data = (await StatisticsClient.InvolvedAsync(_gameVersions, _modIds, Array.Empty<string>()))?.Value ?? Array.Empty<GameStorage>();

        var allGameVersions = data.Select(x => x.GameVersion).ToArray();
        var allModIdsWithVersions = data
            .SelectMany(x => x.Modules)
            .GroupBy(x => x.ModuleId)
            .Select(x => new { Id = x.Key, Versions = x.SelectMany(y => y.Versions).ToArray() })
            .ToDictionary(x => x.Id, x => x.Versions.Select(y => y.Version).Distinct().OrderBy(y => y, new AlphanumComparatorFast()).ToArray());

        var gameVersions = (_gameVersions.Count == 0 ? (ICollection<string>) allGameVersions : (ICollection<string>) _gameVersions);
        var modIds = (_modIds.Count == 0 ? (ICollection<string>) allModIdsWithVersions.Keys : (ICollection<string>) _modIds);
        var modIdsWithVersions = modIds.Where(x => allModIdsWithVersions.ContainsKey(x)).Select(x => new { Key = x, Value = allModIdsWithVersions[x] }).ToList();

        var backgrounds = ChartUtiities.GetColors(gameVersions.Count * modIds.Count, 0.2f).ToList();
        var borders = ChartUtiities.GetColors(gameVersions.Count * modIds.Count, 1f).ToList();

        var dataSets = new List<LineChartDataset<double?>>();
        foreach (var modId in modIds)
        {
            foreach (var gameVersion in gameVersions)
            {
                var versionScores = data
                    .Where(x => x.GameVersion == gameVersion).SelectMany(x => x.Modules)
                    .Where(x => x.ModuleId == modId).SelectMany(x => x.Versions).SelectMany(x => x.Scores)
                    .ToDictionary(x => x.Version, x => x.Score * 100);

                var values = modIdsWithVersions.SelectMany(x => x.Key == modId
                    ? x.Value.Select(modVersion => versionScores.TryGetValue(modVersion, out var val) ? val : (double?) null)
                    : x.Value.Select(_ => (double?) null)).ToList();

                dataSets.Add(new LineChartDataset<double?>
                {
                    Label = $"{gameVersion} {modId}",
                    Data = values,
                    BackgroundColor = Enumerable.Range(0, values.Count).Select(_ => backgrounds[dataSets.Count]).ToList(),
                    BorderColor = Enumerable.Range(0, values.Count).Select(_ => borders[dataSets.Count]).ToList(),
                    Fill = false,
                    PointRadius = 3,
                    CubicInterpolationMode = "monotone",
                });
            }
        }

        await _lineChart.AddLabelsDatasetsAndUpdate(modIdsWithVersions.SelectMany(x => x.Value/*.Select(y => $"{x.Key} {y}")*/).ToArray(), dataSets.ToArray());
    }

    private async Task OnHandleGameVersionReadData(AutocompleteReadDataEventArgs autocompleteReadDataEventArgs)
    {
        if (!autocompleteReadDataEventArgs.CancellationToken.IsCancellationRequested)
        {
            _gameVersionsAutocompleteValues = (await StatisticsClient.AutocompleteGameVersionAsync(autocompleteReadDataEventArgs.SearchValue)).Value ?? Array.Empty<string>();
        }
    }
    private async Task OnHandleModIdReadData(AutocompleteReadDataEventArgs autocompleteReadDataEventArgs)
    {
        if (!autocompleteReadDataEventArgs.CancellationToken.IsCancellationRequested && autocompleteReadDataEventArgs.SearchValue.Length >= 3)
        {
            _modIdsAutocompleteValues = (await StatisticsClient.AutocompleteModuleIdAsync(autocompleteReadDataEventArgs.SearchValue)).Value ?? Array.Empty<string>(); ;
        }
    }

    private async void OnGameVersionChanged(List<string> gameVersions)
    {
        _gameVersions = gameVersions;
        await Refresh();
    }
    private async void OnModIdChanged(List<string> modIds)
    {
        _modIds = modIds;
        await Refresh();
    }
}
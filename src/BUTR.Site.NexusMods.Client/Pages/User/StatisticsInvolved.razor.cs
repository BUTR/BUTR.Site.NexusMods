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

namespace BUTR.Site.NexusMods.Client.Pages.User
{
    public partial class StatisticsInvolved
    {
        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;
        [Inject]
        private IStatisticsClient StatisticsClient { get; set; } = default!;

        private ICollection<string> _gameVersionsAutocompleteValues = default!;
        private ICollection<string> _modIdsAutocompleteValues = default!;
        private ICollection<string> _modVersionsAutocompleteValues = default!;

        private List<string> _gameVersions = new();
        private List<string> _gameVersionsTexts = new();
        private List<string> _modIds = new();
        private List<string> _modIdsTexts = new();
        private List<string> _modVersions = new();
        private List<string> _modVersionsTexts = new();

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
            _modVersions = queries.GetValues("modVersions")?.ToList() ?? _modVersions;

            await Refresh();

            await base.OnInitializedAsync();
        }

        private async Task Refresh()
        {
            var data = await StatisticsClient.InvolvedAsync(_gameVersions, _modIds, _modVersions);

            var allGameVersions = data.Select(x => x.GameVersion).ToArray();
            var allModVersions = data.SelectMany(x => x.Mods).SelectMany(x => x.Versions).Select(x => x.Version).Distinct().OrderBy(x => x, new AlphanumComparatorFast()).ToArray();

            var dataSets = new List<LineChartDataset<double?>>();
            var backgrounds = ChartUtiities.GetColors(allGameVersions.Length * _modIds.Count, 0.2f).ToList();
            var borders = ChartUtiities.GetColors(allGameVersions.Length * _modIds.Count, 1f).ToList();
            foreach (var gameVersion in allGameVersions)
            {
                foreach (var modId in _modIds)
                {
                    var versionScores = data
                        .Where(x => x.GameVersion == gameVersion).SelectMany(x => x.Mods)
                        .Where(x => x.Id == modId).SelectMany(x => x.Versions)
                        .ToDictionary(x => x.Version, x => x.Score * 100);
                    var values = allModVersions.Select(modVersion => versionScores.TryGetValue(modVersion, out var val) ? val : (double?) null).ToList();

                    var background = backgrounds[dataSets.Count];
                    var border = borders[dataSets.Count];
                    dataSets.Add(new LineChartDataset<double?>
                    {
                        Label = $"{gameVersion} {modId}",
                        Data = values,
                        BackgroundColor = Enumerable.Range(0, values.Count).Select(_ => background).ToList(),
                        BorderColor = Enumerable.Range(0, values.Count).Select(_ => border).ToList(),
                        Fill = false,
                        PointRadius = 3,
                        CubicInterpolationMode = "monotone",
                    });
                }
            }
            await _lineChart.Clear();
            await _lineChart.AddLabelsDatasetsAndUpdate(allModVersions, dataSets.ToArray());
        }

        private async Task OnHandleGameVersionReadData(AutocompleteReadDataEventArgs autocompleteReadDataEventArgs)
        {
            if (!autocompleteReadDataEventArgs.CancellationToken.IsCancellationRequested)
            {
                _gameVersionsAutocompleteValues = await StatisticsClient.AutocompletegameversionAsync(autocompleteReadDataEventArgs.SearchValue) ?? Array.Empty<string>();
            }
        }
        private async Task OnHandleModIdReadData(AutocompleteReadDataEventArgs autocompleteReadDataEventArgs)
        {
            if (!autocompleteReadDataEventArgs.CancellationToken.IsCancellationRequested && autocompleteReadDataEventArgs.SearchValue.Length >= 3)
            {
                _modIdsAutocompleteValues = await StatisticsClient.AutocompletemodidAsync(autocompleteReadDataEventArgs.SearchValue) ?? Array.Empty<string>();
            }
        }
        private async Task OnHandleModVersionReadData(AutocompleteReadDataEventArgs autocompleteReadDataEventArgs)
        {
            if (!autocompleteReadDataEventArgs.CancellationToken.IsCancellationRequested)
            {
                //_modVersionsAutocompleteValues = await _statisticsClient.AutocompletemodversionAsync(autocompleteReadDataEventArgs.SearchValue) ?? Array.Empty<string>();
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
        private async void OnModVersionChanged(List<string> modVersions)
        {
            _modVersions = modVersions;
            await Refresh();
        }
    }
}
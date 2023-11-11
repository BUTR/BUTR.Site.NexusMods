using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services;

public record struct OutputFormat(string Value)
{
    public static readonly OutputFormat SideBySide = new("side-by-side");
    public static readonly OutputFormat LineByLine = new("line-by-line");
}

public record struct DiffStyle(string Value)
{
    public static readonly DiffStyle Char = new("char");
    public static readonly DiffStyle Word = new("word");
}

public record struct Matching(string Value)
{
    public static readonly Matching Lines = new("lines");
    public static readonly Matching Words = new("words");
}

public class HtmlConfiguration
{
    /// <summary>
    /// Default is "line-by-line"
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? OutputFormat { get; set; }

    /// <summary>
    /// Default is true
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? DrawFileList { get; set; }

    /// <summary>
    /// Default is "word"
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DiffStyle { get; set; }

    /// <summary>
    /// Default is "none"
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Matching { get; set; }

    /// <summary>
    /// Default is true
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? FileContentToggle { get; set; }

    /// <summary>
    /// Default is false
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Highlight { get; set; }

    /// <summary>
    /// Default is false
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Lazy { get; set; }
}

public sealed class DiffService : IAsyncDisposable
{
    private sealed class Container
    {
        private readonly Func<Task> _action;
        public Container(Func<Task> action) => _action = action;

        [JSInvokable("OnLoad")]
        public Task OnLoad() => _action();

        [JSInvokable("OnError")]
        public Task OnError() => Task.CompletedTask;
    }

    private readonly NavigationManager _navigationManager;
    private readonly TaskCompletionSource _tcs;
    private readonly Lazy<ValueTask<IJSUnmarshalledObjectReference>> _moduleTask;

    public DiffService(NavigationManager navigationManager, IJSRuntime runtime)
    {
        _navigationManager = navigationManager;
        _tcs = new();
        _moduleTask = new(() => runtime.InvokeAsync<IJSUnmarshalledObjectReference>("import", $"{navigationManager.BaseUri}js/diff2html.js"));
    }

    public async Task Initialize()
    {
        var module = await _moduleTask.Value;

        await module.InvokeVoidAsync("init", _navigationManager.BaseUri, DotNetObjectReference.Create(new Container(() =>
        {
            _tcs.TrySetResult();
            return Task.CompletedTask;
        })));
    }

    public async Task Deinitialize()
    {
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("deinit");
        }
    }

    public async Task RenderDiffToElement(string elementId, string diffContent, OutputFormat outputFormat, DiffStyle style, Matching? matching = null)
    {
        var module = await _moduleTask.Value;

        if (_tcs.Task.Status == TaskStatus.WaitingForActivation)
        {
            await Initialize();
        }
        await _tcs.Task;

        await module.InvokeVoidAsync("render", elementId, diffContent, new HtmlConfiguration
        {
            Highlight = true,
            FileContentToggle = false,

            OutputFormat = outputFormat.Value,
            DrawFileList = false,
            Matching = matching?.Value,
            DiffStyle = style.Value,
            Lazy = true,
        });
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("deinit");
            await module.DisposeAsync();
        }
    }
}
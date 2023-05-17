using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using System;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services;

public record PrismJSHighlighterState
{
    public bool IsInitialized { get; set; }
}

public sealed class PrismJSService : IAsyncDisposable
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

    public PrismJSService(NavigationManager navigationManager, IJSRuntime runtime)
    {
        _navigationManager = navigationManager;
        _tcs = new();
        _moduleTask = new(() => runtime.InvokeAsync<IJSUnmarshalledObjectReference>("import", $"{navigationManager.BaseUri}js/prismjs.js"));
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

    public async ValueTask HighlightAll()
    {
        var module = await _moduleTask.Value;

        if (_tcs.Task.Status == TaskStatus.WaitingForActivation)
        {
            await Initialize();
        }
        await _tcs.Task;

        await module.InvokeVoidAsync("highlightAll");
    }

    public async ValueTask<MarkupString> HighlightCIL(string code)
    {
        var module = await _moduleTask.Value;

        if (_tcs.Task.Status == TaskStatus.WaitingForActivation)
        {
            await Initialize();
        }
        await _tcs.Task;

        return new MarkupString(await module.InvokeAsync<string>("highlightCIL", code));
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using System;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services;

public sealed class HighlightJSService : IAsyncDisposable
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

    private readonly TaskCompletionSource _tcs;
    private readonly Lazy<ValueTask<IJSUnmarshalledObjectReference>> _moduleTask;

    public HighlightJSService(IJSRuntime runtime)
    {
        _tcs = new();
        _moduleTask = new(() => runtime.InvokeAsync<IJSUnmarshalledObjectReference>("import", "../js/highlight.js"));
    }

    public async ValueTask HighlightElement(ElementReference element)
    {
        var module = await _moduleTask.Value;

        if (_tcs.Task.Status == TaskStatus.WaitingForActivation)
        {
            await Initialize();
        }
        await _tcs.Task;

        await module.InvokeVoidAsync("render", element);
    }

    public async Task Initialize()
    {
        var module = await _moduleTask.Value;

        await module.InvokeVoidAsync("init", DotNetObjectReference.Create(new Container(() =>
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
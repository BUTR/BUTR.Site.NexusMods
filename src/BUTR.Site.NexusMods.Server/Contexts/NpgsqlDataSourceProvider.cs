using BUTR.Site.NexusMods.Server.Options;

using Microsoft.Extensions.Options;

using Npgsql;

using System.Text.Json;

namespace BUTR.Site.NexusMods.Server.Contexts;

public sealed class NpgsqlDataSourceProvider
{
    private ConnectionStringsOptions _options;
    private JsonSerializerOptions _jsonSerializerOptions;

    private NpgsqlDataSource _main;
    private NpgsqlDataSource _replica;

    public NpgsqlDataSourceProvider(IOptionsMonitor<ConnectionStringsOptions> option, IOptionsMonitor<JsonSerializerOptions> jsonSerializerOptions)
    {
        _options = option.CurrentValue;
        _jsonSerializerOptions = jsonSerializerOptions.CurrentValue;

        option.OnChange(Listener1);
        jsonSerializerOptions.OnChange(Listener2);

        _main = default!;
        _replica = default!;
        Rebuild();
    }

    private void Listener1(ConnectionStringsOptions option) => _options = option;
    private void Listener2(JsonSerializerOptions jsonSerializerOptions) => _jsonSerializerOptions = jsonSerializerOptions;

    private void Rebuild()
    {
        _main = new NpgsqlDataSourceBuilder(_options.Main)
            .ConfigureJsonOptions(_jsonSerializerOptions)
            .EnableDynamicJson()
            .Build();

        _replica = new NpgsqlDataSourceBuilder(_options.Replica)
            .ConfigureJsonOptions(_jsonSerializerOptions)
            .EnableDynamicJson()
            .Build();
    }

    public NpgsqlDataSource WriteDataSource() => _main;
    public NpgsqlDataSource ReadDataSource() => _replica;
}
using BUTR.Site.NexusMods.Server.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Npgsql;

using System;

namespace BUTR.Site.NexusMods.Server.Services.Database
{
    public sealed class MainConnectionProvider
    {
        public NpgsqlConnection Connection => new(_options.Main);

        private readonly ILogger _logger;
        private readonly ConnectionStringsOptions _options;

        public MainConnectionProvider(ILogger<MainConnectionProvider> logger, IOptions<ConnectionStringsOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options.Value ?? throw new ArgumentNullException(nameof(logger));
        }
    }
}
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.Extensions.Caching.Memory;

using System;
using System.Linq;

namespace BUTR.Site.NexusMods.Server.Services
{
    public interface IDiscordStorage
    {
        DiscordOAuthTokens? Get(int userId);
        void Upsert(int userId, DiscordOAuthTokens tokens);
    }

    public sealed class MemoryDiscordStorage : IDiscordStorage
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryDiscordStorage(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        public DiscordOAuthTokens? Get(int userId)
        {
            return _memoryCache.Get<DiscordOAuthTokens>(userId);
        }

        public void Upsert(int userId, DiscordOAuthTokens tokens)
        {
            _memoryCache.Set(userId, tokens);
        }
    }
}
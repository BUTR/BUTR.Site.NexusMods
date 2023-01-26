using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using System;
using System.Linq;

namespace BUTR.Site.NexusMods.Server.Services
{
    public interface IDiscordStorage
    {
        DiscordOAuthTokens? Get(int userId);
        void Upsert(int userId, DiscordOAuthTokens tokens);
    }
    
    public sealed class EFDiscordStorage : IDiscordStorage
    {
        private readonly AppDbContext _dbContext;

        public EFDiscordStorage(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public DiscordOAuthTokens? Get(int userId)
        {
            if (_dbContext.Set<DiscordUserEntity>().FirstOrDefault(x => x.UserId == userId) is { } user)
            {
                return new DiscordOAuthTokens(user.AccessToken, user.RefreshToken, user.ExpiresAt);
            }
            return null;
        }

        public void Upsert(int userId, DiscordOAuthTokens tokens)
        {
            _dbContext.Set<DiscordUserEntity>().Add(new DiscordUserEntity
            {
                UserId = userId,
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken,
                ExpiresAt = tokens.ExpiresAt
            });
            _dbContext.SaveChanges();
        }
    }
}
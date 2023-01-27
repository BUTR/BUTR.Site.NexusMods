using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using System;
using System.Collections.Generic;
using System.Text.Json;

namespace BUTR.Site.NexusMods.Server.Services
{
    public interface IDiscordStorage
    {
        DiscordOAuthTokens? Get(string userId);
        bool Upsert(int nexusModsUserId, string discordUserId, DiscordOAuthTokens tokens);
        bool Remove(int nexusModsUserId, string discordUserId);
    }

    public sealed class DatabaseDiscordStorage : IDiscordStorage
    {
        private readonly AppDbContext _dbContext;

        public DatabaseDiscordStorage(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public DiscordOAuthTokens? Get(string userId)
        {
            var entity = _dbContext.FirstOrDefault<DiscordLinkedRoleTokensEntity>(x => x.UserId == userId);
            return entity is not null ? new DiscordOAuthTokens(entity.AccessToken, entity.RefreshToken, entity.AccessTokenExpiresAt) : null;
        }

        public bool Upsert(int nexusModsUserId, string discordUserId, DiscordOAuthTokens tokens)
        {
            NexusModsUserToDiscordEntity? ApplyChanges1(NexusModsUserToDiscordEntity? existing) => existing switch
            {
                null => new NexusModsUserToDiscordEntity
                {
                    NexusModsId = nexusModsUserId,
                    DiscordId = discordUserId,
                },
                _ => existing with
                {
                    DiscordId = discordUserId,
                }
            };
            if (!_dbContext.AddUpdateRemoveAndSave<NexusModsUserToDiscordEntity>(x => x.NexusModsId == nexusModsUserId, ApplyChanges1))
                return false;
            
            DiscordLinkedRoleTokensEntity? ApplyChanges2(DiscordLinkedRoleTokensEntity? existing) => existing switch
            {
                null => new DiscordLinkedRoleTokensEntity
                {
                    UserId = discordUserId,
                    RefreshToken = tokens.RefreshToken,
                    AccessToken = tokens.AccessToken,
                    AccessTokenExpiresAt = tokens.ExpiresAt
                },
                _ => existing with
                {
                    RefreshToken = tokens.RefreshToken,
                    AccessToken = tokens.AccessToken,
                    AccessTokenExpiresAt = tokens.ExpiresAt
                }
            };
            if (!_dbContext.AddUpdateRemoveAndSave<DiscordLinkedRoleTokensEntity>(x => x.UserId == discordUserId, ApplyChanges2))
                return false;

            UserMetadataEntity? ApplyChanges3(UserMetadataEntity? existing) => existing switch
            {
                null => new UserMetadataEntity
                {
                    UserId = nexusModsUserId,
                    Metadata = new Dictionary<string, string>
                    {
                        {"DiscordTokens", JsonSerializer.Serialize(new DiscordUserTokens(discordUserId, tokens.AccessToken, tokens.RefreshToken, tokens.ExpiresAt))}
                    }
                },
                //_ when existing.Metadata.TryGetValue("DiscordTokens", out var json) => JsonSerializer.Deserialize<DiscordUserTokens>(json) is { } eTokens && eTokens.RefreshToken != tokens.RefreshToken
                //    ? existing
                //    : existing,
                //_ when !existing.Metadata.ContainsKey("DiscordTokens") => existing with
                //{
                //    Metadata = existing.Metadata.AddAndReturn("DiscordTokens", JsonSerializer.Serialize(new DiscordUserTokens(discordUserId, tokens.AccessToken, tokens.RefreshToken, tokens.ExpiresAt)))
                //},
                _ => existing with
                {
                    Metadata = existing.Metadata.SetAndReturn("DiscordTokens", JsonSerializer.Serialize(new DiscordUserTokens(discordUserId, tokens.AccessToken, tokens.RefreshToken, tokens.ExpiresAt)))
                },
            };
            if (!_dbContext.AddUpdateRemoveAndSave<UserMetadataEntity>(x => x.UserId == nexusModsUserId, ApplyChanges3))
                return false;

            return true;
        }

        public bool Remove(int nexusModsUserId, string discordUserId)
        {
            NexusModsUserToDiscordEntity? ApplyChanges1(NexusModsUserToDiscordEntity? existing) => existing switch
            {
                _ => null
            };
            if (!_dbContext.AddUpdateRemoveAndSave<NexusModsUserToDiscordEntity>(x => x.NexusModsId == nexusModsUserId, ApplyChanges1))
                return false;
            
            DiscordLinkedRoleTokensEntity? ApplyChanges(DiscordLinkedRoleTokensEntity? existing) => existing switch
            {
                _ => null
            };
            if (!_dbContext.AddUpdateRemoveAndSave<DiscordLinkedRoleTokensEntity>(x => x.UserId == discordUserId, ApplyChanges))
                return false;

            UserMetadataEntity? ApplyChanges2(UserMetadataEntity? existing) => existing switch
            {
                null => null,
                _ when existing.Metadata.ContainsKey("DiscordTokens") => existing with
                {
                    Metadata = existing.Metadata.RemoveAndReturn<Dictionary<string, string>, string, string>("DiscordTokens")
                },
                _ => existing
            };
            if (!_dbContext.AddUpdateRemoveAndSave<UserMetadataEntity>(x => x.UserId == nexusModsUserId, ApplyChanges2))
                return false;

            return true;
        }
    }
}
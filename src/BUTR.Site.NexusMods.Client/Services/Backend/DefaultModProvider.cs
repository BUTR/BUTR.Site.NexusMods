using BUTR.Site.NexusMods.Client.Models;
using BUTR.Site.NexusMods.ServerClient;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services
{
    public sealed class DefaultModProvider : IModProvider
    {
        private readonly IModClient _modClient;
        private readonly ITokenContainer _tokenContainer;

        public DefaultModProvider(IModClient modClient, ITokenContainer tokenContainer)
        {
            _modClient = modClient ?? throw new ArgumentNullException(nameof(modClient));
            _tokenContainer = tokenContainer ?? throw new ArgumentNullException(nameof(tokenContainer));
        }

        public async Task<ModModelPagingResponse?> GetMods(int page, int pageSize, CancellationToken ct = default)
        {
            var token = await _tokenContainer.GetTokenAsync(ct);
            if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            {
                var mods = await DemoUser.GetMods().ToListAsync(ct);
                return new ModModelPagingResponse(mods, new PagingMetadata(1, (int) Math.Ceiling((double) mods.Count / (double) pageSize), pageSize, mods.Count));
            }

            try
            {
                return await _modClient.PaginatedAsync(page, pageSize, ct);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<bool> LinkMod(int nexusModsId, CancellationToken ct = default)
        {
            var token = await _tokenContainer.GetTokenAsync(ct);
            if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            {
                var mods = await DemoUser.GetMods().ToListAsync(ct);
                if (mods.Find(m => m.ModId == nexusModsId) is null)
                {
                    mods.Add(new($"Demo Mod {nexusModsId}", nexusModsId, ImmutableArray<int>.Empty));
                    return true;
                }

                return false;
            }

            try
            {
                await _modClient.LinkAsync(nexusModsId, ct);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> UnlinkMod(int nexusModsId, CancellationToken ct = default)
        {
            var token = await _tokenContainer.GetTokenAsync(ct);
            if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            {
                var mods = await DemoUser.GetMods().ToListAsync(ct);
                if (mods.Find(m => m.ModId == nexusModsId) is { } mod)
                    return mods.Remove(mod);

                return false;
            }

            try
            {
                await _modClient.UnlinkAsync(nexusModsId, ct);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<ModNexusModsManualLinkModelPagingResponse?> GetManualLinks(int page, int pageSize, CancellationToken ct = default)
        {
            var token = await _tokenContainer.GetTokenAsync(ct);
            if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
                return new ModNexusModsManualLinkModelPagingResponse(new List<ModNexusModsManualLinkModel>(), new PagingMetadata(1, 1, pageSize, 1));

            try
            {
                return await _modClient.ManuallinkpaginatedAsync(page, pageSize, ct);
            }
            catch (Exception)
            {
                return new ModNexusModsManualLinkModelPagingResponse(new List<ModNexusModsManualLinkModel>(), new PagingMetadata(1, 1, pageSize, 1));
            }
        }
        public async Task<bool> ManualLink(string modId, int nexusModsId, CancellationToken ct = default)
        {
            var token = await _tokenContainer.GetTokenAsync(ct);
            if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
                return true;

            try
            {
                await _modClient.ManuallinkAsync(modId, nexusModsId, ct);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> ManualUnlink(string modId, CancellationToken ct = default)
        {
            var token = await _tokenContainer.GetTokenAsync(ct);
            if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
                return true;

            try
            {
                await _modClient.ManualunlinkAsync(modId, ct);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<UserAllowedModsModelPagingResponse?> GetAllowUserMods(int page, int pageSize, CancellationToken ct = default)
        {
            var token = await _tokenContainer.GetTokenAsync(ct);
            if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
                return new UserAllowedModsModelPagingResponse(new List<UserAllowedModsModel>(), new PagingMetadata(1, 1, pageSize, 1));

            try
            {
                return await _modClient.AllowmodpaginatedAsync(page, pageSize, ct);
            }
            catch (Exception)
            {
                return new UserAllowedModsModelPagingResponse(new List<UserAllowedModsModel>(), new PagingMetadata(1, 1, pageSize, 1));
            }
        }
        public async Task<bool> AllowUserMod(int userId, string modId, CancellationToken ct = default)
        {
            var token = await _tokenContainer.GetTokenAsync(ct);
            if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
                return true;

            try
            {
                await _modClient.AllowmodAsync(userId, modId, ct);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> DisallowUserMod(int userId, string modId, CancellationToken ct = default)
        {
            var token = await _tokenContainer.GetTokenAsync(ct);
            if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
                return true;

            try
            {
                await _modClient.DisallowmodAsync(userId, modId, ct);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
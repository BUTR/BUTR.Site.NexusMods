﻿using BUTR.Site.NexusMods.Shared.Models;

using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services
{
    public interface IProfileProvider
    {
        Task<ProfileModel?> GetProfileAsync(CancellationToken ct = default);
    }
}
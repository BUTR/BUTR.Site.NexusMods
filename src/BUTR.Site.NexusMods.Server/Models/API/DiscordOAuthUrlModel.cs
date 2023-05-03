using System;

namespace BUTR.Site.NexusMods.Server.Models.API;

public sealed record DiscordOAuthUrlModel(string Url, Guid State);
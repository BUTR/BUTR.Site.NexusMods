using Bannerlord.ModuleManager;

using System;

namespace BUTR.Site.NexusMods.Server.Models;

public sealed record NexusModsModFileParserResult
{
    public required ModuleInfoExtended ModuleInfo { get; init; }
    public required NexusModsFileId FileId { get; init; }
    public required DateTime Uploaded { get; init; }
}
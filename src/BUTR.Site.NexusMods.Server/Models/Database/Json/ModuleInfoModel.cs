using Bannerlord.ModuleManager;

using System.Linq;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record ModuleInfoModel
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required bool IsOfficial { get; init; }
    public required string Version { get; init; }
    public required bool IsSingleplayerModule { get; init; }
    public required bool IsMultiplayerModule { get; init; }
    public required bool IsServerModule { get; init; }
    public required string Url { get; init; }
    public required string UpdateInfo { get; init; }
    public required DependentModuleModel[] DependentModuleMetadatas { get; init; }
    public required SubModuleInfoModel[] SubModules { get; init; }

    public static ModuleInfoModel Create(ModuleInfoExtended moduleInfo) => new()
    {
        Id = moduleInfo.Id,
        Name = moduleInfo.Name,
        IsOfficial = moduleInfo.IsOfficial,
        Version = moduleInfo.Version.ToString(),
        IsSingleplayerModule = moduleInfo.IsSingleplayerModule,
        IsMultiplayerModule = moduleInfo.IsMultiplayerModule,
        IsServerModule = moduleInfo.IsServerModule,
        Url = moduleInfo.Url,
        UpdateInfo = moduleInfo.UpdateInfo,
        DependentModuleMetadatas = moduleInfo.DependenciesAllDistinct().Select(y => new DependentModuleModel
        {
            Id = y.Id,
            Type = y.IsIncompatible ? "Incompatible" : y.LoadType == LoadType.LoadAfterThis ? "LoadAfterThis" : "LoadBeforeThis",
            IsOptional = y.IsOptional,
            Version = y.Version != ApplicationVersion.Empty ? y.Version.ToString() : null,
            VersionRange = y.VersionRange != ApplicationVersionRange.Empty ? new()
            {
                Min = y.VersionRange.Min != ApplicationVersion.Empty ? y.VersionRange.Min.ToString() : null,
                Max = y.VersionRange.Max != ApplicationVersion.Empty ? y.VersionRange.Max.ToString() : null
            } : null,
        }).ToArray(),
        SubModules = moduleInfo.SubModules.Select(y => new SubModuleInfoModel
        {
            Name = y.Name,
            DLLName = y.DLLName,
            Assemblies = y.Assemblies.ToArray(),
            SubModuleClassType = y.SubModuleClassType,
            Tags = y.Tags.Select(z => new SubModuleInfoTagModel
            {
                Key = z.Key,
                Value = z.Value.ToArray(),
            }).ToArray(),
        }).ToArray(),
    };
}
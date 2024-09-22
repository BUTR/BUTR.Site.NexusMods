namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record SubModuleInfoModel
{
    public required string Name { get; init; }
    public required string DLLName { get; init; }
    public required string[] Assemblies { get; init; }
    public required string SubModuleClassType { get; init; }
    public required SubModuleInfoTagModel[] Tags { get; init; }
}
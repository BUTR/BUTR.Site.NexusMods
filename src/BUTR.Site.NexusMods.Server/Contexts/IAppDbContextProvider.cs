namespace BUTR.Site.NexusMods.Server.Contexts;

internal interface IAppDbContextProvider
{
    void Set(BaseAppDbContext dbContext);
    BaseAppDbContext Get();
}
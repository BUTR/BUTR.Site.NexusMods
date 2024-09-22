using BUTR.Site.NexusMods.Server.Models.Database;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Repositories;

public interface INexusModsArticleEntityRepositoryRead : IRepositoryRead<NexusModsArticleEntity>
{
    Task<IList<string>> GetAllModuleIdsAsync(string authorName, CancellationToken ct);
}

public interface INexusModsArticleEntityRepositoryWrite : IRepositoryWrite<NexusModsArticleEntity>, INexusModsArticleEntityRepositoryRead
{
    Task GenerateAutoCompleteForAuthorNameAsync(CancellationToken ct);
}
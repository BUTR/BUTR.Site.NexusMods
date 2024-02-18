using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Server.Models.API;

public sealed record PaginatedQuery(uint Page, uint PageSize, ICollection<Filtering>? Filters, ICollection<Sorting>? Sortings);
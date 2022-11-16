using System;

namespace BUTR.Site.NexusMods.Server.Models.API
{
    public record ExposedModModel(int Id, string[] ModIds, DateTimeOffset LastCheckedDate);
}
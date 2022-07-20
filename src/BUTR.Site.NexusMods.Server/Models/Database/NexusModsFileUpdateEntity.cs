using System;

namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed record NexusModsFileUpdateEntity : IEntity
    {
        public int NexusModsModId { get; set; } = default!;

        public DateTime LastCheckedDate { get; set; } = default!;
    }
}
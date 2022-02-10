using System;

namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public record ModListEntry
    {
        public Guid Id { get; set; } = default!;

        public string Name { get; set; } = default!;

        public int UserId { get; set; } = default!;

        public string Content { get; set; } = default!;
    }
}
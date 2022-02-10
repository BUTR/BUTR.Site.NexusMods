using System.Text.Json.Serialization;

namespace BUTR.Site.NexusMods.Shared.Models
{
    public record ModModel(string Name, string GameDomain, int ModId)
    {
        [JsonIgnore]
        public string Url => $"https://nexusmods.com/{GameDomain}/mods/{ModId}";
    }
}
using System.Text.Json.Serialization;

namespace BUTR.Site.NexusMods.Server.Models
{
    public record NexusModsModInfoResponse(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("picture_url")] string PictureUrl,
        [property: JsonPropertyName("mod_id")] int Id,
        [property: JsonPropertyName("version")] string Version,
        [property: JsonPropertyName("created_timestamp")] long CreatedTimestamp,
        [property: JsonPropertyName("updated_timestamp")] long UpdatedTimestamp,
        [property: JsonPropertyName("author")] string Author,
        [property: JsonPropertyName("uploaded_by")] string UploadedBy,
        [property: JsonPropertyName("status")] string Status,
        [property: JsonPropertyName("available")] bool Available,
        [property: JsonPropertyName("user")] NexusModsModInfoResponse.UserModel User
    )
    {
        public record UserModel(
            [property: JsonPropertyName("member_id")] int Id,
            [property: JsonPropertyName("name")] string Name
        );
    }
}
using System;
using System.Text.Json.Serialization;

namespace BUTR.CrashReportViewer.Shared.Models.NexusModsAPI
{
    public class NexusModsModInfoResponse
    {
        public class NexusModsUser
        {
            [JsonPropertyName("member_id")]
            public int MemberId { get; set; } = default!;

            [JsonPropertyName("member_group_id")]
            public int MemberGroupId { get; set; } = default!;

            [JsonPropertyName("name")]
            public string Name { get; set; } = default!;
        }

        public class NexusModsEndorsement
        {
            [JsonPropertyName("endorse_status")]
            public string EndorseStatus { get; set; } = default!;

            [JsonPropertyName("timestamp")]
            public object Timestamp { get; set; } = default!;

            [JsonPropertyName("version")]
            public object Version { get; set; } = default!;
        }

        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;

        [JsonPropertyName("summary")]
        public string Summary { get; set; } = default!;

        [JsonPropertyName("description")]
        public string Description { get; set; } = default!;

        [JsonPropertyName("picture_url")]
        public string PictureUrl { get; set; } = default!;

        [JsonPropertyName("uid")]
        public long Uid { get; set; } = default!;

        [JsonPropertyName("mod_id")]
        public int ModId { get; set; } = default!;

        [JsonPropertyName("game_id")]
        public int GameId { get; set; } = default!;

        [JsonPropertyName("allow_rating")]
        public bool AllowRating { get; set; } = default!;

        [JsonPropertyName("domain_name")]
        public string DomainName { get; set; } = default!;

        [JsonPropertyName("category_id")]
        public int CategoryId { get; set; } = default!;

        [JsonPropertyName("version")]
        public string Version { get; set; } = default!;

        [JsonPropertyName("endorsement_count")]
        public int EndorsementCount { get; set; } = default!;

        [JsonPropertyName("created_timestamp")]
        public int CreatedTimestamp { get; set; } = default!;

        [JsonPropertyName("created_time")]
        public DateTime CreatedTime { get; set; } = default!;

        [JsonPropertyName("updated_timestamp")]
        public int UpdatedTimestamp { get; set; } = default!;

        [JsonPropertyName("updated_time")]
        public DateTime UpdatedTime { get; set; } = default!;

        [JsonPropertyName("author")]
        public string Author { get; set; } = default!;

        [JsonPropertyName("uploaded_by")]
        public string UploadedBy { get; set; } = default!;

        [JsonPropertyName("uploaded_users_profile_url")]
        public string UploadedUsersProfileUrl { get; set; } = default!;

        [JsonPropertyName("contains_adult_content")]
        public bool ContainsAdultContent { get; set; } = default!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = default!;

        [JsonPropertyName("available")]
        public bool Available { get; set; } = default!;

        [JsonPropertyName("user")]
        public NexusModsUser User { get; set; } = default!;

        [JsonPropertyName("endorsement")]
        public NexusModsEndorsement Endorsement { get; set; } = default!;
    }
}
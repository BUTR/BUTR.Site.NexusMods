using System.Text.Json;
using System.Text.Json.Serialization;

namespace BUTR.Site.NexusMods.ServerClient
{
    public partial class AuthenticationClient
    {
        public AuthenticationClient(HttpClient client, JsonSerializerOptions options) : this(client)
        {
            _settings = new Lazy<JsonSerializerOptions>(options);
        }
    }

    public partial class CrashReportsClient
    {
        public CrashReportsClient(HttpClient client, JsonSerializerOptions options) : this(client)
        {
            _settings = new Lazy<JsonSerializerOptions>(options);
        }
    }

    public partial class ModClient
    {
        public ModClient(HttpClient client, JsonSerializerOptions options) : this(client)
        {
            _settings = new Lazy<JsonSerializerOptions>(options);
        }
    }

    public partial class ReportsClient
    {
        public ReportsClient(HttpClient client, JsonSerializerOptions options) : this(client)
        {
            _settings = new Lazy<JsonSerializerOptions>(options);
        }
    }

    public partial class UserClient
    {
        public UserClient(HttpClient client, JsonSerializerOptions options) : this(client)
        {
            _settings = new Lazy<JsonSerializerOptions>(options);
        }
    }

    public partial record CrashReportModel
    {
        [JsonIgnore]
        public string? Type => Exception.Split(Environment.NewLine).FirstOrDefault(l => l.Contains("Type:"))?.Split("Type:").Skip(1).FirstOrDefault();

        [JsonIgnore]
        public string ExceptionHtml => Exception.Replace("\r", "<br/>").Replace("\r\n", "<br/>");
    }

    public partial record ModModel
    {
        [JsonIgnore]
        public string Url => $"https://nexusmods.com/mountandblade2bannerlord/mods/{ModId}";
    }

    public partial record ProfileModel
    {
        [JsonIgnore]
        public string? Url => UserId != -1 ? $"https://nexusmods.com/users/{UserId}" : null;
    }
}
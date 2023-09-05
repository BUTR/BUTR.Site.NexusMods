using BUTR.Site.NexusMods.ServerClient.Utils;

using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.ServerClient;

public partial class AuthenticationClient
{
    public AuthenticationClient(HttpClient client, JsonSerializerOptions options) : this(client)
    {
        _settings = new Lazy<JsonSerializerOptions>(options);
    }
}

public partial interface ICrashReportsClient
{
    Task<PagingStreamingData<CrashReportModel>> PaginatedStreamingAsync(PaginatedQuery? body = null, CancellationToken cancellationToken = default);
}

public partial class CrashReportsClient
{
    public CrashReportsClient(HttpClient client, JsonSerializerOptions options) : this(client)
    {
        _settings = new Lazy<JsonSerializerOptions>(options);
    }

    protected virtual void OnPrepareRequest(HttpClient client, HttpRequestMessage request, System.Text.StringBuilder urlBuilder) { }

    partial void PrepareRequest(HttpClient client, HttpRequestMessage request, System.Text.StringBuilder urlBuilder)
    {
        OnPrepareRequest(client, request, urlBuilder);
    }

    public virtual async Task<PagingStreamingData<CrashReportModel>> PaginatedStreamingAsync(PaginatedQuery? body = null, CancellationToken cancellationToken = default)
    {
        var urlBuilder_ = new System.Text.StringBuilder();
        urlBuilder_.Append("api/v1/CrashReports/PaginatedStreaming");

        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new System.Net.Http.HttpRequestMessage())
            {
                var json_ = System.Text.Json.JsonSerializer.Serialize(body, _settings.Value);
                var content_ = new System.Net.Http.StringContent(json_);
                content_.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");
                request_.Content = content_;
                request_.Method = new System.Net.Http.HttpMethod("POST");
                request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/x-ndjson-butr-paging"));

                PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = false;
                try
                {
                    var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    ProcessResponse(client_, response_);

                    var status_ = (int) response_.StatusCode;
                    if (status_ == 200)
                    {
                        try
                        {
                            return PagingStreamingData<CrashReportModel>.Create(response_, JsonSerializerSettings, cancellationToken);
                        }
                        catch (System.Text.Json.JsonException exception)
                        {
                            var message = "Could not deserialize the response body stream as " + typeof(PagingStreamingData<CrashReportModel>).FullName + ".";
                            throw new ApiException(message, (int) response_.StatusCode, string.Empty, headers_, exception);
                        }
                    }
                    else
                    if (status_ == 401)
                    {
                        string responseText_ = (response_.Content == null) ? string.Empty : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("User not authenticated.", status_, responseText_, headers_, null);
                    }
                    else
                    if (status_ == 403)
                    {
                        string responseText_ = (response_.Content == null) ? string.Empty : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("User not authorized to access this endpoint.", status_, responseText_, headers_, null);
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }
}

public partial class NexusModsModClient
{
    public NexusModsModClient(HttpClient client, JsonSerializerOptions options) : this(client)
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

public partial class NexusModsUserClient
{
    public NexusModsUserClient(HttpClient client, JsonSerializerOptions options) : this(client)
    {
        _settings = new Lazy<JsonSerializerOptions>(options);
    }
}

public partial class GamePublicApiDiffClient
{
    public GamePublicApiDiffClient(HttpClient client, JsonSerializerOptions options) : this(client)
    {
        _settings = new Lazy<JsonSerializerOptions>(options);
    }
}


public partial class GameSourceDiffClient
{
    public GameSourceDiffClient(HttpClient client, JsonSerializerOptions options) : this(client)
    {
        _settings = new Lazy<JsonSerializerOptions>(options);
    }
}

public partial class NexusModsArticleClient
{
    public NexusModsArticleClient(HttpClient client, JsonSerializerOptions options) : this(client)
    {
        _settings = new Lazy<JsonSerializerOptions>(options);
    }
}

public partial class ExposedModsClient
{
    public ExposedModsClient(HttpClient client, JsonSerializerOptions options) : this(client)
    {
        _settings = new Lazy<JsonSerializerOptions>(options);
    }
}

public partial class DiscordClient
{
    public DiscordClient(HttpClient client, JsonSerializerOptions options) : this(client)
    {
        _settings = new Lazy<JsonSerializerOptions>(options);
    }
}

public partial class SteamClient
{
    public SteamClient(HttpClient client, JsonSerializerOptions options) : this(client)
    {
        _settings = new Lazy<JsonSerializerOptions>(options);
    }
}

public partial class GOGClient
{
    public GOGClient(HttpClient client, JsonSerializerOptions options) : this(client)
    {
        _settings = new Lazy<JsonSerializerOptions>(options);
    }
}

public partial class StatisticsClient
{
    public StatisticsClient(HttpClient client, JsonSerializerOptions options) : this(client)
    {
        _settings = new Lazy<JsonSerializerOptions>(options);
    }
}

public partial class QuartzClient
{
    public QuartzClient(HttpClient client, JsonSerializerOptions options) : this(client)
    {
        _settings = new Lazy<JsonSerializerOptions>(options);
    }
}

public partial class RecreateStacktraceClient
{
    public RecreateStacktraceClient(HttpClient client, JsonSerializerOptions options) : this(client)
    {
        _settings = new Lazy<JsonSerializerOptions>(options);
    }
}

public partial record CrashReportModel
{
    [JsonIgnore]
    public string ExceptionHtml => Exception.Replace("\r", "<br/>").Replace("\r\n", "<br/>");
}

public partial record NexusModsModModel
{
    public string Url(string gameDomain) => $"https://nexusmods.com/{gameDomain}/mods/{NexusModsModId}";
}

public partial record ProfileModel
{
    public string Url() => $"https://nexusmods.com/users/{NexusModsUserId}";
}

public partial record NexusModsArticleModel
{
    public string Url(string gameDomain) => $"https://nexusmods.com/{gameDomain}/articles/{NexusModsArticleId}";

    public string AuthorUrl() => $"https://nexusmods.com/users/{NexusModsUserId}";
}

public partial record ExposedNexusModsModModel
{
    public string Url(string gameDomain) => $"https://nexusmods.com/{gameDomain}/mods/{NexusModsModId}";
}

public partial record PagingMetadata
{
    public static PagingMetadata Empty => new(1, 1, 0, 0);
}

public partial record PagingAdditionalMetadata
{
    public static PagingAdditionalMetadata Empty => new(0);
}
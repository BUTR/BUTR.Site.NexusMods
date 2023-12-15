using BUTR.Site.NexusMods.ServerClient.Utils;

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
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
    Task<PagingStreamingData<CrashReportModel2>> PaginatedStreamingAsync(PaginatedQuery body, CancellationToken ct = default);
}

public partial class CrashReportsClient
{
    public CrashReportsClient(HttpClient client, JsonSerializerOptions options) : this(client)
    {
        _settings = new Lazy<JsonSerializerOptions>(options);
    }

    protected virtual void OnPrepareRequest(HttpClient client, HttpRequestMessage request, StringBuilder urlBuilder) { }

    partial void PrepareRequest(HttpClient client, HttpRequestMessage request, StringBuilder urlBuilder)
    {
        OnPrepareRequest(client, request, urlBuilder);
    }

    public virtual async Task<PagingStreamingData<CrashReportModel2>> PaginatedStreamingAsync(PaginatedQuery body, CancellationToken ct = default)
    {
        var urlBuilder_ = new StringBuilder();
        urlBuilder_.Append("api/v1/CrashReports/PaginatedStreaming");

        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using var request_ = new HttpRequestMessage();
            request_.Content = JsonContent.Create(body, MediaTypeHeaderValue.Parse("application/json"), _settings.Value);
            request_.Method = HttpMethod.Post;
            request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/x-ndjson-butr-paging"));

            PrepareRequest(client_, request_, urlBuilder_);

            var url_ = urlBuilder_.ToString();
            request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

            PrepareRequest(client_, request_, url_);

            var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
            var disposeResponse_ = true;
            try
            {
                var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                if (response_.Content is { Headers: not null })
                {
                    foreach (var item_ in response_.Content.Headers)
                        headers_[item_.Key] = item_.Value;
                }

                ProcessResponse(client_, response_);

                var status_ = (int) response_.StatusCode;
                switch (response_.StatusCode)
                {
                    case HttpStatusCode.OK:
                    {
                        try
                        {
                            var pagingStreamingData = PagingStreamingData<CrashReportModel2>.Create(response_, JsonSerializerSettings, ct);
                            disposeResponse_ = false;
                            return pagingStreamingData;
                        }
                        catch (JsonException exception)
                        {
                            var message = $"Could not deserialize the response body stream as {typeof(PagingStreamingData<CrashReportModel2>).FullName}.";
                            throw new ApiException(message, (int) response_.StatusCode, string.Empty, headers_, exception);
                        }
                    }
                    case HttpStatusCode.Unauthorized:
                    {
                        var responseText_ = response_.Content == null! ? string.Empty : await response_.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                        throw new ApiException("User not authenticated.", status_, responseText_, headers_, null);
                    }
                    case HttpStatusCode.Forbidden:
                    {
                        var responseText_ = response_.Content == null! ? string.Empty : await response_.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                        throw new ApiException("User not authorized to access this endpoint.", status_, responseText_, headers_, null);
                    }
                    default:
                    {
                        var responseData_ = response_.Content == null! ? null : await response_.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                        throw new ApiException($"The HTTP status code of the response was not expected ({status_}).", status_, responseData_, headers_, null);
                    }
                }
            }
            finally
            {
                if (disposeResponse_)
                    response_.Dispose();
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

/*
public partial class RecreateStacktraceClient
{
    public RecreateStacktraceClient(HttpClient client, JsonSerializerOptions options) : this(client)
    {
        _settings = new Lazy<JsonSerializerOptions>(options);
    }
}
*/

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
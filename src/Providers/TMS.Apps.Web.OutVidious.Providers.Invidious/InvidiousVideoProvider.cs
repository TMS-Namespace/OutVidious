using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using TMS.Apps.Web.OutVidious.Common.ProvidersCore;
using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Contracts;
using TMS.Apps.Web.OutVidious.Providers.Invidious.ApiModels;
using TMS.Apps.Web.OutVidious.Providers.Invidious.Converters;
using TMS.Apps.Web.OutVidious.Providers.Invidious.Mappers;

namespace TMS.Apps.Web.OutVidious.Providers.Invidious;

/// <summary>
/// Video provider implementation for Invidious instances.
/// </summary>
public sealed partial class InvidiousVideoProvider : VideoProviderBase
{
    private readonly JsonSerializerOptions _jsonOptions;

    public InvidiousVideoProvider(HttpClient httpClient, ILogger<InvidiousVideoProvider> logger, Uri baseUrl)
        : base(httpClient, logger, baseUrl)
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Converters = { new FlexibleStringConverter() }
        };
    }

    /// <inheritdoc />
    public override string ProviderId => "invidious";

    /// <inheritdoc />
    public override string DisplayName => "Invidious";

    /// <inheritdoc />
    public override string Description => "Privacy-focused YouTube frontend providing access to YouTube videos without tracking.";

    /// <inheritdoc />
    public override async Task<VideoInfo?> GetVideoInfoAsync(string videoId, CancellationToken cancellationToken)
    {
        ValidateVideoIdNotEmpty(videoId);

        var apiUrl = $"{BaseUrl.ToString().TrimEnd('/')}/api/v1/videos/{Uri.EscapeDataString(videoId)}";
        Logger.LogDebug("Fetching video details from Invidious: {ApiUrl}", apiUrl);

        try
        {
            var response = await HttpClient.GetAsync(apiUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning(
                    "Invidious API request failed with status {StatusCode} for video {VideoId}",
                    response.StatusCode, 
                    videoId);
                return null;
            }

            var dto = await response.Content.ReadFromJsonAsync<InvidiousVideoDetailsDto>(
                _jsonOptions, 
                cancellationToken);

            if (dto == null)
            {
                Logger.LogWarning("Invidious API returned null for video {VideoId}", videoId);
                return null;
            }

            var videoInfo = InvidiousMapper.ToVideoInfo(dto, BaseUrl);
            Logger.LogDebug("Successfully fetched and mapped video details for: {VideoId}", videoId);

            return videoInfo;
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex, "HTTP error while fetching video {VideoId} from Invidious", videoId);
            throw;
        }
        catch (JsonException ex)
        {
            Logger.LogError(ex, "JSON parsing error for video {VideoId} from Invidious", videoId);
            throw;
        }
    }

    /// <inheritdoc />
    public override Uri GetEmbedUrl(string videoId)
    {
        ValidateVideoIdNotEmpty(videoId);
        return CreateUri($"embed/{Uri.EscapeDataString(videoId)}?autoplay=1&local=true");
    }

    /// <inheritdoc />
    public override Uri GetWatchUrl(string videoId)
    {
        ValidateVideoIdNotEmpty(videoId);
        return CreateUri($"watch?v={Uri.EscapeDataString(videoId)}");
    }

    /// <inheritdoc />
    public override Uri? GetDashManifestUrl(string videoId)
    {
        ValidateVideoIdNotEmpty(videoId);
        // Use local=true for proxying through Invidious (avoids CORS issues)
        // Use unique_res=1 to ensure unique resolutions in the manifest
        return CreateUri($"api/manifest/dash/id/{Uri.EscapeDataString(videoId)}?local=true&unique_res=1");
    }

    /// <inheritdoc />
    public override Uri? GetHlsManifestUrl(string videoId)
    {
        ValidateVideoIdNotEmpty(videoId);
        return CreateUri($"api/manifest/hls_variant/{Uri.EscapeDataString(videoId)}");
    }

    /// <inheritdoc />
    public override Uri? GetProxiedDashManifestUrl(string videoId)
    {
        ValidateVideoIdNotEmpty(videoId);
        // Returns a local proxy endpoint to avoid CORS issues
        return new Uri($"/api/proxy/dash/{Uri.EscapeDataString(videoId)}", UriKind.Relative);
    }

    /// <inheritdoc />
    public override bool IsValidVideoId(string videoId)
    {
        if (string.IsNullOrWhiteSpace(videoId))
        {
            return false;
        }

        // YouTube video IDs are 11 characters, containing letters, numbers, underscores, and hyphens
        return YoutubeVideoIdRegex().IsMatch(videoId);
    }

    [GeneratedRegex(@"^[a-zA-Z0-9_-]{11}$")]
    private static partial Regex YoutubeVideoIdRegex();
}

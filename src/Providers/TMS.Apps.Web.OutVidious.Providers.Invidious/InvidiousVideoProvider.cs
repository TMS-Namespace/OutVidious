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

    /// <inheritdoc />
    public override async Task<ChannelDetails?> GetChannelDetailsAsync(string channelId, CancellationToken cancellationToken)
    {
        ValidateChannelIdNotEmpty(channelId);

        var apiUrl = $"{BaseUrl.ToString().TrimEnd('/')}/api/v1/channels/{Uri.EscapeDataString(channelId)}";
        Logger.LogDebug("Fetching channel details from Invidious: {ApiUrl}", apiUrl);

        try
        {
            var response = await HttpClient.GetAsync(apiUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning(
                    "Invidious API request failed with status {StatusCode} for channel {ChannelId}",
                    response.StatusCode,
                    channelId);
                return null;
            }

            var dto = await response.Content.ReadFromJsonAsync<InvidiousChannelDto>(
                _jsonOptions,
                cancellationToken);

            if (dto == null)
            {
                Logger.LogWarning("Invidious API returned null for channel {ChannelId}", channelId);
                return null;
            }

            var channelDetails = ChannelMapper.ToChannelDetails(dto, BaseUrl);
            Logger.LogDebug("Successfully fetched and mapped channel details for: {ChannelId}", channelId);

            return channelDetails;
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex, "HTTP error while fetching channel {ChannelId} from Invidious", channelId);
            throw;
        }
        catch (JsonException ex)
        {
            Logger.LogError(ex, "JSON parsing error for channel {ChannelId} from Invidious", channelId);
            throw;
        }
    }

    /// <inheritdoc />
    public override async Task<ChannelVideoPage?> GetChannelVideosAsync(
        string channelId,
        string? tabId,
        string? continuationToken,
        CancellationToken cancellationToken)
    {
        ValidateChannelIdNotEmpty(channelId);

        // Default to "videos" tab if not specified
        var tab = string.IsNullOrWhiteSpace(tabId) ? "videos" : tabId;
        
        var apiUrl = BuildChannelVideosUrl(channelId, tab, continuationToken);
        Logger.LogDebug("Fetching channel videos from Invidious: {ApiUrl}", apiUrl);

        try
        {
            var response = await HttpClient.GetAsync(apiUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning(
                    "Invidious API request failed with status {StatusCode} for channel videos {ChannelId}",
                    response.StatusCode,
                    channelId);
                return ChannelVideoPage.Empty(channelId, tab);
            }

            var dto = await response.Content.ReadFromJsonAsync<InvidiousChannelVideosResponseDto>(
                _jsonOptions,
                cancellationToken);

            if (dto == null || dto.Videos == null)
            {
                Logger.LogWarning("Invidious API returned null for channel videos {ChannelId}", channelId);
                return ChannelVideoPage.Empty(channelId, tab);
            }

            var videos = dto.Videos.Select(v => ChannelMapper.ToVideoSummary(v, BaseUrl)).ToList();

            var page = new ChannelVideoPage
            {
                ChannelId = channelId,
                Tab = tab,
                Videos = videos,
                ContinuationToken = dto.Continuation,
                TotalVideoCount = null // Invidious doesn't provide total count in paginated responses
            };

            Logger.LogDebug(
                "Successfully fetched {VideoCount} videos for channel {ChannelId}, HasMore: {HasMore}",
                videos.Count,
                channelId,
                page.HasMore);

            return page;
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex, "HTTP error while fetching channel videos {ChannelId} from Invidious", channelId);
            throw;
        }
        catch (JsonException ex)
        {
            Logger.LogError(ex, "JSON parsing error for channel videos {ChannelId} from Invidious", channelId);
            throw;
        }
    }

    /// <inheritdoc />
    public override Uri GetChannelUrl(string channelId)
    {
        ValidateChannelIdNotEmpty(channelId);
        return CreateUri($"channel/{Uri.EscapeDataString(channelId)}");
    }

    /// <inheritdoc />
    public override bool IsValidChannelId(string channelId)
    {
        if (string.IsNullOrWhiteSpace(channelId))
        {
            return false;
        }

        // YouTube channel IDs are 24 characters starting with "UC"
        // Also accept custom handles starting with "@"
        return YoutubeChannelIdRegex().IsMatch(channelId) || 
               YoutubeChannelHandleRegex().IsMatch(channelId);
    }

    private string BuildChannelVideosUrl(string channelId, string tab, string? continuationToken)
    {
        var baseApiUrl = $"{BaseUrl.ToString().TrimEnd('/')}/api/v1/channels/{Uri.EscapeDataString(channelId)}/{tab}";

        if (!string.IsNullOrWhiteSpace(continuationToken))
        {
            return $"{baseApiUrl}?continuation={Uri.EscapeDataString(continuationToken)}";
        }

        return baseApiUrl;
    }

    [GeneratedRegex(@"^[a-zA-Z0-9_-]{11}$")]
    private static partial Regex YoutubeVideoIdRegex();

    [GeneratedRegex(@"^UC[a-zA-Z0-9_-]{22}$")]
    private static partial Regex YoutubeChannelIdRegex();

    [GeneratedRegex(@"^@[a-zA-Z0-9_.-]{3,30}$")]
    private static partial Regex YoutubeChannelHandleRegex();
}

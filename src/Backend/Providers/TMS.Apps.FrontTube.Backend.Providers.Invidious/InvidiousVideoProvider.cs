using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Configuration;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Providers.Invidious.ApiModels;
using TMS.Apps.FrontTube.Backend.Providers.Invidious.Converters;
using TMS.Apps.FrontTube.Backend.Providers.Invidious.Mappers;

namespace TMS.Apps.FrontTube.Backend.Providers.Invidious;

/// <summary>
/// Video provider implementation for Invidious instances.
/// </summary>
public sealed class InvidiousVideoProvider : ProviderBase
{
    private readonly JsonSerializerOptions _jsonOptions;

    public InvidiousVideoProvider(
        ILoggerFactory loggerFactory,
        IHttpClientFactory httpClientFactory,
        ProviderConfig config)
        : base(CreateHttpClient(config), loggerFactory.CreateLogger<InvidiousVideoProvider>(), config.BaseUri)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(config);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Converters = { new FlexibleStringConverter() }
        };
    }

    private static HttpClient CreateHttpClient(ProviderConfig config)
    {
        var handler = new HttpClientHandler();

        if (config.BypassSslValidation)
        {
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }

        var client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds)
        };

        return client;
    }

    /// <inheritdoc />
    public override string ProviderId => "invidious";

    /// <inheritdoc />
    public override string DisplayName => "Invidious";

    /// <inheritdoc />
    public override string Description => "Privacy-focused YouTube frontend providing access to YouTube videos without tracking.";

    /// <inheritdoc />
    public override async Task<Video?> GetVideoInfoAsync(string videoId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(videoId))
        {
            throw new ArgumentException("Video ID cannot be empty.", nameof(videoId));
        }

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
        if (string.IsNullOrWhiteSpace(videoId))
        {
            throw new ArgumentException("Video ID cannot be empty.", nameof(videoId));
        }
        
        return CreateUri($"embed/{Uri.EscapeDataString(videoId)}?autoplay=1&local=true");
    }

    /// <inheritdoc />
    public override async Task<Channel?> GetChannelDetailsAsync(string channelId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(channelId))
        {
            throw new ArgumentException("Channel ID cannot be empty.", nameof(channelId));
        }

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
    public override async Task<VideosPage?> GetChannelVideosAsync(
        string channelId,
        string? tabId,
        string? continuationToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(channelId))
        {
            throw new ArgumentException("Channel ID cannot be empty.", nameof(channelId));
        }

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
                return VideosPage.Empty(channelId, tab);
            }

            // TODO: log the actual response when the is error or deserialization fails
            var dto = await response.Content.ReadFromJsonAsync<InvidiousChannelVideosResponseDto>(
                _jsonOptions,
                cancellationToken);

            if (dto == null || dto.Videos == null)
            {
                Logger.LogWarning("Invidious API returned null for channel videos {ChannelId}", channelId);
                return VideosPage.Empty(channelId, tab);
            }

            var videos = dto.Videos.Select(v => ChannelMapper.ToVideoSummary(v, BaseUrl)).ToList();

            var page = new VideosPage
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

    private string BuildChannelVideosUrl(string channelId, string tab, string? continuationToken)
    {
        var baseApiUrl = $"{BaseUrl.ToString().TrimEnd('/')}/api/v1/channels/{Uri.EscapeDataString(channelId)}/{tab}";

        if (!string.IsNullOrWhiteSpace(continuationToken))
        {
            return $"{baseApiUrl}?continuation={Uri.EscapeDataString(continuationToken)}";
        }

        return baseApiUrl;
    }
}

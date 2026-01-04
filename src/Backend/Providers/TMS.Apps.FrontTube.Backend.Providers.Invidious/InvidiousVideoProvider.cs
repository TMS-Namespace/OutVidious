using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Configuration;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tools;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tools.YouTube;
using TMS.Apps.FrontTube.Backend.Providers.Invidious.ApiModels;
using TMS.Apps.FrontTube.Backend.Providers.Invidious.Converters;
using TMS.Apps.FrontTube.Backend.Providers.Invidious.Mappers;

namespace TMS.Apps.FrontTube.Backend.Providers.Invidious;

/// <summary>
/// Video provider implementation for Invidious instances.
/// </summary>
public sealed class InvidiousVideoProvider : ProviderBase
{
    private const string DefaultChannelTab = "videos";

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
    public override async Task<VideoCommon?> GetVideoAsync(RemoteIdentityCommon videoIdentity, CancellationToken cancellationToken)
    {
        var videoId = GetRemoteIdOrThrow(videoIdentity, RemoteIdentityTypeCommon.Video);

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
    public override Uri GetEmbedUrl(RemoteIdentityCommon videoIdentity)
    {
        var videoId = GetRemoteIdOrThrow(videoIdentity, RemoteIdentityTypeCommon.Video);
        
        return CreateUri($"embed/{Uri.EscapeDataString(videoId)}?autoplay=1&local=true");
    }

    /// <inheritdoc />
    public override async Task<ChannelCommon?> GetChannelAsync(RemoteIdentityCommon channelIdentity, CancellationToken cancellationToken)
    {
        var channelId = GetRemoteIdOrThrow(channelIdentity, RemoteIdentityTypeCommon.Channel);

        var apiUrl = $"{BaseUrl.ToString().TrimEnd('/')}/api/v1/channels/{Uri.EscapeDataString(channelId)}";
        Logger.LogDebug("Fetching channel details from Invidious: {ApiUrl}", apiUrl);

        string? responseContent = null;

        try
        {
            var response = await HttpClient.GetAsync(apiUrl, cancellationToken);

            responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
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
                Logger.LogWarning("Invidious API returned null for channel {ChannelId}. Response content: {ResponseContent}", channelId, responseContent);
                return null;
            }

            var channelDetails = ChannelMapper.ToChannelDetails(dto, BaseUrl);
            Logger.LogDebug("Successfully fetched and mapped channel details for: {ChannelId}", channelId);

            return channelDetails;
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex, "HTTP error while fetching channel {ChannelId} from Invidious. Response content: {ResponseContent}", channelId, responseContent);
            throw;
        }
        catch (JsonException ex)
        {
            Logger.LogError(ex, "JSON parsing error for channel {ChannelId} from Invidious. Response content: {ResponseContent}", channelId, responseContent);
            throw;
        }
    }

    /// <inheritdoc />
    public override async Task<VideosPageCommon?> GetChannelVideosTabAsync(
        RemoteIdentityCommon channelIdentity,
        string tab,
        string? continuationToken,
        CancellationToken cancellationToken)
    {
        var channelId = GetRemoteIdOrThrow(channelIdentity, RemoteIdentityTypeCommon.Channel);
        var resolvedTab = string.IsNullOrWhiteSpace(tab) ? DefaultChannelTab : tab;
        
        var apiUrl = BuildChannelVideosUrl(channelId, resolvedTab, continuationToken);
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
                return VideosPageCommon.Empty(channelIdentity, resolvedTab);
            }

            // TODO: log the actual response when the is error or deserialization fails
            var dto = await response.Content.ReadFromJsonAsync<InvidiousChannelVideosResponseDto>(
                _jsonOptions,
                cancellationToken);

            if (dto == null || dto.Videos == null)
            {
                Logger.LogWarning("Invidious API returned null for channel videos {ChannelId}", channelId);
                return VideosPageCommon.Empty(channelIdentity, resolvedTab);
            }

            var videos = dto.Videos.Select(v => ChannelMapper.ToVideoSummary(v, BaseUrl)).ToList();

            var page = new VideosPageCommon
            {
                ChannelRemoteIdentity = channelIdentity,
                Tab = resolvedTab,
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

    private static string GetRemoteIdOrThrow(RemoteIdentityCommon identity, RemoteIdentityTypeCommon expectedType)
    {
        ArgumentNullException.ThrowIfNull(identity);

        if (identity.IdentityType != expectedType)
        {
            throw new ArgumentException($"Identity type must be {expectedType}.", nameof(identity));
        }

        if (!string.IsNullOrWhiteSpace(identity.RemoteId))
        {
            return identity.RemoteId;
        }

        if (!YouTubeIdentityParser.TryParse(identity.AbsoluteRemoteUrl, out var parts))
        {
            throw new ArgumentException(
                $"Remote identity URL '{identity.AbsoluteRemoteUrl}' is not a valid YouTube URL: {string.Join(", ", parts.Errors)}.",
                nameof(identity));
        }

        if (!parts.IsSupported())
        {
            throw new ArgumentException(
                $"Remote identity URL '{identity.AbsoluteRemoteUrl}' is not supported by FrontTube, the recognized identity type is '{parts.IdentityType}'.",
                nameof(identity));
        }

        if (expectedType == RemoteIdentityTypeCommon.Video && !parts.IsVideo)
        {
            throw new ArgumentException($"Remote identity URL '{identity.AbsoluteRemoteUrl}' is not a valid video identity.", nameof(identity));
        }

        if (expectedType == RemoteIdentityTypeCommon.Channel && !parts.IsChannel)
        {
            throw new ArgumentException($"Remote identity URL '{identity.AbsoluteRemoteUrl}' is not a valid channel identity.", nameof(identity));
        }

        var remoteId = parts.PrimaryRemoteId;

        if (string.IsNullOrWhiteSpace(remoteId))
        {
            throw new ArgumentException($"Remote ID is required for {expectedType} identity.", nameof(identity));
        }

        return remoteId;
    }
}

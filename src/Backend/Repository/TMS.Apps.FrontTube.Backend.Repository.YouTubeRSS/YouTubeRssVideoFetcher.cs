using System.Diagnostics;
using System.Net;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Models;
using TMS.Apps.FrontTube.Backend.Repository.YouTubeRSS.DTOs;
using TMS.Apps.FrontTube.Backend.Repository.YouTubeRSS.Mappers;
using TMS.Apps.FrontTube.Backend.Repository.YouTubeRSS.Tools;

namespace TMS.Apps.FrontTube.Backend.Repository.YouTubeRSS;

/// <summary>
/// Fetches channel videos from YouTube using RSS/Atom feeds.
/// This is a lightweight alternative to API-based fetching that doesn't require authentication.
/// Limited to the latest ~15 videos per channel.
/// </summary>
public sealed class YouTubeRssVideoFetcher
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<YouTubeRssVideoFetcher> _logger;
    private readonly RssFeedParser _feedParser;

    public YouTubeRssVideoFetcher(
        HttpClient httpClient,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _httpClient = httpClient;
        _logger = loggerFactory.CreateLogger<YouTubeRssVideoFetcher>();
        _feedParser = new RssFeedParser(loggerFactory.CreateLogger<RssFeedParser>());
    }

    /// <summary>
    /// Fetches the latest videos for a channel identified by the given remote identity.
    /// </summary>
    /// <param name="channelIdentity">The remote identity of the channel. Must have IdentityType of Channel.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A response containing the list of video metadata for the channel.</returns>
    /// <exception cref="ArgumentException">Thrown when the identity type is not Channel.</exception>
    public async Task<JsonWebResponse<IReadOnlyList<VideoMetadataCommon>>> GetChannelVideosAsync(
        RemoteIdentityCommon channelIdentity,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(channelIdentity);

        if (channelIdentity.IdentityType != RemoteIdentityTypeCommon.Channel)
        {
            throw new ArgumentException(
                $"Expected identity type '{RemoteIdentityTypeCommon.Channel}', but got '{channelIdentity.IdentityType}'.",
                nameof(channelIdentity));
        }

        if (string.IsNullOrWhiteSpace(channelIdentity.RemoteId))
        {
            throw new ArgumentException(
                "Channel identity does not contain a valid remote ID.",
                nameof(channelIdentity));
        }

        var feedUrl = BuildFeedUrl(channelIdentity.RemoteId);
        var stopwatch = Stopwatch.StartNew();

        _logger.LogDebug(
            "[{Method}] Fetching RSS feed for channel '{ChannelId}' from '{FeedUrl}'.",
            nameof(GetChannelVideosAsync),
            channelIdentity.RemoteId,
            feedUrl);

        try
        {
            var feedResult = await FetchAndParseFeedAsync(feedUrl, cancellationToken);
            stopwatch.Stop();

            if (feedResult is null)
            {
                _logger.LogWarning(
                    "[{Method}] Failed to parse RSS feed for channel '{ChannelId}'.",
                    nameof(GetChannelVideosAsync),
                    channelIdentity.RemoteId);

                return JsonWebResponse<IReadOnlyList<VideoMetadataCommon>>.HttpError(
                    feedUrl,
                    HttpMethodType.Get,
                    HttpStatusCode.UnprocessableEntity,
                    null,
                    stopwatch.ElapsedMilliseconds);
            }

            var channelMetadata = RssFeedMapper.ToChannelMetadata(feedResult.Channel);
            var videos = feedResult.Videos
                .Select(v => RssFeedMapper.ToVideoMetadata(v, channelMetadata))
                .ToList();

            _logger.LogInformation(
                "[{Method}] Successfully fetched '{VideoCount}' videos for channel '{ChannelName}' via RSS.",
                nameof(GetChannelVideosAsync),
                videos.Count,
                channelMetadata.Name);

            return JsonWebResponse<IReadOnlyList<VideoMetadataCommon>>.Success(
                feedUrl,
                HttpMethodType.Get,
                HttpStatusCode.OK,
                null,
                videos,
                stopwatch.ElapsedMilliseconds);
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "[{Method}] Unexpected error: HTTP request failed for channel '{ChannelId}'.",
                nameof(GetChannelVideosAsync),
                channelIdentity.RemoteId);

            return JsonWebResponse<IReadOnlyList<VideoMetadataCommon>>.NetworkError(
                feedUrl,
                HttpMethodType.Get,
                ex,
                stopwatch.ElapsedMilliseconds);
        }
        catch (XmlException ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "[{Method}] Unexpected error: XML parsing failed for channel '{ChannelId}'.",
                nameof(GetChannelVideosAsync),
                channelIdentity.RemoteId);

            return JsonWebResponse<IReadOnlyList<VideoMetadataCommon>>.DeserializationError(
                feedUrl,
                HttpMethodType.Get,
                HttpStatusCode.OK,
                null,
                ex,
                stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            _logger.LogDebug(
                "[{Method}] Request cancelled for channel '{ChannelId}'.",
                nameof(GetChannelVideosAsync),
                channelIdentity.RemoteId);

            return JsonWebResponse<IReadOnlyList<VideoMetadataCommon>>.Cancelled(
                feedUrl,
                HttpMethodType.Get,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "[{Method}] Unexpected error: Failed to fetch RSS feed for channel '{ChannelId}'.",
                nameof(GetChannelVideosAsync),
                channelIdentity.RemoteId);

            return JsonWebResponse<IReadOnlyList<VideoMetadataCommon>>.NetworkError(
                feedUrl,
                HttpMethodType.Get,
                ex,
                stopwatch.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Fetches the latest videos for a channel identified by its channel ID string.
    /// </summary>
    /// <param name="channelId">The YouTube channel ID (e.g., "UC_x5XG1OV2P6uZZ5FSM9Ttw").</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A response containing the list of video metadata for the channel.</returns>
    public Task<JsonWebResponse<IReadOnlyList<VideoMetadataCommon>>> GetChannelVideosAsync(
        string channelId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(channelId);

        var identity = new RemoteIdentityCommon(RemoteIdentityTypeCommon.Channel, channelId);
        return GetChannelVideosAsync(identity, cancellationToken);
    }

    /// <summary>
    /// Builds the RSS feed URL for a given channel ID.
    /// </summary>
    /// <param name="channelId">The YouTube channel ID.</param>
    /// <returns>The complete RSS feed URL.</returns>
    private static string BuildFeedUrl(string channelId)
    {
        return $"{YouTubeRssConstants.RssFeedBaseUrl}?{YouTubeRssConstants.ChannelIdParam}={Uri.EscapeDataString(channelId)}";
    }

    /// <summary>
    /// Fetches and parses the RSS feed from the given URL.
    /// </summary>
    private async Task<RssFeedResult?> FetchAndParseFeedAsync(string feedUrl, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(feedUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        // Load as XDocument first for extended element access
        var feedXml = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);

        // Reset stream position for SyndicationFeed (need to reload)
        stream.Position = 0;

        using var xmlReader = XmlReader.Create(stream, new XmlReaderSettings
        {
            Async = true,
            DtdProcessing = DtdProcessing.Ignore
        });

        var feed = SyndicationFeed.Load(xmlReader);

        if (feed is null)
        {
            return null;
        }

        return _feedParser.Parse(feed, feedXml);
    }
}

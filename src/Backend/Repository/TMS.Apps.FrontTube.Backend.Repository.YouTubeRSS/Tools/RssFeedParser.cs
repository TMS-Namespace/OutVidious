using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Repository.YouTubeRSS.DTOs;

namespace TMS.Apps.FrontTube.Backend.Repository.YouTubeRSS.Tools;

/// <summary>
/// Parses YouTube RSS/Atom feeds into DTOs.
/// </summary>
internal sealed class RssFeedParser
{
    private static readonly XNamespace YouTubeNs = YouTubeRssConstants.YouTubeNamespace;
    private static readonly XNamespace MediaNs = YouTubeRssConstants.MediaNamespace;

    private readonly ILogger<RssFeedParser> _logger;

    public RssFeedParser(ILogger<RssFeedParser> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Parses a SyndicationFeed into an RssFeedResult DTO.
    /// </summary>
    /// <param name="feed">The syndication feed to parse.</param>
    /// <param name="feedXml">The original XML for extracting extended elements.</param>
    /// <returns>The parsed RSS feed result.</returns>
    public RssFeedResult Parse(SyndicationFeed feed, XDocument feedXml)
    {
        ArgumentNullException.ThrowIfNull(feed);
        ArgumentNullException.ThrowIfNull(feedXml);

        var channel = ParseChannel(feed, feedXml);
        var videos = ParseVideos(feed, feedXml, channel);

        _logger.LogDebug(
            "[{Method}] Parsed RSS feed for channel '{ChannelName}' with '{VideoCount}' videos.",
            nameof(Parse),
            channel.ChannelName,
            videos.Count);

        return new RssFeedResult
        {
            Channel = channel,
            Videos = videos
        };
    }

    /// <summary>
    /// Parses channel metadata from the feed header.
    /// </summary>
    private RssFeedChannel ParseChannel(SyndicationFeed feed, XDocument feedXml)
    {
        var root = feedXml.Root;
        var channelId = root?.Element(YouTubeNs + "channelId")?.Value ?? string.Empty;

        var channelLink = feed.Links
            .FirstOrDefault(l => l.RelationshipType == "alternate")?.Uri?.ToString()
            ?? $"https://www.youtube.com/channel/{channelId}";

        DateTimeOffset? updatedAt = null;
        if (feed.LastUpdatedTime != DateTimeOffset.MinValue)
        {
            updatedAt = feed.LastUpdatedTime;
        }

        return new RssFeedChannel
        {
            ChannelId = channelId,
            ChannelName = feed.Title?.Text ?? "Unknown Channel",
            ChannelUrl = channelLink,
            UpdatedAtUtc = updatedAt
        };
    }

    /// <summary>
    /// Parses video entries from the feed items.
    /// </summary>
    private List<RssFeedVideo> ParseVideos(SyndicationFeed feed, XDocument feedXml, RssFeedChannel channel)
    {
        var videos = new List<RssFeedVideo>();
        
        // Use the Atom namespace for entry elements
        XNamespace atomNs = "http://www.w3.org/2005/Atom";
        var entryElements = feedXml.Root?.Elements(atomNs + "entry").ToList() ?? [];

        var index = 0;
        foreach (var item in feed.Items)
        {
            try
            {
                var entryXml = index < entryElements.Count ? entryElements[index] : null;
                var video = ParseVideoEntry(item, entryXml, channel.ChannelId);

                if (video is not null)
                {
                    videos.Add(video);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "[{Method}] Failed to parse video entry with title '{Title}'.",
                    nameof(ParseVideos),
                    item.Title?.Text ?? "Unknown");
            }

            index++;
        }

        return videos;
    }

    /// <summary>
    /// Parses a single video entry from a syndication item.
    /// </summary>
    private RssFeedVideo? ParseVideoEntry(SyndicationItem item, XElement? entryXml, string channelId)
    {
        // Extract video ID from yt:videoId element
        var videoId = entryXml?.Element(YouTubeNs + "videoId")?.Value;

        if (string.IsNullOrWhiteSpace(videoId))
        {
            // Try extracting from the item ID (yt:video:VIDEO_ID format)
            videoId = ExtractVideoIdFromItemId(item.Id);
        }

        if (string.IsNullOrWhiteSpace(videoId))
        {
            _logger.LogWarning(
                "[{Method}] Could not extract video ID from entry with title '{Title}'.",
                nameof(ParseVideoEntry),
                item.Title?.Text ?? "Unknown");
            return null;
        }

        var videoLink = item.Links
            .FirstOrDefault(l => l.RelationshipType == "alternate")?.Uri?.ToString()
            ?? $"https://www.youtube.com/watch?v={videoId}";

        // Extract media:group content
        var mediaGroup = entryXml?.Element(MediaNs + "group");
        var description = mediaGroup?.Element(MediaNs + "description")?.Value;
        var thumbnail = ParseThumbnail(mediaGroup);
        var viewCount = ParseViewCount(mediaGroup);
        var starRating = ParseStarRating(mediaGroup);

        return new RssFeedVideo
        {
            VideoId = videoId,
            Title = item.Title?.Text ?? "Untitled",
            VideoUrl = videoLink,
            PublishedAtUtc = item.PublishDate != DateTimeOffset.MinValue 
                ? item.PublishDate 
                : DateTimeOffset.UtcNow,
            UpdatedAtUtc = item.LastUpdatedTime != DateTimeOffset.MinValue 
                ? item.LastUpdatedTime 
                : null,
            Description = description,
            Thumbnail = thumbnail,
            ViewCount = viewCount,
            StarRating = starRating
        };
    }

    /// <summary>
    /// Extracts video ID from the item ID in format "yt:video:VIDEO_ID".
    /// </summary>
    private static string? ExtractVideoIdFromItemId(string? itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return null;
        }

        const string prefix = "yt:video:";
        if (itemId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return itemId[prefix.Length..];
        }

        return null;
    }

    /// <summary>
    /// Parses thumbnail information from the media:group element.
    /// </summary>
    private static RssFeedThumbnail? ParseThumbnail(XElement? mediaGroup)
    {
        var thumbnailElement = mediaGroup?.Element(MediaNs + "thumbnail");

        if (thumbnailElement is null)
        {
            return null;
        }

        var url = thumbnailElement.Attribute("url")?.Value;
        var widthStr = thumbnailElement.Attribute("width")?.Value;
        var heightStr = thumbnailElement.Attribute("height")?.Value;

        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        // Default thumbnail dimensions if not provided
        var width = int.TryParse(widthStr, out var w) ? w : 480;
        var height = int.TryParse(heightStr, out var h) ? h : 360;

        return new RssFeedThumbnail
        {
            Url = url,
            Width = width,
            Height = height
        };
    }

    /// <summary>
    /// Parses view count from the media:community/media:statistics element.
    /// </summary>
    private static long? ParseViewCount(XElement? mediaGroup)
    {
        var communityElement = mediaGroup?.Element(MediaNs + "community");
        var statisticsElement = communityElement?.Element(MediaNs + "statistics");

        var viewsStr = statisticsElement?.Attribute("views")?.Value;

        if (long.TryParse(viewsStr, out var views))
        {
            return views;
        }

        return null;
    }

    /// <summary>
    /// Parses star rating from the media:community/media:starRating element.
    /// </summary>
    private static double? ParseStarRating(XElement? mediaGroup)
    {
        var communityElement = mediaGroup?.Element(MediaNs + "community");
        var starRatingElement = communityElement?.Element(MediaNs + "starRating");

        var averageStr = starRatingElement?.Attribute("average")?.Value;

        if (double.TryParse(averageStr, out var rating))
        {
            return rating;
        }

        return null;
    }
}

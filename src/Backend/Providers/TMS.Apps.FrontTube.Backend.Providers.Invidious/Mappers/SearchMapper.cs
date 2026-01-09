using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.Mappers;

/// <summary>
/// Maps Invidious search DTOs to common contracts.
/// </summary>
internal static class SearchMapper
{
    /// <summary>
    /// Maps search suggestions DTO to common contract.
    /// </summary>
    public static SearchSuggestionsCommon ToSearchSuggestions(SearchSuggestions dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new SearchSuggestionsCommon
        {
            Query = dto.Query,
            Suggestions = dto.Suggestions
        };
    }

    /// <summary>
    /// Maps a search video DTO to a SearchResultVideoCommon.
    /// </summary>
    public static SearchResultVideoCommon ToSearchResultVideo(SearchVideo dto, Uri baseUrl)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(baseUrl);

        var videoMetadata = new VideoMetadataCommon
        {
            RemoteIdentity = new RemoteIdentityCommon(
                RemoteIdentityTypeCommon.Video,
                dto.VideoId),
            Title = dto.Title,
            Duration = TimeSpan.FromSeconds(dto.LengthSeconds),
            ViewCount = dto.ViewCount,
            PublishedAtUtc = dto.Published > 0 ? DateTimeOffset.FromUnixTimeSeconds(dto.Published) : null,
#pragma warning disable CS0618 // Type or member is obsolete
            PublishedAgo = dto.PublishedText,
#pragma warning restore CS0618 // Type or member is obsolete
            Channel = new ChannelMetadataCommon
            {
                RemoteIdentity = new RemoteIdentityCommon(
                    RemoteIdentityTypeCommon.Channel,
                    dto.AuthorId),
                Name = dto.Author
            },
            Thumbnails = dto.VideoThumbnails.Select(ThumbnailMapper.ToThumbnailInfo).ToList(),
            IsLive = dto.LiveNow,
            IsUpcoming = dto.IsUpcoming
        };

        return new SearchResultVideoCommon
        {
            Type = SearchResultType.Video,
            Video = videoMetadata,
            Is4K = dto.Is4k ?? false,
            Is8K = dto.Is8k ?? false,
            IsVr180 = dto.IsVr180 ?? false,
            IsVr360 = dto.IsVr360 ?? false,
            Is3D = dto.Is3d ?? false,
            IsHdr = false
        };
    }

    /// <summary>
    /// Maps a search channel DTO to a SearchResultChannelCommon.
    /// </summary>
    public static SearchResultChannelCommon ToSearchResultChannel(SearchChannel dto, Uri baseUrl)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(baseUrl);

        var channelMetadata = new ChannelMetadataCommon
        {
            RemoteIdentity = new RemoteIdentityCommon(
                RemoteIdentityTypeCommon.Channel,
                dto.AuthorId),
            Name = dto.Author,
            SubscriberCount = dto.SubCount,
            Avatars = dto.AuthorThumbnails.Select(ThumbnailMapper.ToChannelThumbnailInfo).ToList()
        };

        return new SearchResultChannelCommon
        {
            Type = SearchResultType.Channel,
            Channel = channelMetadata,
            DescriptionSnippet = dto.Description,
            DescriptionHtml = dto.DescriptionHtml
        };
    }

    /// <summary>
    /// Maps a search playlist DTO to a SearchResultPlaylistCommon.
    /// </summary>
    public static SearchResultPlaylistCommon ToSearchResultPlaylist(SearchPlaylist dto, Uri baseUrl)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(baseUrl);

        return new SearchResultPlaylistCommon
        {
            Type = SearchResultType.Playlist,
            PlaylistId = dto.PlaylistId,
            Title = dto.Title,
            ThumbnailUrl = dto.PlaylistThumbnail,
            AuthorName = dto.Author,
            AuthorId = dto.AuthorId,
            VideoCount = dto.VideoCount,
            Videos = dto.Videos.Select(v => ToPlaylistVideoSummary(v, baseUrl)).ToList()
        };
    }

    /// <summary>
    /// Maps a search hashtag DTO to a SearchResultHashtagCommon.
    /// </summary>
    public static SearchResultHashtagCommon ToSearchResultHashtag(SearchHashtag dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new SearchResultHashtagCommon
        {
            Type = SearchResultType.Hashtag,
            Hashtag = dto.Title,
            Url = dto.Url,
            VideoCount = dto.VideoCount,
            ChannelCount = dto.ChannelCount
        };
    }

    private static VideoMetadataCommon ToPlaylistVideoSummary(PlaylistVideo dto, Uri baseUrl)
    {
        return new VideoMetadataCommon
        {
            RemoteIdentity = new RemoteIdentityCommon(
                RemoteIdentityTypeCommon.Video,
                dto.VideoId),
            Title = dto.Title,
            Duration = TimeSpan.FromSeconds(dto.LengthSeconds),
            Channel = new ChannelMetadataCommon
            {
                RemoteIdentity = new RemoteIdentityCommon(
                    RemoteIdentityTypeCommon.Channel,
                    dto.AuthorId),
                Name = dto.Author
            }
        };
    }
}

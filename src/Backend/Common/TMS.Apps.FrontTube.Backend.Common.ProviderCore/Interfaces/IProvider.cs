using TMS.Apps.FrontTube.Backend.Common.DataEnums.Enums;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Models;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;

/// <summary>
/// Interface for video providers that can fetch video information and generate playback URLs.
/// All providers must implement this interface to be used by the application.
/// </summary>
public interface IProvider : IProviderMetadata, IDisposable
{
    /// <summary>
    /// Gets detailed information about a video.
    /// </summary>
    /// <param name="videoIdentity">The remote identity for the video.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response containing video information or error details.</returns>
    Task<JsonWebResponse<VideoCommon?>> GetVideoAsync(RemoteIdentityCommon videoIdentity, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the embed URL for a video.
    /// </summary>
    /// <param name="videoIdentity">The remote identity for the video.</param>
    /// <returns>The embed URL.</returns>
    Uri GetEmbedVideoPlayerUri(RemoteIdentityCommon videoIdentity);

    /// <summary>
    /// Gets detailed information about a channel.
    /// </summary>
    /// <param name="channelIdentity">The remote identity for the channel.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response containing channel details or error details.</returns>
    Task<JsonWebResponse<ChannelCommon?>> GetChannelAsync(RemoteIdentityCommon channelIdentity, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a page of videos from a channel.
    /// </summary>
    /// <param name="channelIdentity">The remote identity for the channel.</param>
    /// <param name="tab">The channel tab to fetch from.</param>
    /// <param name="page">The page number (1-based). If null, uses continuation token instead.</param>
    /// <param name="continuationToken">Token for pagination. Null for the first page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response containing a page of videos with continuation token for next page or error details.</returns>
    Task<JsonWebResponse<VideosPageCommon?>> GetChannelVideosTabAsync(
        RemoteIdentityCommon channelIdentity,
        ChannelTabType tab,
        int? page,
        string? continuationToken,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets comments for a video.
    /// </summary>
    /// <param name="videoIdentity">The remote identity for the video.</param>
    /// <param name="sortBy">Sort order for comments.</param>
    /// <param name="continuationToken">Token for pagination. Null for the first page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response containing a page of comments or error details.</returns>
    Task<JsonWebResponse<CommentsPageCommon?>> GetCommentsAsync(
        RemoteIdentityCommon videoIdentity,
        CommentSortType? sortBy,
        string? continuationToken,
        CancellationToken cancellationToken);

    /// <summary>
    /// Searches for videos, channels, and playlists.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="sortBy">Sort order for results.</param>
    /// <param name="type">Filter by result type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response containing search results or error details.</returns>
    Task<JsonWebResponse<SearchResultsCommon?>> SearchAsync(
        string query,
        int page,
        SearchSortType? sortBy,
        SearchType? type,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets search suggestions for autocomplete.
    /// </summary>
    /// <param name="query">The partial search query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response containing search suggestions or error details.</returns>
    Task<JsonWebResponse<SearchSuggestionsCommon?>> GetSearchSuggestionsAsync(
        string query,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets trending videos.
    /// </summary>
    /// <param name="category">The trending category.</param>
    /// <param name="region">The region code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response containing trending videos or error details.</returns>
    Task<JsonWebResponse<TrendingVideosCommon?>> GetTrendingAsync(
        TrendingCategory category,
        RegionCode? region,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets popular videos.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response containing popular videos or error details.</returns>
    Task<JsonWebResponse<TrendingVideosCommon?>> GetPopularAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets a playlist by ID.
    /// </summary>
    /// <param name="playlistIdentity">The remote identity for the playlist.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response containing playlist details or error details.</returns>
    Task<JsonWebResponse<PlaylistCommon?>> GetPlaylistAsync(
        RemoteIdentityCommon playlistIdentity,
        int page,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets a mix (auto-generated playlist) by ID.
    /// </summary>
    /// <param name="mixIdentity">The remote identity for the mix.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response containing mix details or error details.</returns>
    Task<JsonWebResponse<PlaylistCommon?>> GetMixAsync(
        RemoteIdentityCommon mixIdentity,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets instance statistics.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response containing instance stats or error details.</returns>
    Task<JsonWebResponse<InstanceStatsCommon?>> GetInstanceStatsAsync(CancellationToken cancellationToken);
}

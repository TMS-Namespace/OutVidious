using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;
using TMS.Apps.FrontTube.Backend.Providers.Invidious.Tools;

namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.Mappers;

/// <summary>
/// Maps Invidious comment DTOs to common contracts.
/// </summary>
internal static class CommentsMapper
{
    /// <summary>
    /// Maps a comments response DTO to a CommentsPageCommon.
    /// </summary>
    public static CommentsPageCommon ToCommentsPage(CommentsResponse dto, string videoId)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentException.ThrowIfNullOrWhiteSpace(videoId);

        return new CommentsPageCommon
        {
            VideoId = videoId,
            Comments = dto.Comments.Select(ToComment).ToList(),
            ContinuationToken = dto.Continuation,
            TotalCommentCount = dto.CommentCount,
            Source = "youtube",
            VideoIdentity = new RemoteIdentityCommon(
                RemoteIdentityTypeCommon.Video,
                InvidiousHelpers.ResolveVideoUrl(videoId))
        };
    }

    /// <summary>
    /// Maps a single comment DTO to a CommentCommon.
    /// </summary>
    public static CommentCommon ToComment(Comment dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new CommentCommon
        {
            RemoteCommentId = dto.CommentId,
            AuthorName = dto.Author,
            AuthorId = dto.AuthorId,
            AuthorUrl = dto.AuthorUrl,
            RemoteIdentity = new RemoteIdentityCommon(
                RemoteIdentityTypeCommon.Comment,
                dto.CommentId),
            AuthorChannel = new ChannelMetadataCommon
            {
                RemoteIdentity = new RemoteIdentityCommon(
                    RemoteIdentityTypeCommon.Channel,
                    InvidiousHelpers.ResolveAuthorChannelUrl(dto.AuthorId)),
                Name = dto.Author,
                Avatars = dto.AuthorThumbnails
                    .Select(ThumbnailMapper.ToChannelThumbnailInfo)
                    .ToList()
            },
            Avatars = dto.AuthorThumbnails.Select(ThumbnailMapper.ToChannelThumbnailInfo).ToList(),
            IsAuthorVerified = dto.AuthorIsChannelOwner,
            IsVideoAuthor = dto.AuthorIsChannelOwner,
            IsCreatorHearted = dto.CreatorHeart is not null,
            Content = dto.Content,
            ContentHtml = dto.ContentHtml,
            LikeCount = dto.LikeCount,
            PublishedAtUTC = DateTimeOffset.FromUnixTimeSeconds(dto.Published).UtcDateTime,
            PublishedText = dto.PublishedText,
            IsPinned = dto.IsPinned,
            IsEdited = dto.IsEdited,
            ReplyCount = dto.Replies?.ReplyCount ?? 0,
            RepliesContinuationToken = dto.Replies?.Continuation
        };
    }
}

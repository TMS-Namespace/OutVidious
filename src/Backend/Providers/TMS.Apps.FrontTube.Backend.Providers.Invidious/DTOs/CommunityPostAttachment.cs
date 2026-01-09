namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Community post attachment (video, image, poll, etc).
/// </summary>
internal sealed record CommunityPostAttachment
{
    public string Type { get; init; } = string.Empty;

    // For video attachments
    public string? VideoId { get; init; }

    public string? Title { get; init; }

    public int? LengthSeconds { get; init; }

    // For image attachments
    public IReadOnlyList<VideoThumbnail> Images { get; init; } = [];

    // For poll attachments
    public IReadOnlyList<PollChoice> Choices { get; init; } = [];

    public long? TotalVotes { get; init; }
}

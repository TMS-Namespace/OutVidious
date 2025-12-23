using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

namespace TMS.Apps.FrontTube.Backend.Repository.CacheManager.Mappers;

/// <summary>
/// Maps between database entities and provider contracts for channels.
/// </summary>
public static class ChannelEntityMapper
{
    /// <summary>
    /// Default channel tabs to use when loading from database.
    /// Most YouTube channels have at least Videos tab.
    /// </summary>
    private static readonly IReadOnlyList<ChannelTab> DefaultChannelTabs =
    [
        new ChannelTab { TabId = "videos", DisplayName = "Videos", IsAvailable = true },
        new ChannelTab { TabId = "shorts", DisplayName = "Shorts", IsAvailable = true },
        new ChannelTab { TabId = "streams", DisplayName = "Live", IsAvailable = true },
        new ChannelTab { TabId = "playlists", DisplayName = "Playlists", IsAvailable = true }
    ];

    /// <summary>
    /// Converts a ChannelEntity to a ChannelDetails contract.
    /// </summary>
    public static ChannelDetails ToContract(ChannelEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new ChannelDetails
        {
            ChannelId = entity.RemoteId,
            Name = entity.Title,
            Description = entity.Description ?? string.Empty,
            DescriptionHtml = entity.DescriptionHtml,
            SubscriberCount = entity.SubscriberCount,
            SubscriberCountText = FormatSubscriberCount(entity.SubscriberCount),
            VideoCount = entity.VideoCount,
            TotalViewCount = entity.TotalViewCount,
            JoinedAt = entity.JoinedAt.HasValue
                ? new DateTimeOffset(entity.JoinedAt.Value, TimeSpan.Zero)
                : null,
            IsVerified = entity.IsVerified,
            Keywords = ParseKeywords(entity.Keywords),
            Avatars = entity.Avatars
                .Select(a => ImageEntityMapper.ToThumbnailInfo(a.Image))
                .ToList(),
            Banners = entity.Banners
                .Select(b => ImageEntityMapper.ToThumbnailInfo(b.Image))
                .ToList(),
            AvailableTabs = DefaultChannelTabs // Use default tabs when loading from DB
        };
    }

    /// <summary>
    /// Converts a ChannelEntity to a ChannelInfo contract (compact version).
    /// </summary>
    public static ChannelInfo ToChannelInfo(ChannelEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new ChannelInfo
        {
            ChannelId = entity.RemoteId,
            Name = entity.Title,
            SubscriberCount = entity.SubscriberCount,
            SubscriberCountText = FormatSubscriberCount(entity.SubscriberCount),
            Thumbnails = entity.Avatars
                .Select(a => ImageEntityMapper.ToThumbnailInfo(a.Image))
                .ToList()
        };
    }

    /// <summary>
    /// Converts a ChannelDetails contract to a ChannelEntity for database storage.
    /// </summary>
    public static ChannelEntity ToEntity(ChannelDetails contract)
    {
        ArgumentNullException.ThrowIfNull(contract);

        return new ChannelEntity
        {
            RemoteId = contract.ChannelId,
            Title = contract.Name,
            Description = contract.Description,
            DescriptionHtml = contract.DescriptionHtml,
            SubscriberCount = contract.SubscriberCount,
            VideoCount = contract.VideoCount,
            TotalViewCount = contract.TotalViewCount,
            JoinedAt = contract.JoinedAt?.UtcDateTime,
            IsVerified = contract.IsVerified,
            Keywords = JoinKeywords(contract.Keywords),
            CreatedAt = DateTime.UtcNow,
            LastSyncedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Converts a ChannelInfo contract to a ChannelEntity (for video's channel reference).
    /// </summary>
    public static ChannelEntity ToEntity(ChannelInfo contract)
    {
        ArgumentNullException.ThrowIfNull(contract);

        return new ChannelEntity
        {
            RemoteId = contract.ChannelId,
            Title = contract.Name,
            SubscriberCount = contract.SubscriberCount,
            CreatedAt = DateTime.UtcNow,
            LastSyncedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates an existing entity with data from a contract.
    /// </summary>
    public static void UpdateEntity(ChannelEntity entity, ChannelDetails contract)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(contract);

        entity.Title = contract.Name;
        entity.Description = contract.Description;
        entity.DescriptionHtml = contract.DescriptionHtml;
        entity.SubscriberCount = contract.SubscriberCount;
        entity.VideoCount = contract.VideoCount;
        entity.TotalViewCount = contract.TotalViewCount;
        entity.JoinedAt = contract.JoinedAt?.UtcDateTime;
        entity.IsVerified = contract.IsVerified;
        entity.Keywords = JoinKeywords(contract.Keywords);
        entity.LastSyncedAt = DateTime.UtcNow;
    }

    private static string? FormatSubscriberCount(long? count)
    {
        if (!count.HasValue)
        {
            return null;
        }

        return count.Value switch
        {
            >= 1_000_000_000 => $"{count.Value / 1_000_000_000.0:F1}B subscribers",
            >= 1_000_000 => $"{count.Value / 1_000_000.0:F1}M subscribers",
            >= 1_000 => $"{count.Value / 1_000.0:F1}K subscribers",
            _ => $"{count.Value:N0} subscribers"
        };
    }

    private static IReadOnlyList<string> ParseKeywords(string? keywords)
    {
        if (string.IsNullOrWhiteSpace(keywords))
        {
            return [];
        }

        return keywords.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static string? JoinKeywords(IReadOnlyList<string>? keywords)
    {
        if (keywords is null || keywords.Count == 0)
        {
            return null;
        }

        return string.Join(",", keywords);
    }
}

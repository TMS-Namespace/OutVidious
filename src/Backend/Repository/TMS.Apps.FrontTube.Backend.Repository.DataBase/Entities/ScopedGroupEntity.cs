namespace TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;

/// <summary>
/// Represents a user-defined group for organizing subscriptions.
/// Groups can be hierarchical (have parent groups).
/// </summary>
public class ScopedGroupEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Group display name/alias.
    /// </summary>
    public required string Alias { get; set; }

    /// <summary>
    /// Parent group ID for hierarchical organization. Null for root-level groups.
    /// </summary>
    public int? ParentGroupId { get; set; }

    /// <summary>
    /// User who owns this group.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// When the group was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the group was last modified.
    /// </summary>
    public DateTime ModifiedAt { get; set; }

    // Navigation properties
    public UserEntity User { get; set; } = null!;

    public ScopedGroupEntity? ParentGroup { get; set; }

    public ICollection<ScopedGroupEntity> ChildGroups { get; set; } = [];

    public ICollection<ScopedSubscriptionGroupMapEntity> SubscriptionMappings { get; set; } = [];
}

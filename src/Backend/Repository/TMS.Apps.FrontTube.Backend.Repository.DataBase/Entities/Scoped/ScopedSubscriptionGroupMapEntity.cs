namespace TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities.Scoped;

/// <summary>
/// Maps subscriptions to groups.
/// A subscription can belong to multiple groups.
/// </summary>
public class ScopedSubscriptionGroupMapEntity
{
    public int Id { get; set; }

    /// <summary>
    /// The group this subscription belongs to.
    /// </summary>
    public int GroupId { get; set; }

    /// <summary>
    /// The subscription being grouped.
    /// </summary>
    public int SubscriptionId { get; set; }

    // Navigation properties
    public ScopedGroupEntity Group { get; set; } = null!;

    public ScopedSubscriptionEntity Subscription { get; set; } = null!;
}

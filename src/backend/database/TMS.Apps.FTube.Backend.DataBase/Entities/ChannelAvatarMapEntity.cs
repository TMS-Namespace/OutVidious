namespace TMS.Apps.FTube.Backend.DataBase.Entities;

/// <summary>
/// Maps channels to their avatar images.
/// </summary>
public class ChannelAvatarMapEntity
{
    public int Id { get; set; }

    public int ChannelId { get; set; }

    public int ImageId { get; set; }

    // Navigation properties
    public ChannelEntity Channel { get; set; } = null!;

    public ImageEntity Image { get; set; } = null!;
}

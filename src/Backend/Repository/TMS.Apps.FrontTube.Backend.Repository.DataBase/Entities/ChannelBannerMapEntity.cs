namespace TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;

/// <summary>
/// Maps channels to their banner images.
/// </summary>
public class ChannelBannerMapEntity
{
    public int Id { get; set; }

    public int ChannelId { get; set; }

    public int ImageId { get; set; }

    // Navigation properties
    public ChannelEntity Channel { get; set; } = null!;

    public ImageEntity Image { get; set; } = null!;
}

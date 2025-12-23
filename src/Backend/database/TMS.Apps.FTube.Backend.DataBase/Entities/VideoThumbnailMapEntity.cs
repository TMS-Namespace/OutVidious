namespace TMS.Apps.FTube.Backend.DataBase.Entities;

/// <summary>
/// Maps videos to their thumbnail images.
/// </summary>
public class VideoThumbnailMapEntity
{
    public int Id { get; set; }

    public int VideoId { get; set; }

    public int ImageId { get; set; }

    // Navigation properties
    public VideoEntity Video { get; set; } = null!;

    public ImageEntity Image { get; set; } = null!;
}

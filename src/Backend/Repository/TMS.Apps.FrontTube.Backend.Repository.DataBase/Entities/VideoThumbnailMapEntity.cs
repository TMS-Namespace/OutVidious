using TMS.Apps.FrontTube.Backend.Repository.DataBase.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;

/// <summary>
/// Maps videos to their thumbnail images.
/// </summary>
public class VideoThumbnailMapEntity : TrackableEntitiesBase, IImageMap
{
    //public int Id { get; set; }

    public int VideoId { get; set; }

    public int ImageId { get; set; }

    // Navigation properties
    public VideoEntity Video { get; set; } = null!;

    public ImageEntity Image { get; set; } = null!;

    public static IImageMap Create(
        ImageEntity imageEntity,
        ICacheableEntity parentEntity)
    {
        if (parentEntity is not VideoEntity videoEntity)
        {
            throw new ArgumentException("Parent entity must be of type VideoEntity", nameof(parentEntity));
        }

        return new VideoThumbnailMapEntity
        {
            VideoId = videoEntity.Id,
            ImageId = imageEntity.Id,
            Video = videoEntity,
            Image = imageEntity
        };
    }
}

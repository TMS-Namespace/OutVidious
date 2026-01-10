using TMS.Apps.FrontTube.Backend.Repository.DataBase.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities.Cache;

/// <summary>
/// Maps videos to their thumbnail images.
/// </summary>
public class CacheVideoThumbnailMapEntity : EntityBase, IImageMap
{
    //public int Id { get; set; }

    public int VideoId { get; set; }

    public int ImageId { get; set; }

    // Navigation properties
    public CacheVideoEntity Video { get; set; } = null!;

    public CacheImageEntity Image { get; set; } = null!;

    public static IImageMap Create(
        CacheImageEntity imageEntity,
        ICacheableEntity parentEntity)
    {
        if (parentEntity is not CacheVideoEntity videoEntity)
        {
            throw new ArgumentException("Parent entity must be of type VideoEntity", nameof(parentEntity));
        }

        return new CacheVideoThumbnailMapEntity
        {
            VideoId = videoEntity.Id,
            ImageId = imageEntity.Id,
            Video = videoEntity,
            Image = imageEntity
        };
    }
}

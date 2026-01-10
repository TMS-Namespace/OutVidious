using TMS.Apps.FrontTube.Backend.Repository.DataBase.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities.Cache;

/// <summary>
/// Maps channels to their banner images.
/// </summary>
public class CacheChannelBannerMapEntity : EntityBase, IImageMap
{
    //public int Id { get; set; }

    public int ChannelId { get; set; }

    public int ImageId { get; set; }

    // Navigation properties
    public CacheChannelEntity Channel { get; set; } = null!;

    public CacheImageEntity Image { get; set; } = null!;

    public static IImageMap Create(
        CacheImageEntity imageEntity,
        ICacheableEntity parentEntity)
    {
        if (parentEntity is not CacheChannelEntity channelEntity)
        {
            throw new ArgumentException("Parent entity must be of type ChannelEntity", nameof(parentEntity));
        }

        return new CacheChannelBannerMapEntity
        {
            ChannelId = channelEntity.Id,
            ImageId = imageEntity.Id,
            Channel = channelEntity,
            Image = imageEntity
        };
    }
}

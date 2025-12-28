using TMS.Apps.FrontTube.Backend.Repository.DataBase.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;

/// <summary>
/// Maps channels to their banner images.
/// </summary>
public class ChannelBannerMapEntity : TrackableEntitiesBase, IImageMap
{
    //public int Id { get; set; }

    public int ChannelId { get; set; }

    public int ImageId { get; set; }

    // Navigation properties
    public ChannelEntity Channel { get; set; } = null!;

    public ImageEntity Image { get; set; } = null!;

    public static IImageMap Create(
        ImageEntity imageEntity,
        ICacheableEntity parentEntity)
    {
        if (parentEntity is not ChannelEntity channelEntity)
        {
            throw new ArgumentException("Parent entity must be of type ChannelEntity", nameof(parentEntity));
        }

        return new ChannelBannerMapEntity
        {
            ChannelId = channelEntity.Id,
            ImageId = imageEntity.Id,
            Channel = channelEntity,
            Image = imageEntity
        };
    }
}

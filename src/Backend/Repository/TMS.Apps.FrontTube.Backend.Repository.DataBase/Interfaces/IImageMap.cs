using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;

namespace TMS.Apps.FrontTube.Backend.Repository.DataBase.Interfaces;

/// <summary>
/// This interface is to easily create the map between image and some other cacheable entity
/// </summary>
    public interface IImageMap : IEntity
    {
        int ImageId { get; set; }

        ImageEntity Image { get; set; }

        virtual static IImageMap Create(
            ImageEntity imageEntity,
            ICacheableEntity parentEntity) => throw new NotImplementedException();
    }
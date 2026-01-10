using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities.Cache;

namespace TMS.Apps.FrontTube.Backend.Repository.DataBase.Interfaces;

/// <summary>
/// This interface is to easily create the map between image and some other cacheable entity
/// </summary>
    public interface IImageMap : IEntity
    {
        int ImageId { get; set; }

        CacheImageEntity Image { get; set; }

        virtual static IImageMap Create(
            CacheImageEntity imageEntity,
            ICacheableEntity parentEntity) => throw new NotImplementedException();
    }
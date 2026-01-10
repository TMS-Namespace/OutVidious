using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities.Cache;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Repository.Tools;

internal static class IdentityExtensions
{
    public static RemoteIdentityCommon ToIdentity(this ICacheableCommon common)
    {
        ArgumentNullException.ThrowIfNull(common);

        return common.RemoteIdentity;
    }

    public static RemoteIdentityCommon ToIdentity(this ICacheableEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var identityType = ResolveIdentityType(entity);
        return new RemoteIdentityCommon(identityType, entity.RemoteIdentity);
    }

    private static RemoteIdentityTypeCommon ResolveIdentityType(ICacheableEntity entity)
    {
        return entity switch
        {
            CacheVideoEntity => RemoteIdentityTypeCommon.Video,
            CacheChannelEntity => RemoteIdentityTypeCommon.Channel,
            CacheImageEntity => RemoteIdentityTypeCommon.Image,
            CacheCaptionEntity => RemoteIdentityTypeCommon.Caption,
            CacheStreamEntity => RemoteIdentityTypeCommon.Stream,
            _ => throw new NotSupportedException($"Entity type {entity.GetType().Name} is not supported for identity resolution.")
        };
    }
}

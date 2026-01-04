using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Repository.Cache.Tools;

public static class IdentityExtensions
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
        return new RemoteIdentityCommon(identityType, entity.AbsoluteRemoteUrl);
    }

    private static RemoteIdentityTypeCommon ResolveIdentityType(ICacheableEntity entity)
    {
        return entity switch
        {
            VideoEntity => RemoteIdentityTypeCommon.Video,
            ChannelEntity => RemoteIdentityTypeCommon.Channel,
            ImageEntity => RemoteIdentityTypeCommon.Image,
            CaptionEntity => RemoteIdentityTypeCommon.Caption,
            StreamEntity => RemoteIdentityTypeCommon.Stream,
            _ => throw new NotSupportedException($"Entity type {entity.GetType().Name} is not supported for identity resolution.")
        };
    }
}

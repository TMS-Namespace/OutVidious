using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Repository.Cache.Tools;
    public static class IdentityExtensions
    {
    public static CacheableIdentity ToIdentity(this ICacheableCommon common)
    {
        return new CacheableIdentity
        {
            AbsoluteRemoteUrlString = common.AbsoluteRemoteUrl.ToString()
        };
    }

        public static CacheableIdentity ToIdentity(this ICacheableEntity entity)
    {
        return new CacheableIdentity()
        {
            DataBaseId = entity.Id,
            AbsoluteRemoteUrlString = entity.AbsoluteRemoteUrl,
        };
    }

    }

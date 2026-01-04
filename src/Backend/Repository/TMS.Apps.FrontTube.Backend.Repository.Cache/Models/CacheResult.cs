using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;
using TMS.Apps.FrontTube.Backend.Repository.Cache.Enums;
using TMS.Apps.FrontTube.Backend.Repository.Cache.Interfaces;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Repository.Cache.Models;


public record CacheResult<T>(
    EntityStatus Status,
    RemoteIdentityCommon Identity,
    T? Entity,
    ICacheableCommon? Common,
    string? Error) : ICacheResult
    where T : class, ICacheableEntity
{
    EntityStatus ICacheResult.ResultType => Status;
    RemoteIdentityCommon ICacheResult.Identity => Identity;
    ICacheableEntity? ICacheResult.EntityNeutral => Entity;
    ICacheableCommon? ICacheResult.Common => Common;
    string? ICacheResult.Error => Error;
}
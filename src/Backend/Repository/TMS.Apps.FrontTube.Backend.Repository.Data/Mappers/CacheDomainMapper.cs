using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Repository.Models;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;
using DomainContracts = TMS.Apps.FrontTube.Backend.Repository.Data.Contracts;

namespace TMS.Apps.FrontTube.Backend.Repository.Data.Mappers;

internal static class CacheDomainMapper
{
    public static DomainContracts.ChannelDomain ToDomain(CacheResult<ChannelEntity> cacheResult, DomainContracts.ChannelDomain? target = null)
    {
        ArgumentNullException.ThrowIfNull(cacheResult);

        if (cacheResult.Entity is null)
        {
            throw new InvalidOperationException("Cache result entity is required to map to channel domain.");
        }

        target = EntityDomainMapper.ToDomain(cacheResult.Entity, target);
        target.FetchingError = cacheResult.Error;

        return target;
    }

    public static DomainContracts.VideoDomain ToDomain(CacheResult<VideoEntity> cacheResult, DomainContracts.VideoDomain? target = null)
    {
        ArgumentNullException.ThrowIfNull(cacheResult);

        if (cacheResult.Entity is null)
        {
            throw new InvalidOperationException("Cache result entity is required to map to video domain.");
        }

        target = EntityDomainMapper.ToDomain(cacheResult.Entity, target);
        target.FetchingError = cacheResult.Error;

        return target;
    }

    public static DomainContracts.ImageDomain ToDomain(CacheResult<ImageEntity> cacheResult, DomainContracts.ImageDomain? target = null)
    {
        ArgumentNullException.ThrowIfNull(cacheResult);

        if (cacheResult.Entity is null)
        {
            throw new InvalidOperationException("Cache result entity is required to map to image domain.");
        }

        target = EntityDomainMapper.ToDomain(cacheResult.Entity, target);
        target.FetchingError = cacheResult.Error;

        return target;
    }

    public static DomainContracts.StreamDomain ToDomain(CacheResult<StreamEntity> cacheResult, DomainContracts.StreamDomain? target = null)
    {
        ArgumentNullException.ThrowIfNull(cacheResult);

        if (cacheResult.Entity is null)
        {
            throw new InvalidOperationException("Cache result entity is required to map to stream domain.");
        }

        target = EntityDomainMapper.ToDomain(cacheResult.Entity, target);
        target.FetchingError = cacheResult.Error;

        return target;
    }

    public static DomainContracts.CaptionDomain ToDomain(CacheResult<CaptionEntity> cacheResult, DomainContracts.CaptionDomain? target = null)
    {
        ArgumentNullException.ThrowIfNull(cacheResult);

        if (cacheResult.Entity is null)
        {
            throw new InvalidOperationException("Cache result entity is required to map to caption domain.");
        }

        target = EntityDomainMapper.ToDomain(cacheResult.Entity, target);
        target.FetchingError = cacheResult.Error;

        return target;
    }
}

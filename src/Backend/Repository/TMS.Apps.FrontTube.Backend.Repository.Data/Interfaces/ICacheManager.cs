using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;
using TMS.Apps.FrontTube.Backend.Repository.Models;
using TMS.Apps.FrontTube.Backend.Repository.DataBase;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Repository.Interfaces;

/// <summary>
/// Repository interface for managing cached entities.
/// Implements a multi-tier caching strategy: Second level caching → Database → Provider.
/// All lookups are performed using hash keys computed from absolute remote URLs.
/// </summary>
internal interface ICacheManager : IDisposable
{
    IProvider Provider { get; set; }
    //Task<CacheResult<T>> GetGloballyAsync<T>(CacheableIdentity identity, CancellationToken cancellationToken, bool autoSave = true) where T : class, ICacheableEntity;
    //Task<List<CacheResult<T>>> GetGloballyAsync<T>(IReadOnlyList<CacheableIdentity> identities, CancellationToken cancellationToken, bool autoSave = true) where T : class, ICacheableEntity;
    Task<List<CacheResult<T>>> GetLocallyAsync<T>(IReadOnlyList<ICacheableCommon> commons, DataBaseContext dataBaseContext, CancellationToken cancellationToken) where T : class, ICacheableEntity;
    Task<CacheResult<T>> GetLocallyAsync<T>(ICacheableCommon common, DataBaseContext dataBaseContext, CancellationToken cancellationToken) where T : class, ICacheableEntity;

}

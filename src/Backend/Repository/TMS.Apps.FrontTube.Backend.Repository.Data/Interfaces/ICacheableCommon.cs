using TMS.Apps.FrontTube.Backend.Repository.Data.Contracts;

namespace TMS.Apps.FrontTube.Backend.Repository.Data.Interfaces;

public interface ICacheableDomain
{
    RemoteIdentityDomain RemoteIdentity { get; }

    DateTime? LastSyncedAt { get; set; }
}

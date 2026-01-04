using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;

public interface ICacheableCommon : ICommonContract
{
    RemoteIdentityCommon RemoteIdentity { get; }

    bool IsMetaData { get; }
}

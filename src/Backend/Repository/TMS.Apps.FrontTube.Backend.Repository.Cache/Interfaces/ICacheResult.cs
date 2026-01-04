using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;
using TMS.Apps.FrontTube.Backend.Repository.Cache.Enums;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Repository.Cache.Interfaces;

public interface ICacheResult
{
    EntityStatus ResultType { get;  }
    RemoteIdentityCommon Identity { get;  }
    ICacheableEntity? EntityNeutral { get;  }
    ICacheableCommon? Common { get;  }
    string? Error { get;  }
}
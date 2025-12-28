using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;
    public interface ICacheableCommon
    {
        long Hash { get;  }


        // /// <summary>
        // /// When this entity was last synchronized from the remote source.
        // /// Used for staleness checking.
        // /// </summary>
        // DateTime? LastSyncedAt { get; set; }

        /// <summary>
        /// The original remote URL of this entity.
        /// </summary>
        Uri AbsoluteRemoteUrl { get;  }

        bool IsMetaData { get; }
    }

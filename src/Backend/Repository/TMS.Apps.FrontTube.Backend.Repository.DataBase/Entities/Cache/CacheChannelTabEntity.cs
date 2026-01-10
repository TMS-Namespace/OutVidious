using TMS.Apps.FrontTube.Backend.Repository.DataBase.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities.Cache;

    public class CacheChannelTabEntity : EntityBase, ICacheableEntity
    {
        public required int ChannelId { get; set; }

        public required int TabTypeId { get; set; }

        public required string Title { get; set; }

        public long Hash { get ; set; }
        
        public DateTime? LastSyncedAt { get; set; }

        public required string RemoteIdentity { get; set; }
}

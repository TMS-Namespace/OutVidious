using Microsoft.EntityFrameworkCore;
using TMS.Apps.FrontTube.Backend.Repository.DataBase;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities.Cache;

namespace TMS.Apps.FrontTube.Backend.Repository.CacheManager.Tools
{
    internal static class DatabaseContextExtensions
    {
        public static IQueryable<CacheVideoEntity> BuildVideosQuery(this DataBaseContext dbContext, bool full, bool noTracking)
        {
            IQueryable<CacheVideoEntity> query = dbContext
            .Videos
            .Include(v => v.Channel)
                .ThenInclude(c => c.Avatars)
                    .ThenInclude(ca => ca.Image)
            .Include(v => v.Thumbnails)
                .ThenInclude(vt => vt.Image)
            .AsSplitQuery();

            if (full)
            {
                query = query
                    .Include(v => v.Streams)
                    .Include(v => v.Captions);
            }

            if (noTracking)
            {
                query = query.AsNoTracking();
            }

            return query;
        }

        public static IQueryable<CacheChannelEntity> BuildChannelsQuery(this DataBaseContext dbContext, bool full, bool noTracking)
        {
            IQueryable<CacheChannelEntity> query = dbContext
            .Channels
            .Include(c => c.Avatars)
                .ThenInclude(ca => ca.Image)
            .AsSplitQuery();

            if (full)
            {
                query = query
                    .Include(c => c.Banners)
                        .ThenInclude(cb => cb.Image);
            }

            if (noTracking)
            {
                query = query.AsNoTracking();
            }

            return query;
        }
    }
}
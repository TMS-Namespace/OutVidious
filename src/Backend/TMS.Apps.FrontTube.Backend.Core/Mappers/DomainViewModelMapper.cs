using TMS.Apps.FrontTube.Backend.Core.ViewModels;
using TMS.Apps.FrontTube.Backend.Repository.Data.Contracts;

namespace TMS.Apps.FrontTube.Backend.Core.Mappers;

public static class DomainViewModelMapper
{
    public static Image ToViewModel(Super super, ImageDomain domain, Image? target = null)
    {
        ArgumentNullException.ThrowIfNull(super);
        ArgumentNullException.ThrowIfNull(domain);

        if (target is null)
        {
            return new Image(super, domain);
        }

        target.UpdateFromDomain(domain);
        return target;
    }

    public static Channel ToViewModel(Super super, ChannelDomain domain, Channel? target = null)
    {
        ArgumentNullException.ThrowIfNull(super);
        ArgumentNullException.ThrowIfNull(domain);

        var avatars = domain.Avatars
            .Select(avatar => ToViewModel(super, avatar))
            .ToList();

        var banners = domain.Banners
            .Select(banner => ToViewModel(super, banner))
            .ToList();

        if (target is null)
        {
            return new Channel(super, domain, avatars, banners);
        }

        target.UpdateFromDomain(domain, avatars, banners);
        return target;
    }

    public static Video ToViewModel(
        Super super,
        VideoDomain domain,
        Channel? channel = null,
        Video? target = null)
    {
        ArgumentNullException.ThrowIfNull(super);
        ArgumentNullException.ThrowIfNull(domain);

        var resolvedChannel = channel;
        if (resolvedChannel is null)
        {
            if (domain.Channel is null)
            {
                throw new ArgumentException("VideoDomain.Channel is required when no Channel is provided.", nameof(domain));
            }

            resolvedChannel = ToViewModel(super, domain.Channel);
        }

        var thumbnails = domain.Thumbnails
            .Select(thumbnail => ToViewModel(super, thumbnail))
            .ToList();

        if (target is null)
        {
            return new Video(super, resolvedChannel, domain, thumbnails);
        }

        target.UpdateFromDomain(domain, resolvedChannel, thumbnails);
        return target;
    }

    public static VideosPage ToViewModel(
        Super super,
        VideosPageDomain domain,
        Channel channel,
        IReadOnlyList<Video>? videos = null,
        VideosPage? target = null)
    {
        ArgumentNullException.ThrowIfNull(super);
        ArgumentNullException.ThrowIfNull(domain);
        ArgumentNullException.ThrowIfNull(channel);

        var resolvedVideos = videos?.ToList()
            ?? domain.Videos
                .Select(video => ToViewModel(super, video, channel))
                .ToList();

        if (target is null)
        {
            return new VideosPage(super, domain, resolvedVideos, channel);
        }

        return target with
        {
            Channel = channel,
            Tab = domain.Tab,
            Videos = resolvedVideos,
            ContinuationToken = domain.ContinuationToken,
            TotalVideoCount = domain.TotalVideoCount,
            PageNumber = domain.PageNumber
        };
    }
}

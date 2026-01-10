using TMS.Apps.FrontTube.Backend.Core.Enums;
using TMS.Apps.FrontTube.Backend.Core.ViewModels;
using TMS.Apps.FrontTube.Backend.Repository.Data.Contracts;
using TMS.Apps.FrontTube.Backend.Repository.Data.Enums;
using DomainEnums = TMS.Apps.FrontTube.Backend.Repository.Data.Enums;
using CoreEnums = TMS.Apps.FrontTube.Backend.Core.Enums;

namespace TMS.Apps.FrontTube.Backend.Core.Mappers;

public static class DomainViewModelMapper
{
    public static RemoteIdentity ToViewModel(RemoteIdentityDomain domain)
    {
        ArgumentNullException.ThrowIfNull(domain);

        return new RemoteIdentity
        {
            IdentityType = ToViewModel(domain.IdentityType),
            AbsoluteRemoteUrl = domain.AbsoluteRemoteUrl,
            Hash = domain.Hash,
            RemoteId = domain.RemoteId
        };
    }

    private static RemoteIdentityType ToViewModel(RemoteIdentityTypeDomain type)
    {
        return type switch
        {
            RemoteIdentityTypeDomain.Video => RemoteIdentityType.Video,
            RemoteIdentityTypeDomain.Channel => RemoteIdentityType.Channel,
            RemoteIdentityTypeDomain.Image => RemoteIdentityType.Image,
            RemoteIdentityTypeDomain.Caption => RemoteIdentityType.Caption,
            RemoteIdentityTypeDomain.Stream => RemoteIdentityType.Stream,
            _ => throw new NotSupportedException($"Unsupported identity type: {type}.")
        };
    }

    public static CoreEnums.ChannelTab ToViewModelChannelTab(DomainEnums.ChannelTabType domainTab)
    {
        return domainTab switch
        {
            DomainEnums.ChannelTabType.Videos => CoreEnums.ChannelTab.Videos,
            DomainEnums.ChannelTabType.Shorts => CoreEnums.ChannelTab.Shorts,
            DomainEnums.ChannelTabType.Streams => CoreEnums.ChannelTab.Streams,
            DomainEnums.ChannelTabType.Playlists => CoreEnums.ChannelTab.Playlists,
            DomainEnums.ChannelTabType.Community => CoreEnums.ChannelTab.Community,
            DomainEnums.ChannelTabType.Channels => CoreEnums.ChannelTab.Channels,
            DomainEnums.ChannelTabType.Latest => CoreEnums.ChannelTab.Latest,
            DomainEnums.ChannelTabType.Podcasts => CoreEnums.ChannelTab.Podcasts,
            DomainEnums.ChannelTabType.Releases => CoreEnums.ChannelTab.Releases,
            _ => CoreEnums.ChannelTab.Videos
        };
    }

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
        if (target is null)
        {
            return new Channel(super, domain);
        }

        target.UpdateFromDomain(domain);
        return target;
    }

    public static Video ToViewModel(
        Super super,
        VideoDomain domain,
        Video? target = null)
    {
        ArgumentNullException.ThrowIfNull(super);
        ArgumentNullException.ThrowIfNull(domain);
        if (target is null)
        {
            return new Video(super, domain);
        }

        target.UpdateFromDomain(domain);
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
                .Select(video => ToViewModel(super, video))
                .ToList();

        if (target is null)
        {
            return new VideosPage(super, domain, resolvedVideos, channel);
        }

        return target with
        {
            Channel = channel,
            Tab = ToViewModelChannelTab(domain.Tab),
            Videos = resolvedVideos,
            ContinuationToken = domain.ContinuationToken,
            TotalVideoCount = domain.TotalVideoCount,
            PageNumber = domain.PageNumber
        };
    }
}

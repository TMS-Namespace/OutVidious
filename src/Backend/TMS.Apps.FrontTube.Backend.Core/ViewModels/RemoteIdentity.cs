using TMS.Apps.FrontTube.Backend.Core.Enums;
using TMS.Apps.FrontTube.Backend.Core.Tools;

namespace TMS.Apps.FrontTube.Backend.Core.ViewModels;

/// <summary>
/// ViewModel representation of a remote identity.
/// </summary>
public sealed record RemoteIdentity
{
    public required RemoteIdentityType IdentityType { get; init; }

    public required string AbsoluteRemoteUrl { get; init; }

    public required long Hash { get; init; }

    public string? RemoteId { get; init; }

    public string GetProxyUrl(ProxyToProvider proxy)
    {
        ArgumentNullException.ThrowIfNull(proxy);

        return IdentityType switch
        {
            RemoteIdentityType.Video => BuildWatchUrl(),
            RemoteIdentityType.Channel => BuildChannelUrl(),
            RemoteIdentityType.Image => BuildImageProxyUrl(proxy),
            RemoteIdentityType.Stream => proxy.BuildVideoPlaybackProxyUrl(this) ?? string.Empty,
            _ => string.Empty
        };
    }

    private string BuildWatchUrl()
    {
        var encodedUrl = Uri.EscapeDataString(AbsoluteRemoteUrl);
        return $"/watch?url={encodedUrl}";
    }

    private string BuildChannelUrl()
    {
        var encodedUrl = Uri.EscapeDataString(AbsoluteRemoteUrl);
        return $"/channel/{encodedUrl}";
    }

    private string BuildImageProxyUrl(ProxyToProvider proxy)
    {
        if (!Uri.TryCreate(AbsoluteRemoteUrl, UriKind.Absolute, out var originalUrl))
        {
            return string.Empty;
        }

        var fetchUrl = proxy.ProxyImageRemoteUrl(originalUrl);
        var encodedOriginalUrl = Uri.EscapeDataString(originalUrl.ToString());
        var encodedFetchUrl = Uri.EscapeDataString(fetchUrl.ToString());
        return $"/api/ImageProxy?originalUrl={encodedOriginalUrl}&fetchUrl={encodedFetchUrl}";
    }
}

using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Cache;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
    
public record CacheableIdentity
{
    public required string AbsoluteRemoteUrlString { get; init; }
    
    private long? _hash;
    public long Hash => _hash ??= HashHelper.ComputeHash(AbsoluteRemoteUrlString.ToString());

    public string? RemoteId {get; init;}

    public int? DataBaseId {get; init;}

    private Uri? _absoluteRemoteUrl;
    public Uri AbsoluteRemoteUrl => _absoluteRemoteUrl ??= new Uri(AbsoluteRemoteUrlString);

}
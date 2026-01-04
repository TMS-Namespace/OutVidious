using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Cache;

namespace TMS.Apps.FrontTube.Backend.Repository.Data.Contracts;

public sealed class IdentityDomain
{
    public required string AbsoluteRemoteUrlString { get; set; }

    private long? _hash;
    public long Hash => _hash ??= HashHelper.ComputeHash(AbsoluteRemoteUrlString);

    public string? RemoteId { get; set; }

    public int? DataBaseId { get; set; }

    private Uri? _absoluteRemoteUrl;
    public Uri AbsoluteRemoteUrl => _absoluteRemoteUrl ??= new Uri(AbsoluteRemoteUrlString);

    public string? FetchingError { get; set; }
}

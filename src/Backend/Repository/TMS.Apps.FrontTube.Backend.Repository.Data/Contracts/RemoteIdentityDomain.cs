using TMS.Apps.FrontTube.Backend.Repository.Data.Enums;

namespace TMS.Apps.FrontTube.Backend.Repository.Data.Contracts;

public sealed class RemoteIdentityDomain
{
    public required string AbsoluteRemoteUrl { get; init; }

    public required RemoteIdentityTypeDomain IdentityType { get; init; }

    public required long Hash { get; init; }

    public string? RemoteId { get; init; }

    public required Uri AbsoluteRemoteUri { get; init; }

    public int? DataBaseId { get; set; }

    public string? FetchingError { get; set; }
}

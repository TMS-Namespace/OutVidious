using System.Diagnostics.CodeAnalysis;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tools;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tools.YouTube;


namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

public record RemoteIdentityCommon
{
    [SetsRequiredMembers]
    public RemoteIdentityCommon(RemoteIdentityTypeCommon identityType, string remoteIdentity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(remoteIdentity);

        IdentityType = identityType;

        if (identityType is RemoteIdentityTypeCommon.Video or RemoteIdentityTypeCommon.Channel)
        {
            var isValid = YouTubeIdentityParser.TryParse(remoteIdentity, out var parts);

            if (!isValid)
            {
                throw new ArgumentException($"The provided URL '{remoteIdentity}' is not a valid YouTube URL, because: {string.Join(", ", parts.Errors)}.", nameof(remoteIdentity));
            }

            if (!parts.IsSupported())
            {
                throw new ArgumentException($"The provided URL '{remoteIdentity}' is not supported by FrontTube, the recognized identity type is '{parts.IdentityType}'.", nameof(remoteIdentity));
            }

            if (identityType == RemoteIdentityTypeCommon.Video && !parts.IsVideo)
            {
                throw new ArgumentException($"The provided URL '{remoteIdentity}' is not a valid video identity.", nameof(remoteIdentity));
            }

            if (identityType == RemoteIdentityTypeCommon.Channel && !parts.IsChannel)
            {
                throw new ArgumentException($"The provided URL '{remoteIdentity}' is not a valid channel identity.", nameof(remoteIdentity));
            }

            var canonicalUrl = parts.ToUrl() ?? parts.AbsoluteRemoteUrl;
            if (canonicalUrl is null)
            {
                throw new ArgumentException($"The provided URL '{remoteIdentity}' could not be normalized to a canonical URL.", nameof(remoteIdentity));
            }

            AbsoluteRemoteUri = canonicalUrl;
            AbsoluteRemoteUrl = canonicalUrl.ToString();
            RemoteId = parts.PrimaryRemoteId;

            if (string.IsNullOrWhiteSpace(RemoteId))
            {
                throw new ArgumentException($"The provided URL '{remoteIdentity}' does not contain a valid remote ID.", nameof(remoteIdentity));
            }
        }
        else
        {
            if (!Uri.TryCreate(remoteIdentity, UriKind.RelativeOrAbsolute, out var uri))
            {
                throw new ArgumentException($"The provided URL '{remoteIdentity}' is not a valid URL.", nameof(remoteIdentity));
            }

            AbsoluteRemoteUri = uri;
            AbsoluteRemoteUrl = uri.ToString();
        }

        Hash = HashHelper.ComputeHash(AbsoluteRemoteUrl);
    }

    public required string AbsoluteRemoteUrl { get; init; }

    public RemoteIdentityTypeCommon IdentityType { get; init; }

    public long Hash { get; init; }

    public string? RemoteId { get; init; }

    public Uri AbsoluteRemoteUri { get; init; }

}

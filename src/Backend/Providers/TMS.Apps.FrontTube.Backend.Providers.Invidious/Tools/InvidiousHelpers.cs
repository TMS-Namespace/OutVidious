using System.Text.Json;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Configuration;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tools;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tools.YouTube;
using TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;
using TMS.Apps.FrontTube.Backend.Providers.Invidious.Mappers;

namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.Tools;

/// <summary>
/// Helper methods for Invidious provider operations.
/// </summary>
internal static class InvidiousHelpers
{
    /// <summary>
    /// Creates an HttpClient with the specified configuration.
    /// </summary>
    /// <param name="config">Provider configuration.</param>
    /// <returns>Configured HttpClient instance.</returns>
    internal static HttpClient CreateHttpClient(ProviderConfig config)
    {
        var handler = new HttpClientHandler();

        if (config.BypassSslValidation)
        {
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }

        var client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds)
        };

        return client;
    }

    /// <summary>
    /// Extracts the remote ID from a RemoteIdentityCommon instance, validating its type.
    /// </summary>
    /// <param name="identity">The remote identity.</param>
    /// <param name="expectedType">The expected identity type.</param>
    /// <returns>The extracted remote ID.</returns>
    /// <exception cref="ArgumentNullException">When identity is null.</exception>
    /// <exception cref="ArgumentException">When identity type doesn't match expected type or URL is invalid.</exception>
    internal static string GetRemoteIdOrThrow(RemoteIdentityCommon identity, RemoteIdentityTypeCommon expectedType)
    {
        ArgumentNullException.ThrowIfNull(identity);

        if (identity.IdentityType != expectedType)
        {
            throw new ArgumentException($"Identity type must be {expectedType}.", nameof(identity));
        }

        if (!string.IsNullOrWhiteSpace(identity.RemoteId))
        {
            return identity.RemoteId;
        }

        if (!YouTubeIdentityParser.TryParse(identity.AbsoluteRemoteUrl, out var parts))
        {
            throw new ArgumentException(
                $"Remote identity URL '{identity.AbsoluteRemoteUrl}' is not a valid YouTube URL: {string.Join(", ", parts.Errors)}.",
                nameof(identity));
        }

        if (!parts.IsSupported())
        {
            throw new ArgumentException(
                $"Remote identity URL '{identity.AbsoluteRemoteUrl}' is not supported by FrontTube, the recognized identity type is '{parts.IdentityType}'.",
                nameof(identity));
        }

        if (expectedType == RemoteIdentityTypeCommon.Video && !parts.IsVideo)
        {
            throw new ArgumentException($"Remote identity URL '{identity.AbsoluteRemoteUrl}' is not a valid video identity.", nameof(identity));
        }

        if (expectedType == RemoteIdentityTypeCommon.Channel && !parts.IsChannel)
        {
            throw new ArgumentException($"Remote identity URL '{identity.AbsoluteRemoteUrl}' is not a valid channel identity.", nameof(identity));
        }

        var remoteId = parts.PrimaryRemoteId;

        if (string.IsNullOrWhiteSpace(remoteId))
        {
            throw new ArgumentException($"Remote ID is required for {expectedType} identity.", nameof(identity));
        }

        return remoteId;
    }

    /// <summary>
    /// Parses raw search results into typed search result items.
    /// </summary>
    /// <param name="rawResults">Raw search results from API.</param>
    /// <param name="baseUrl">Base URL for the Invidious instance.</param>
    /// <param name="jsonOptions">JSON serializer options.</param>
    /// <param name="logUnknownTypes">Action to log unknown types.</param>
    /// <returns>List of parsed search result items.</returns>
    internal static List<SearchResultItemCommon> ParseSearchResults(
        List<object> rawResults,
        Uri baseUrl,
        JsonSerializerOptions jsonOptions,
        Action<string?> logUnknownTypes)
    {
        var items = new List<SearchResultItemCommon>();

        foreach (var raw in rawResults)
        {
            if (raw is not JsonElement element)
            {
                continue;
            }

            var typeProperty = element.TryGetProperty("type", out var typeProp) ? typeProp.GetString()?.Trim() : null;

            switch (typeProperty?.ToLowerInvariant())
            {
                case "video":
                    var video = element.Deserialize<SearchVideo>(jsonOptions);
                    if (video != null)
                    {
                        items.Add(SearchMapper.ToSearchResultVideo(video, baseUrl));
                    }
                    break;

                case "channel":
                    var channel = element.Deserialize<SearchChannel>(jsonOptions);
                    if (channel != null)
                    {
                        items.Add(SearchMapper.ToSearchResultChannel(channel, baseUrl));
                    }
                    break;

                case "playlist":
                    var playlist = element.Deserialize<SearchPlaylist>(jsonOptions);
                    if (playlist != null)
                    {
                        items.Add(SearchMapper.ToSearchResultPlaylist(playlist, baseUrl));
                    }
                    break;

                case "hashtag":
                    var hashtag = element.Deserialize<SearchHashtag>(jsonOptions);
                    if (hashtag != null)
                    {
                        items.Add(SearchMapper.ToSearchResultHashtag(hashtag));
                    }
                    break;

                default:
                    logUnknownTypes(typeProperty);
                    break;
            }
        }

        return items;
    }
}

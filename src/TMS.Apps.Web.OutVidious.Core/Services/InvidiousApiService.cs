using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using TMS.Apps.Web.OutVidious.Core.Converters;
using TMS.Apps.Web.OutVidious.Core.Interfaces;
using TMS.Apps.Web.OutVidious.Core.Models;

namespace TMS.Apps.Web.OutVidious.Core.Services;

/// <summary>
/// Service for interacting with the Invidious API.
/// </summary>
public sealed class InvidiousApiService : IInvidiousApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<InvidiousApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public InvidiousApiService(HttpClient httpClient, ILogger<InvidiousApiService> logger, string baseUrl)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        BaseUrl = baseUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(baseUrl));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Converters = { new FlexibleStringConverter() }
        };
    }

    /// <inheritdoc />
    public string BaseUrl { get; }

    /// <inheritdoc />
    public async Task<VideoDetails?> GetVideoDetailsAsync(string videoId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(videoId))
        {
            throw new ArgumentException("Video ID cannot be empty.", nameof(videoId));
        }

        var apiUrl = $"{BaseUrl}/api/v1/videos/{Uri.EscapeDataString(videoId)}";
        _logger.LogDebug("Fetching video details from: {ApiUrl}", apiUrl);

        try
        {
            var response = await _httpClient.GetAsync(apiUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("API request failed with status {StatusCode} for video {VideoId}",
                    response.StatusCode, videoId);
                return null;
            }

            var videoDetails = await response.Content.ReadFromJsonAsync<VideoDetails>(_jsonOptions, cancellationToken);
            _logger.LogDebug("Successfully fetched video details for: {VideoId}", videoId);

            return videoDetails;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching video {VideoId}", videoId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error for video {VideoId}", videoId);
            throw;
        }
    }

    /// <inheritdoc />
    public string GetEmbedUrl(string videoId)
    {
        if (string.IsNullOrWhiteSpace(videoId))
        {
            throw new ArgumentException("Video ID cannot be empty.", nameof(videoId));
        }

        return $"{BaseUrl}/embed/{Uri.EscapeDataString(videoId)}?autoplay=1&local=true";
    }

    /// <inheritdoc />
    public string GetWatchUrl(string videoId)
    {
        if (string.IsNullOrWhiteSpace(videoId))
        {
            throw new ArgumentException("Video ID cannot be empty.", nameof(videoId));
        }

        return $"{BaseUrl}/watch?v={Uri.EscapeDataString(videoId)}";
    }

    /// <inheritdoc />
    public string GetDashManifestUrl(string videoId)
    {
        if (string.IsNullOrWhiteSpace(videoId))
        {
            throw new ArgumentException("Video ID cannot be empty.", nameof(videoId));
        }

        // Use local=true for proxying through Invidious (avoids CORS issues)
        // Use unique_res=1 to ensure unique resolutions in the manifest
        return $"{BaseUrl}/api/manifest/dash/id/{Uri.EscapeDataString(videoId)}?local=true&unique_res=1";
    }

    /// <inheritdoc />
    public string GetProxiedDashManifestUrl(string videoId)
    {
        if (string.IsNullOrWhiteSpace(videoId))
        {
            throw new ArgumentException("Video ID cannot be empty.", nameof(videoId));
        }

        // Use local proxy endpoint to avoid CORS issues
        return $"/api/proxy/dash/{Uri.EscapeDataString(videoId)}";
    }
}

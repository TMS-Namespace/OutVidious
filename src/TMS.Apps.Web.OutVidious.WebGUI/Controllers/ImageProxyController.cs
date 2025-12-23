using Microsoft.AspNetCore.Mvc;
using TMS.Apps.FTube.Backend.DataRepository.Interfaces;
using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Interfaces;

namespace TMS.Apps.Web.OutVidious.WebGUI.Controllers;

/// <summary>
/// Controller that proxies image requests through the caching layer (memory → DB → web).
/// This allows the browser to load images via standard HTML img tags while benefiting from server-side caching.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ImageProxyController : ControllerBase
{
    private readonly IDataRepository _dataRepository;
    private readonly IVideoProvider _videoProvider;
    private readonly ILogger<ImageProxyController> _logger;

    public ImageProxyController(
        IDataRepository dataRepository,
        IVideoProvider videoProvider,
        ILogger<ImageProxyController> logger)
    {
        _dataRepository = dataRepository;
        _videoProvider = videoProvider;
        _logger = logger;
    }

    /// <summary>
    /// Gets an image by its original URL and fetch URL, using the caching layer.
    /// The image is fetched from memory cache, database, or web as needed.
    /// </summary>
    /// <param name="originalUrl">The original URL of the image (YouTube CDN URL).</param>
    /// <param name="fetchUrl">The URL to fetch the image from (provider proxy URL).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The image data with appropriate content type.</returns>
    [HttpGet]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Client)]
    public async Task<IActionResult> GetImageAsync(
        [FromQuery] string originalUrl,
        [FromQuery] string fetchUrl,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(originalUrl) || !Uri.TryCreate(originalUrl, UriKind.Absolute, out var original))
        {
            return BadRequest("Valid original URL is required.");
        }

        if (string.IsNullOrWhiteSpace(fetchUrl) || !Uri.TryCreate(fetchUrl, UriKind.Absolute, out var fetch))
        {
            return BadRequest("Valid fetch URL is required.");
        }

        try
        {
            _logger.LogDebug("ImageProxy: Fetching image: {OriginalUrl} via {FetchUrl}", originalUrl, fetchUrl);

            var cachedImage = await _dataRepository.GetImageAsync(original, fetch, ct);

            if (cachedImage is null)
            {
                _logger.LogWarning("ImageProxy: Failed to fetch image: {OriginalUrl}", originalUrl);
                return NotFound();
            }

            _logger.LogDebug("ImageProxy: Returning image: {OriginalUrl}, MimeType: {MimeType}, Size: {Size} bytes", 
                originalUrl, cachedImage.MimeType, cachedImage.Data.Length);

            return File(cachedImage.Data, cachedImage.MimeType);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("ImageProxy: Request cancelled for: {OriginalUrl}", originalUrl);
            return StatusCode(499); // Client Closed Request
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ImageProxy: Error fetching image: {OriginalUrl}", originalUrl);
            return StatusCode(500, "Failed to fetch image.");
        }
    }
}

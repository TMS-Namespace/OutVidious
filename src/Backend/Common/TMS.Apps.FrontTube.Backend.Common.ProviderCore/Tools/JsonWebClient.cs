using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Models;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tools;

/// <summary>
/// HTTP client wrapper for JSON API requests with comprehensive error handling and logging.
/// This class does not throw exceptions; errors are returned in the response contract.
/// </summary>
public sealed class JsonWebClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<JsonWebClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonWebClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for requests.</param>
    /// <param name="jsonOptions">JSON serialization options.</param>
    /// <param name="loggerFactory">Logger factory for creating logger.</param>
    public JsonWebClient(
        HttpClient httpClient,
        JsonSerializerOptions jsonOptions,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(jsonOptions);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _httpClient = httpClient;
        _jsonOptions = jsonOptions;
        _logger = loggerFactory.CreateLogger<JsonWebClient>();
    }

    /// <summary>
    /// Performs an HTTP GET request and deserializes the response to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response to.</typeparam>
    /// <param name="url">The URL to request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response containing either the deserialized data or error information.</returns>
    public async Task<JsonWebResponse<T>> GetAsync<T>(string url, CancellationToken cancellationToken)
    {
        return await ExecuteAsync<T>(HttpMethodType.Get, url, content: null, cancellationToken);
    }

    /// <summary>
    /// Performs an HTTP POST request and deserializes the response to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response to.</typeparam>
    /// <param name="url">The URL to request.</param>
    /// <param name="content">The content to send in the request body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response containing either the deserialized data or error information.</returns>
    public async Task<JsonWebResponse<T>> PostAsync<T>(string url, HttpContent? content, CancellationToken cancellationToken)
    {
        return await ExecuteAsync<T>(HttpMethodType.Post, url, content, cancellationToken);
    }

    /// <summary>
    /// Performs an HTTP PUT request and deserializes the response to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response to.</typeparam>
    /// <param name="url">The URL to request.</param>
    /// <param name="content">The content to send in the request body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response containing either the deserialized data or error information.</returns>
    public async Task<JsonWebResponse<T>> PutAsync<T>(string url, HttpContent? content, CancellationToken cancellationToken)
    {
        return await ExecuteAsync<T>(HttpMethodType.Put, url, content, cancellationToken);
    }

    /// <summary>
    /// Performs an HTTP DELETE request and deserializes the response to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response to.</typeparam>
    /// <param name="url">The URL to request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response containing either the deserialized data or error information.</returns>
    public async Task<JsonWebResponse<T>> DeleteAsync<T>(string url, CancellationToken cancellationToken)
    {
        return await ExecuteAsync<T>(HttpMethodType.Delete, url, content: null, cancellationToken);
    }

    /// <summary>
    /// Performs an HTTP GET request and returns the raw response string.
    /// </summary>
    /// <param name="url">The URL to request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response containing either the raw string or error information.</returns>
    public async Task<JsonWebResponse<string>> GetStringAsync(string url, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogDebug("[{Method}] Fetching URL '{Url}'.", nameof(GetStringAsync), url);

        try
        {
            using var response = await _httpClient.GetAsync(url, cancellationToken);
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
            stopwatch.Stop();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "[{Method}] HTTP request to '{Url}' failed with status '{StatusCode}' in {Duration}ms.",
                    nameof(GetStringAsync),
                    url,
                    response.StatusCode,
                    stopwatch.ElapsedMilliseconds);

                return JsonWebResponse<string>.HttpError(
                    url,
                    HttpMethodType.Get,
                    response.StatusCode,
                    responseText,
                    stopwatch.ElapsedMilliseconds);
            }

            _logger.LogDebug(
                "[{Method}] Successfully fetched '{Url}' in {Duration}ms.",
                nameof(GetStringAsync),
                url,
                stopwatch.ElapsedMilliseconds);

            return JsonWebResponse<string>.Success(
                url,
                HttpMethodType.Get,
                response.StatusCode,
                responseText,
                responseText,
                stopwatch.ElapsedMilliseconds);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            stopwatch.Stop();
            _logger.LogWarning(
                ex,
                "[{Method}] Request to '{Url}' timed out after {Duration}ms.",
                nameof(GetStringAsync),
                url,
                stopwatch.ElapsedMilliseconds);

            return JsonWebResponse<string>.TimeoutError(url, HttpMethodType.Get, ex, stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException ex)
        {
            stopwatch.Stop();

            // OperationCanceledException (and TaskCanceledException) can be either a timeout or cancellation
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "[{Method}] Request to '{Url}' was cancelled after {Duration}ms.",
                    nameof(GetStringAsync),
                    url,
                    stopwatch.ElapsedMilliseconds);

                return JsonWebResponse<string>.Cancelled(url, HttpMethodType.Get, stopwatch.ElapsedMilliseconds);
            }

            _logger.LogWarning(
                ex,
                "[{Method}] Request to '{Url}' timed out after {Duration}ms.",
                nameof(GetStringAsync),
                url,
                stopwatch.ElapsedMilliseconds);

            return JsonWebResponse<string>.TimeoutError(url, HttpMethodType.Get, ex, stopwatch.ElapsedMilliseconds);
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "[{Method}] Unexpected error: Network error while requesting '{Url}' after {Duration}ms.",
                nameof(GetStringAsync),
                url,
                stopwatch.ElapsedMilliseconds);

            return JsonWebResponse<string>.NetworkError(url, HttpMethodType.Get, ex, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "[{Method}] Unexpected error: while requesting '{Url}' after {Duration}ms.",
                nameof(GetStringAsync),
                url,
                stopwatch.ElapsedMilliseconds);

            return JsonWebResponse<string>.NetworkError(url, HttpMethodType.Get, ex, stopwatch.ElapsedMilliseconds);
        }
    }

    private async Task<JsonWebResponse<T>> ExecuteAsync<T>(
        HttpMethodType methodType,
        string url,
        HttpContent? content,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var methodName = methodType.ToString().ToUpperInvariant();

        _logger.LogDebug("[{Method}] {HttpMethod} request to '{Url}'.", nameof(ExecuteAsync), methodName, url);

        HttpResponseMessage? response = null;
        string? responseText = null;

        try
        {
            response = methodType switch
            {
                HttpMethodType.Get => await _httpClient.GetAsync(url, cancellationToken),
                HttpMethodType.Post => await _httpClient.PostAsync(url, content, cancellationToken),
                HttpMethodType.Put => await _httpClient.PutAsync(url, content, cancellationToken),
                HttpMethodType.Delete => await _httpClient.DeleteAsync(url, cancellationToken),
                HttpMethodType.Patch => await _httpClient.PatchAsync(url, content, cancellationToken),
                _ => throw new NotSupportedException($"HTTP method '{methodType}' is not supported.")
            };

            responseText = await response.Content.ReadAsStringAsync(cancellationToken);
            stopwatch.Stop();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "[{Method}] {HttpMethod} request to '{Url}' failed with status '{StatusCode}' in {Duration}ms.",
                    nameof(ExecuteAsync),
                    methodName,
                    url,
                    response.StatusCode,
                    stopwatch.ElapsedMilliseconds);

                return JsonWebResponse<T>.HttpError(
                    url,
                    methodType,
                    response.StatusCode,
                    responseText,
                    stopwatch.ElapsedMilliseconds);
            }

            // Attempt to deserialize
            var deserializationResult = await DeserializeResponseAsync<T>(
                responseText,
                response.StatusCode,
                url,
                methodType,
                methodName,
                cancellationToken);

            if (deserializationResult.IsSuccess)
            {
                _logger.LogDebug(
                    "[{Method}] {HttpMethod} request to '{Url}' succeeded in {Duration}ms.",
                    nameof(ExecuteAsync),
                    methodName,
                    url,
                    stopwatch.ElapsedMilliseconds);

                return JsonWebResponse<T>.Success(
                    url,
                    methodType,
                    response.StatusCode,
                    responseText,
                    deserializationResult.Data!,
                    stopwatch.ElapsedMilliseconds);
            }

            return deserializationResult;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            stopwatch.Stop();
            _logger.LogWarning(
                ex,
                "[{Method}] {HttpMethod} request to '{Url}' timed out after {Duration}ms.",
                nameof(ExecuteAsync),
                methodName,
                url,
                stopwatch.ElapsedMilliseconds);

            return JsonWebResponse<T>.TimeoutError(url, methodType, ex, stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException ex)
        {
            stopwatch.Stop();

            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "[{Method}] {HttpMethod} request to '{Url}' was cancelled after {Duration}ms.",
                    nameof(ExecuteAsync),
                    methodName,
                    url,
                    stopwatch.ElapsedMilliseconds);

                return JsonWebResponse<T>.Cancelled(url, methodType, stopwatch.ElapsedMilliseconds);
            }

            _logger.LogWarning(
                ex,
                "[{Method}] {HttpMethod} request to '{Url}' timed out after {Duration}ms.",
                nameof(ExecuteAsync),
                methodName,
                url,
                stopwatch.ElapsedMilliseconds);

            return JsonWebResponse<T>.TimeoutError(url, methodType, ex, stopwatch.ElapsedMilliseconds);
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "[{Method}] Unexpected error: Network error during {HttpMethod} request to '{Url}' after {Duration}ms.",
                nameof(ExecuteAsync),
                methodName,
                url,
                stopwatch.ElapsedMilliseconds);

            return JsonWebResponse<T>.NetworkError(url, methodType, ex, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "[{Method}] Unexpected error: during {HttpMethod} request to '{Url}' after {Duration}ms.",
                nameof(ExecuteAsync),
                methodName,
                url,
                stopwatch.ElapsedMilliseconds);

            return JsonWebResponse<T>.NetworkError(url, methodType, ex, stopwatch.ElapsedMilliseconds);
        }
        finally
        {
            response?.Dispose();
        }
    }

    private async Task<JsonWebResponse<T>> DeserializeResponseAsync<T>(
        string responseText,
        HttpStatusCode statusCode,
        string url,
        HttpMethodType methodType,
        string methodName,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = JsonSerializer.Deserialize<T>(responseText, _jsonOptions);

            if (data is null)
            {
                _logger.LogWarning(
                    "[{Method}] {HttpMethod} request to '{Url}' returned null after deserialization.",
                    nameof(DeserializeResponseAsync),
                    methodName,
                    url);

                return JsonWebResponse<T>.DeserializationError(
                    url,
                    methodType,
                    statusCode,
                    responseText,
                    new InvalidOperationException("Deserialization returned null."),
                    0);
            }

            return new JsonWebResponse<T>
            {
                RequestedUrl = url,
                HttpMethod = methodType,
                StatusCode = statusCode,
                ResponseText = responseText,
                Data = data,
                DurationMs = 0
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "[{Method}] Unexpected error: Failed to deserialize response from '{Url}'.",
                nameof(DeserializeResponseAsync),
                url);

            return JsonWebResponse<T>.DeserializationError(
                url,
                methodType,
                statusCode,
                responseText,
                ex,
                0);
        }
    }
}

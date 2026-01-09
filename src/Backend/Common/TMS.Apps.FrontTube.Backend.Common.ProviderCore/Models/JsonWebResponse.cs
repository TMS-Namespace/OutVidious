using System.Net;
using System.Text;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Models;

/// <summary>
/// Represents the response from a JSON web request, including success/error state and diagnostics.
/// </summary>
/// <typeparam name="T">The type of the deserialized response object.</typeparam>
public sealed record JsonWebResponse<T>
{
    /// <summary>
    /// The requested URL.
    /// </summary>
    public required string RequestedUrl { get; init; }

    /// <summary>
    /// The HTTP method used for the request.
    /// </summary>
    public required HttpMethodType HttpMethod { get; init; }

    /// <summary>
    /// The HTTP status code returned by the server.
    /// Returns null if the request failed before receiving a response.
    /// </summary>
    public HttpStatusCode? StatusCode { get; init; }

    /// <summary>
    /// The raw response body as text.
    /// </summary>
    public string? ResponseText { get; init; }

    /// <summary>
    /// The deserialized response object.
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Indicates whether the operation was cancelled.
    /// </summary>
    public bool IsCancelled { get; init; }

    /// <summary>
    /// Indicates whether there was a deserialization error.
    /// </summary>
    public bool HasDeserializationError { get; init; }

    /// <summary>
    /// Indicates whether there was an HTTP error (non-success status code).
    /// </summary>
    public bool HasHttpError => StatusCode.HasValue && !IsSuccessStatusCode(StatusCode.Value);

    /// <summary>
    /// Indicates whether there was a network or connection error.
    /// </summary>
    public bool HasNetworkError { get; init; }

    /// <summary>
    /// Indicates whether there was a timeout.
    /// </summary>
    public bool HasTimeout { get; init; }

    /// <summary>
    /// Error message if any error occurred.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// The exception that occurred, if any.
    /// </summary>
    public Exception? Exception { get; init; }

    /// <summary>
    /// Indicates whether the request was successful (no errors and data is available).
    /// </summary>
    public bool IsSuccess => !IsCancelled 
                             && !HasDeserializationError 
                             && !HasHttpError 
                             && !HasNetworkError 
                             && !HasTimeout 
                             && Data is not null;

    /// <summary>
    /// Indicates whether any error occurred.
    /// </summary>
    public bool HasError => IsCancelled 
                            || HasDeserializationError 
                            || HasHttpError 
                            || HasNetworkError 
                            || HasTimeout;

    /// <summary>
    /// Duration of the request in milliseconds.
    /// </summary>
    public long DurationMs { get; init; }

    /// <summary>
    /// Creates a successful response.
    /// </summary>
    public static JsonWebResponse<T> Success(
        string requestedUrl,
        HttpMethodType httpMethod,
        HttpStatusCode statusCode,
        string? responseText,
        T data,
        long durationMs)
    {
        return new JsonWebResponse<T>
        {
            RequestedUrl = requestedUrl,
            HttpMethod = httpMethod,
            StatusCode = statusCode,
            ResponseText = responseText,
            Data = data,
            DurationMs = durationMs
        };
    }

    /// <summary>
    /// Creates a cancelled response.
    /// </summary>
    public static JsonWebResponse<T> Cancelled(
        string requestedUrl,
        HttpMethodType httpMethod,
        long durationMs)
    {
        return new JsonWebResponse<T>
        {
            RequestedUrl = requestedUrl,
            HttpMethod = httpMethod,
            IsCancelled = true,
            ErrorMessage = "The request was cancelled.",
            DurationMs = durationMs
        };
    }

    /// <summary>
    /// Creates an HTTP error response.
    /// </summary>
    public static JsonWebResponse<T> HttpError(
        string requestedUrl,
        HttpMethodType httpMethod,
        HttpStatusCode statusCode,
        string? responseText,
        long durationMs)
    {
        return new JsonWebResponse<T>
        {
            RequestedUrl = requestedUrl,
            HttpMethod = httpMethod,
            StatusCode = statusCode,
            ResponseText = responseText,
            ErrorMessage = $"HTTP request failed with status code {(int)statusCode} ({statusCode}).",
            DurationMs = durationMs
        };
    }

    /// <summary>
    /// Creates a deserialization error response.
    /// </summary>
    public static JsonWebResponse<T> DeserializationError(
        string requestedUrl,
        HttpMethodType httpMethod,
        HttpStatusCode? statusCode,
        string? responseText,
        Exception exception,
        long durationMs)
    {
        return new JsonWebResponse<T>
        {
            RequestedUrl = requestedUrl,
            HttpMethod = httpMethod,
            StatusCode = statusCode,
            ResponseText = responseText,
            HasDeserializationError = true,
            ErrorMessage = $"Failed to deserialize response: {exception.Message}",
            Exception = exception,
            DurationMs = durationMs
        };
    }

    /// <summary>
    /// Creates a network error response.
    /// </summary>
    public static JsonWebResponse<T> NetworkError(
        string requestedUrl,
        HttpMethodType httpMethod,
        Exception exception,
        long durationMs)
    {
        return new JsonWebResponse<T>
        {
            RequestedUrl = requestedUrl,
            HttpMethod = httpMethod,
            HasNetworkError = true,
            ErrorMessage = $"Network error: {exception.Message}",
            Exception = exception,
            DurationMs = durationMs
        };
    }

    /// <summary>
    /// Creates a timeout error response.
    /// </summary>
    public static JsonWebResponse<T> TimeoutError(
        string requestedUrl,
        HttpMethodType httpMethod,
        Exception exception,
        long durationMs)
    {
        return new JsonWebResponse<T>
        {
            RequestedUrl = requestedUrl,
            HttpMethod = httpMethod,
            HasTimeout = true,
            ErrorMessage = $"Request timed out: {exception.Message}",
            Exception = exception,
            DurationMs = durationMs
        };
    }

    /// <summary>
    /// Maps the response to a different type using the provided mapping function.
    /// </summary>
    /// <typeparam name="TResult">The target type.</typeparam>
    /// <param name="mapper">The mapping function.</param>
    /// <returns>A new JsonWebResponse with the mapped data.</returns>
    public JsonWebResponse<TResult> Map<TResult>(Func<T, TResult> mapper)
    {
        return new JsonWebResponse<TResult>
        {
            RequestedUrl = RequestedUrl,
            HttpMethod = HttpMethod,
            StatusCode = StatusCode,
            ResponseText = ResponseText,
            Data = Data is not null ? mapper(Data) : default,
            IsCancelled = IsCancelled,
            HasDeserializationError = HasDeserializationError,
            HasNetworkError = HasNetworkError,
            HasTimeout = HasTimeout,
            ErrorMessage = ErrorMessage,
            Exception = Exception,
            DurationMs = DurationMs
        };
    }

    /// <summary>
    /// Maps the response to a different type, preserving error state when source data is null.
    /// </summary>
    /// <typeparam name="TResult">The target type.</typeparam>
    /// <param name="mapper">The mapping function.</param>
    /// <returns>A new JsonWebResponse with the mapped data or null if source was null/had errors.</returns>
    public JsonWebResponse<TResult?> MapOrNull<TResult>(Func<T, TResult?> mapper) where TResult : class
    {
        return new JsonWebResponse<TResult?>
        {
            RequestedUrl = RequestedUrl,
            HttpMethod = HttpMethod,
            StatusCode = StatusCode,
            ResponseText = ResponseText,
            Data = Data is not null ? mapper(Data) : null,
            IsCancelled = IsCancelled,
            HasDeserializationError = HasDeserializationError,
            HasNetworkError = HasNetworkError,
            HasTimeout = HasTimeout,
            ErrorMessage = ErrorMessage,
            Exception = Exception,
            DurationMs = DurationMs
        };
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== JsonWebResponse ===");
        sb.AppendLine($"URL: {RequestedUrl}");
        sb.AppendLine($"Method: {HttpMethod}");
        sb.AppendLine($"Duration: {DurationMs}ms");

        if (StatusCode.HasValue)
        {
            sb.AppendLine($"Status: {(int)StatusCode.Value} ({StatusCode.Value})");
        }
        else
        {
            sb.AppendLine("Status: No response received");
        }

        sb.AppendLine($"Success: {IsSuccess}");

        if (HasError)
        {
            sb.AppendLine("--- Error Details ---");
            
            if (IsCancelled)
            {
                sb.AppendLine("  - Cancelled: Yes");
            }

            if (HasHttpError)
            {
                sb.AppendLine("  - HTTP Error: Yes");
            }

            if (HasDeserializationError)
            {
                sb.AppendLine("  - Deserialization Error: Yes");
            }

            if (HasNetworkError)
            {
                sb.AppendLine("  - Network Error: Yes");
            }

            if (HasTimeout)
            {
                sb.AppendLine("  - Timeout: Yes");
            }

            if (!string.IsNullOrWhiteSpace(ErrorMessage))
            {
                sb.AppendLine($"  - Message: {ErrorMessage}");
            }

            if (Exception is not null)
            {
                sb.AppendLine($"  - Exception Type: {Exception.GetType().Name}");
                sb.AppendLine($"  - Exception Message: {Exception.Message}");
            }
        }

        if (!string.IsNullOrWhiteSpace(ResponseText))
        {
            var truncatedResponse = ResponseText.Length > 2000 
                ? ResponseText[..2000] + "... [TRUNCATED]" 
                : ResponseText;
            sb.AppendLine("--- Response Body ---");
            sb.AppendLine(truncatedResponse);
        }

        sb.AppendLine("======================");

        return sb.ToString();
    }

    private static bool IsSuccessStatusCode(HttpStatusCode statusCode)
    {
        return (int)statusCode >= 200 && (int)statusCode < 300;
    }
}

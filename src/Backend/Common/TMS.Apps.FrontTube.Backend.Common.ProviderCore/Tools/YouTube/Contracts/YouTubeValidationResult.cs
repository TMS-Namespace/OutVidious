namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tools.YouTube.Contracts;

/// <summary>
/// Represents the result of a YouTube identity validation.
/// </summary>
public sealed record YouTubeValidationResult
{
    /// <summary>
    /// Whether the validation passed.
    /// </summary>
    public required bool IsValid { get; init; }

    /// <summary>
    /// List of validation errors if validation failed.
    /// </summary>
    public IReadOnlyList<string> Errors { get; init; } = [];

    /// <summary>
    /// List of validation warnings (non-critical issues).
    /// </summary>
    public IReadOnlyList<string> Warnings { get; init; } = [];

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static YouTubeValidationResult Success() => new()
    {
        IsValid = true
    };

    /// <summary>
    /// Creates a successful validation result with warnings.
    /// </summary>
    public static YouTubeValidationResult SuccessWithWarnings(IReadOnlyList<string> warnings) => new()
    {
        IsValid = true,
        Warnings = warnings
    };

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    public static YouTubeValidationResult Failure(params string[] errors) => new()
    {
        IsValid = false,
        Errors = errors
    };

    /// <summary>
    /// Creates a failed validation result with errors.
    /// </summary>
    public static YouTubeValidationResult Failure(IReadOnlyList<string> errors) => new()
    {
        IsValid = false,
        Errors = errors
    };
}

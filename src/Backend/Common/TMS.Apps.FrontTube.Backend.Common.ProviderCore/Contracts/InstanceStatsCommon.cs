using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

/// <summary>
/// Instance statistics from the provider.
/// </summary>
public sealed record InstanceStatsCommon : ICommonContract
{
    /// <summary>
    /// Instance version string.
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    /// Software name (e.g., "Invidious").
    /// </summary>
    public string SoftwareName { get; init; } = string.Empty;

    /// <summary>
    /// Software version.
    /// </summary>
    public string SoftwareVersion { get; init; } = string.Empty;

    /// <summary>
    /// Git branch.
    /// </summary>
    public string? Branch { get; init; }

    /// <summary>
    /// Whether new user registration is open.
    /// </summary>
    public bool OpenRegistrations { get; init; }

    /// <summary>
    /// Total number of users.
    /// </summary>
    public long TotalUsers { get; init; }

    /// <summary>
    /// Users active in the last 6 months.
    /// </summary>
    public long ActiveHalfYearUsers { get; init; }

    /// <summary>
    /// Users active in the last month.
    /// </summary>
    public long ActiveMonthUsers { get; init; }

    /// <summary>
    /// Last update timestamp (Unix epoch).
    /// </summary>
    public long UpdatedAt { get; init; }

    /// <summary>
    /// Last channel refresh timestamp (Unix epoch).
    /// </summary>
    public long LastChannelRefreshedAt { get; init; }
}

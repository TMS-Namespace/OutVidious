using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.Mappers;

/// <summary>
/// Maps Invidious instance stats DTOs to common contracts.
/// </summary>
internal static class InstanceStatsMapper
{
    /// <summary>
    /// Maps instance stats DTO to common contract.
    /// </summary>
    public static InstanceStatsCommon ToInstanceStats(InstanceStats dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new InstanceStatsCommon
        {
            Version = dto.Version,
            SoftwareName = dto.Software.Name,
            SoftwareVersion = dto.Software.Version,
            Branch = dto.Software.Branch,
            OpenRegistrations = dto.OpenRegistrations,
            TotalUsers = dto.Usage.Users.Total,
            ActiveHalfYearUsers = dto.Usage.Users.ActiveHalfYear,
            ActiveMonthUsers = dto.Usage.Users.ActiveMonth,
            UpdatedAt = dto.Metadata.UpdatedAt,
            LastChannelRefreshedAt = dto.Metadata.LastChannelRefreshedAt
        };
    }
}

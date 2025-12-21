using Microsoft.AspNetCore.Components;
using TMS.Apps.Web.OutVidious.Core.Enums;
using TMS.Apps.Web.OutVidious.Core.ViewModels;

namespace TMS.Apps.Web.OutVidious.WebGUI.Components.Video;

/// <summary>
/// Code-behind for the Invidious video player component.
/// </summary>
public partial class InvidiousPlayerComponent : ComponentBase, IDisposable
{
    private bool _disposed;

    [Parameter]
    public VideoPlayerViewModel? ViewModel { get; set; }

    [Parameter]
    public string InvidiousBaseUrl { get; set; } = string.Empty;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (ViewModel != null)
        {
            ViewModel.StateChanged -= OnViewModelStateChanged;
            ViewModel.StateChanged += OnViewModelStateChanged;
        }
    }

    private void OnViewModelStateChanged(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    private void OnPlayerModeChanged(PlayerMode mode)
    {
        ViewModel?.SetPlayerMode(mode);
    }

    private void OnQualityChanged(string quality)
    {
        ViewModel?.SetQuality(quality);
    }

    private string? GetPosterUrl()
    {
        var thumbnail = ViewModel?.VideoDetails?.VideoThumbnails
            .FirstOrDefault(t => t.Quality == "maxres" || t.Quality == "sddefault");

        return thumbnail?.Url;
    }

    private string GetAuthorUrl()
    {
        if (string.IsNullOrEmpty(InvidiousBaseUrl) || ViewModel?.VideoDetails == null)
        {
            return "#";
        }

        return $"{InvidiousBaseUrl}/channel/{ViewModel.VideoDetails.AuthorId}";
    }

    private static string FormatViewCount(long count)
    {
        return count switch
        {
            >= 1_000_000_000 => $"{count / 1_000_000_000.0:F1}B",
            >= 1_000_000 => $"{count / 1_000_000.0:F1}M",
            >= 1_000 => $"{count / 1_000.0:F1}K",
            _ => count.ToString("N0")
        };
    }

    private static string FormatNumber(int count)
    {
        return count switch
        {
            >= 1_000_000 => $"{count / 1_000_000.0:F1}M",
            >= 1_000 => $"{count / 1_000.0:F1}K",
            _ => count.ToString("N0")
        };
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (ViewModel != null)
        {
            ViewModel.StateChanged -= OnViewModelStateChanged;
        }

        _disposed = true;
    }
}

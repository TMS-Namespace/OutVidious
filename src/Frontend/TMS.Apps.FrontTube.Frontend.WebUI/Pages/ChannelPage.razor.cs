using Microsoft.AspNetCore.Components;
using TMS.Apps.FrontTube.Frontend.WebUI.Services;

namespace TMS.Apps.FrontTube.Frontend.WebUI.Pages;

/// <summary>
/// Redirect page for channel URLs.
/// Opens the channel in the dock panel and navigates back to the home page.
/// </summary>
public partial class ChannelPage : ComponentBase
{
    [Inject]
    private Orchestrator Orchestrator { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private ILogger<ChannelPage> Logger { get; set; } = null!;

    /// <summary>
    /// The channel ID from the route.
    /// </summary>
    [Parameter]
    public string? ChannelId { get; set; }

    protected override void OnParametersSet()
    {
        if (!string.IsNullOrWhiteSpace(ChannelId))
        {
            Logger.LogDebug("[{MethodName}] Opening channel '{ChannelId}' in dock panel.", nameof(OnParametersSet), ChannelId);
            
            // Trigger channel opening in the dock panel via Orchestrator
            Orchestrator.OpenChannel(ChannelId);
            
            // Navigate back to home page (the channel will be shown in the dock panel)
            NavigationManager.NavigateTo("/", replace: true);
        }
    }
}

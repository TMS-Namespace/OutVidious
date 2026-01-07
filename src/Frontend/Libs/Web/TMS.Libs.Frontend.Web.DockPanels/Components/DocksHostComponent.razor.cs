

using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Linq;
using TMS.Libs.Frontend.Web.DockPanels.Enums;
using TMS.Libs.Frontend.Web.DockPanels.Models;
using TMS.Libs.Frontend.Web.DockPanels.Services;

namespace TMS.Libs.Frontend.Web.DockPanels.Components;

/// <summary>
/// Dock panels host component.
/// </summary>
public partial class DocksHostComponent : IDisposable
{
    private const int DefaultDelayBetweenOperationsMs = 30;
    private const int InitialDelayMs = 100;
    private const int DrawerConfigurationDelayMs = 100;
    private const string DefaultVersion = "v1";
    private const string DefaultLocalStoragePrefix = "bb-dockview";

    private readonly List<DockPanelComponentBase> _components = [];
    private readonly CancellationTokenSource _initializationCts = new();
    private DockPanelOptions _options = new();
    private bool _isInitialized;
    private bool _isDisposed;
    private ILogger? _logger;

    /// <summary>
    /// Gets or sets the dock panel host name for layout persistence.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the layout configuration JSON.
    /// </summary>
    [Parameter]
    public string? LayoutConfig { get; set; }

    /// <summary>
    /// Gets or sets whether to show the close button.
    /// </summary>
    [Parameter]
    public bool ShowClose { get; set; } = true;

    /// <summary>
    /// Gets or sets whether panels are locked.
    /// </summary>
    [Parameter]
    public bool IsLock { get; set; }

    /// <summary>
    /// Gets or sets whether to show the lock button.
    /// </summary>
    [Parameter]
    public bool ShowLock { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show the maximize button.
    /// </summary>
    [Parameter]
    public bool ShowMaximize { get; set; } = true;

    /// <summary>
    /// Gets or sets whether panels are floating.
    /// </summary>
    [Parameter]
    public bool IsFloating { get; set; }

    /// <summary>
    /// Gets or sets whether to show the float button.
    /// </summary>
    [Parameter]
    public bool ShowFloat { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show the pin button.
    /// </summary>
    [Parameter]
    public bool ShowPin { get; set; } = true;

    /// <summary>
    /// Gets or sets the render mode for panels.
    /// </summary>
    [Parameter]
    public DockPanelRenderMode Renderer { get; set; }

    /// <summary>
    /// Gets or sets the callback for lock state changes.
    /// </summary>
    [Parameter]
    public Func<IReadOnlyList<DockPanelComponent>, bool, Task>? OnLockChangedCallbackAsync { get; set; }

    /// <summary>
    /// Gets or sets the callback when panel visibility changes.
    /// </summary>
    [Parameter]
    public Func<DockPanelComponent, bool, Task>? OnPanelVisibleChangedAsync { get; set; }

    /// <summary>
    /// Gets or sets the callback when a panel is registered.
    /// </summary>
    [Parameter]
    public EventCallback<DockPanelComponent> OnPanelAddedAsync { get; set; }

    /// <summary>
    /// Gets or sets the callback when a drawer group is ready.
    /// </summary>
    [Parameter]
    public EventCallback<(DockPanelComponent Panel, Guid GroupId)> OnDrawerReadyAsync { get; set; }

    /// <summary>
    /// Gets or sets the callback when the splitter finishes resizing.
    /// </summary>
    [Parameter]
    public Func<Task>? OnSplitterCallbackAsync { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the JS module finishes initialization.
    /// </summary>
    [Parameter]
    public Func<Task>? OnInitializedCallbackAsync { get; set; }

    /// <summary>
    /// Gets or sets the child content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the layout version for local storage.
    /// </summary>
    [Parameter]
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets whether to enable local storage for layouts.
    /// </summary>
    [Parameter]
    public bool? EnableLocalStorage { get; set; }

    /// <summary>
    /// Gets or sets the local storage key prefix.
    /// </summary>
    [Parameter]
    public string? LocalStoragePrefix { get; set; }

    /// <summary>
    /// Gets or sets the dock panel theme.
    /// </summary>
    [Parameter]
    public DockPanelTheme Theme { get; set; } = DockPanelTheme.Light;

    /// <summary>
    /// Gets or sets the group configurations for initial drawer state.
    /// </summary>
    [Parameter]
    public IReadOnlyList<DockGroupConfiguration> GroupConfigurations { get; set; } = [];

    /// <summary>
    /// Gets or sets the default drawer width in pixels.
    /// </summary>
    [Parameter]
    public int DefaultDrawerWidthPx { get; set; } = 500;

    /// <summary>
    /// Gets or sets the sidebar width in pixels.
    /// </summary>
    [Parameter]
    public int? AsideWidthPx { get; set; }

    /// <summary>
    /// Gets or sets the sidebar button inline padding in pixels.
    /// </summary>
    [Parameter]
    public int? AsideButtonPaddingInlinePx { get; set; }

    /// <summary>
    /// Gets or sets the extra right-side inline padding for sidebar buttons in pixels.
    /// </summary>
    [Parameter]
    public int? AsideButtonPaddingInlineEndExtraPx { get; set; }

    /// <summary>
    /// Gets or sets the sidebar button block padding in pixels.
    /// </summary>
    [Parameter]
    public int? AsideButtonPaddingBlockPx { get; set; }

    /// <summary>
    /// Gets or sets the sidebar button gap between icon and title in pixels.
    /// </summary>
    [Parameter]
    public int? AsideButtonGapPx { get; set; }

    /// <summary>
    /// Gets or sets the gap between dock action buttons in pixels.
    /// </summary>
    [Parameter]
    public int? ActionButtonsGapPx { get; set; }

    /// <summary>
    /// Gets or sets the CSS class applied while initialization is in progress.
    /// </summary>
    [Parameter]
    public string? HiddenClass { get; set; }

    /// <summary>
    /// Gets or sets whether the layout is ready.
    /// </summary>
    [Parameter]
    public bool IsReady { get; set; }

    /// <summary>
    /// Gets or sets the callback when the ready state changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> IsReadyChanged { get; set; }

    /// <summary>
    /// Event raised when initialization is complete.
    /// </summary>
    [Parameter]
    public EventCallback OnInitializationCompleteAsync { get; set; }

    /// <summary>
    /// Event raised when a panel's visible state changes.
    /// </summary>
    [Parameter]
    public EventCallback<(DockPanelComponent Panel, bool IsVisible)> OnVisibleStateChangedAsync { get; set; }

    /// <summary>
    /// Event raised when a panel's active state changes.
    /// </summary>
    [Parameter]
    public EventCallback<(DockPanelComponent Panel, bool IsActive)> OnActiveStateChangedAsync { get; set; }

    [CascadingParameter]
    private DocksHostComponent? DockPanelsParent { get; set; }

    [Inject]
    private IConfiguration Configuration { get; set; } = null!;

    [Inject]
    private IThemeProvider ThemeProviderService { get; set; } = null!;

    [Inject]
    private ILoggerFactory LoggerFactory { get; set; } = null!;

    [Inject]
    private IDockPanelInterop DockPanelInterop { get; set; } = null!;

    private string? ClassString => CssBuilder.Default("bb-dockview")
        .AddClassFromAttributes(AdditionalAttributes)
        .AddClass(HiddenClass, !_isInitialized && !string.IsNullOrWhiteSpace(HiddenClass))
        .Build();

    private string DockPanelId => Id ?? Name ?? string.Empty;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();

        _logger = LoggerFactory.CreateLogger<DocksHostComponent>();

        var section = Configuration.GetSection(nameof(DockPanelOptions));
        _options = section.Exists() ? section.Get<DockPanelOptions>() ?? new DockPanelOptions() : new DockPanelOptions();

        ThemeProviderService.ThemeChangedAsync += OnThemeChangedAsync;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="firstRender"></param>
    /// <returns></returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (!firstRender)
        {
            try
            {
                await InvokeVoidAsync("update", Id, GetOptions());
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "[{MethodName}] Unexpected error: Failed to update dock panels for instance '{DockPanelId}'.",
                    nameof(OnAfterRenderAsync),
                    DockPanelId);
                throw;
            }
        }
    }

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    protected override async Task InvokeInitAsync()
    {
        try
        {
            await InvokeVoidAsync("init", Id, Interop, GetOptions());
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                ex,
                "[{MethodName}] Unexpected error: Failed to initialize dock panels for instance '{DockPanelId}'.",
                nameof(InvokeInitAsync),
                DockPanelId);
            throw;
        }
    }

    private DockPanelConfig GetOptions() => new()
    {
        EnableLocalStorage = EnableLocalStorage ?? _options.EnableLocalStorage ?? false,
        LocalStorageKey = $"{GetPrefixKey()}-{Name}-{GetVersion()}",
        IsLock = IsLock,
        ShowLock = ShowLock,
        IsFloating = IsFloating,
        ShowFloat = ShowFloat,
        ShowClose = ShowClose,
        ShowPin = ShowPin,
        ShowMaximize = ShowMaximize,
        Renderer = Renderer,
        LayoutConfig = LayoutConfig,
        Theme = Theme.ToDescriptionString(),
        AsideWidthPx = AsideWidthPx > 0 ? AsideWidthPx : null,
        AsideButtonPaddingInlinePx = AsideButtonPaddingInlinePx > 0 ? AsideButtonPaddingInlinePx : null,
        AsideButtonPaddingInlineEndExtraPx = AsideButtonPaddingInlineEndExtraPx > 0 ? AsideButtonPaddingInlineEndExtraPx : null,
        AsideButtonPaddingBlockPx = AsideButtonPaddingBlockPx > 0 ? AsideButtonPaddingBlockPx : null,
        AsideButtonGapPx = AsideButtonGapPx > 0 ? AsideButtonGapPx : null,
        ActionButtonsGapPx = ActionButtonsGapPx > 0 ? ActionButtonsGapPx : null,
        InitializedCallback = nameof(InitializedCallbackAsync),
        PanelVisibleChangedCallback = nameof(PanelVisibleChangedCallbackAsync),
        PanelActiveChangedCallback = nameof(PanelActiveChangedCallbackAsync),
        PanelAddedCallback = nameof(PanelAddedCallbackAsync),
        DrawerReadyCallback = nameof(DrawerReadyCallbackAsync),
        LockChangedCallback = nameof(LockChangedCallbackAsync),
        SplitterCallback = nameof(SplitterCallbackAsync),
        Contents = _components
    };

    private string GetVersion() => Version ?? _options.Version ?? DefaultVersion;

    private string GetPrefixKey() => LocalStoragePrefix ?? _options.LocalStoragePrefix ?? DefaultLocalStoragePrefix;

    /// <summary>
    /// Resets the layout to the default configuration.
    /// </summary>
    /// <param name="layoutConfig">Optional layout JSON override.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task ResetAsync(string? layoutConfig, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var options = GetOptions();
        if (layoutConfig != null)
        {
            options.LayoutConfig = layoutConfig;
        }

        try
        {
            await InvokeVoidAsync("reset", Id, options);
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                ex,
                "[{MethodName}] Unexpected error: Failed to reset dock panels for instance '{DockPanelId}'.",
                nameof(ResetAsync),
                DockPanelId);
            throw;
        }
    }

    /// <summary>
    /// Saves the current layout as JSON.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The layout JSON.</returns>
    public async Task<string?> SaveLayoutAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            return await InvokeAsync<string?>("save", Id);
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                ex,
                "[{MethodName}] Unexpected error: Failed to save dock panel layout for instance '{DockPanelId}'.",
                nameof(SaveLayoutAsync),
                DockPanelId);
            throw;
        }
    }

    private Task OnThemeChangedAsync(string themeName)
    {
        Theme = themeName == "dark" ? DockPanelTheme.Dark : DockPanelTheme.Light;
        return Task.CompletedTask;
    }

    /// <summary>
    /// JS-invokable initialization callback.
    /// </summary>
    [JSInvokable]
    public async Task InitializedCallbackAsync()
    {
        if (OnInitializedCallbackAsync != null)
        {
            await OnInitializedCallbackAsync();
        }

        await ApplyInitialConfigurationAsync(_initializationCts.Token);
    }

    /// <summary>
    /// JS-invokable visibility callback.
    /// </summary>
    [JSInvokable]
    public async Task PanelVisibleChangedCallbackAsync(Guid panelId, bool status)
    {
        var panelComponent = FindPanelComponent(panelId);
        if (panelComponent is null)
        {
            _logger?.LogDebug(
                "[{MethodName}] Panel '{PanelId}' not found for visibility change.",
                nameof(PanelVisibleChangedCallbackAsync),
                panelId);
            return;
        }

        if (OnPanelVisibleChangedAsync != null)
        {
            await OnPanelVisibleChangedAsync(panelComponent, status);
        }

        if (OnVisibleStateChangedAsync.HasDelegate)
        {
            await OnVisibleStateChangedAsync.InvokeAsync((panelComponent, status));
        }
    }

    /// <summary>
    /// JS-invokable active state callback.
    /// </summary>
    [JSInvokable]
    public async Task PanelActiveChangedCallbackAsync(Guid panelId, bool isActive)
    {
        var panelComponent = FindPanelComponent(panelId);
        if (panelComponent != null)
        {
            await panelComponent.SetActiveStateAsync(isActive);
        }
        else
        {
            _logger?.LogDebug(
                "[{MethodName}] Panel '{PanelId}' not found for active change.",
                nameof(PanelActiveChangedCallbackAsync),
                panelId);
            return;
        }

        if (OnActiveStateChangedAsync.HasDelegate)
        {
            await OnActiveStateChangedAsync.InvokeAsync((panelComponent, isActive));
        }
    }

    private DockPanelComponent? FindPanelComponent(Guid panelId)
    {
        return FindPanelComponent(_components, panelId);
    }

    private static DockPanelComponent? FindPanelComponent(
        IEnumerable<DockPanelComponentBase> components,
        Guid panelId)
    {
        foreach (var component in components)
        {
            if (component is DockPanelComponent panel)
            {
                if (panel.PanelId == panelId)
                {
                    return panel;
                }
            }
            else if (component is DocksCollectionComponent content)
            {
                var found = FindPanelComponent(content.Items, panelId);
                if (found != null)
                {
                    return found;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// JS-invokable lock callback.
    /// </summary>
    [JSInvokable]
    public async Task LockChangedCallbackAsync(Guid[] panels, bool state)
    {
        if (OnLockChangedCallbackAsync != null)
        {
            var resolvedPanels = panels
                .Select(panelId => FindPanelComponent(panelId))
                .Where(panel => panel is not null)
                .Cast<DockPanelComponent>()
                .ToArray();

            await OnLockChangedCallbackAsync(resolvedPanels, state);
        }
    }

    /// <summary>
    /// JS-invokable splitter callback.
    /// </summary>
    [JSInvokable]
    public async Task SplitterCallbackAsync()
    {
        if (OnSplitterCallbackAsync != null)
        {
            await OnSplitterCallbackAsync();
        }
    }

    private void Dispose(bool disposing)
    {
        if (!disposing || _isDisposed)
        {
            return;
        }

        _initializationCts.Cancel();
        _initializationCts.Dispose();

        CancelPendingWaiters();

        ThemeProviderService.ThemeChangedAsync -= OnThemeChangedAsync;

        _isDisposed = true;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

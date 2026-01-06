using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace TMS.Apps.FrontTube.Frontend.WebUI.Services;

/// <summary>
/// Captures browser console messages and logs them via Serilog.
/// </summary>
internal sealed class BrowserConsoleCapture : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<BrowserConsoleCapture> _logger;
    private IJSObjectReference? _module;
    private DotNetObjectReference<BrowserConsoleCapture>? _dotNetRef;
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserConsoleCapture"/> class.
    /// </summary>
    /// <param name="jsRuntime">The JS runtime for interop calls.</param>
    /// <param name="logger">The logger instance.</param>
    public BrowserConsoleCapture(IJSRuntime jsRuntime, ILogger<BrowserConsoleCapture> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    /// <summary>
    /// Initializes the browser console capture.
    /// Must be called after the component is rendered.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (_module is not null)
        {
            return;
        }

        try
        {
            _module = await _jsRuntime.InvokeAsync<IJSObjectReference>(
                "import",
                cancellationToken,
                "./js/browser-console-capture.js");

            _dotNetRef = DotNetObjectReference.Create(this);
            await _module.InvokeVoidAsync("initialize", cancellationToken, _dotNetRef);

            _logger.LogInformation("[{MethodName}] Browser console capture initialized.", nameof(InitializeAsync));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{MethodName}] Failed to initialize browser console capture.", nameof(InitializeAsync));
        }
    }

    /// <summary>
    /// Called from JavaScript when a browser console message is captured.
    /// </summary>
    /// <param name="level">The log level.</param>
    /// <param name="message">The log message.</param>
    [JSInvokable]
    public void OnBrowserConsoleMessage(string level, string message)
    {
        var formattedMessage = "[Browser] {Message}";

        switch (level)
        {
            case "Error":
                _logger.LogError(formattedMessage, message);
                break;
            case "Warning":
                _logger.LogWarning(formattedMessage, message);
                break;
            case "Debug":
                _logger.LogDebug(formattedMessage, message);
                break;
            case "Information":
            default:
                _logger.LogInformation(formattedMessage, message);
                break;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        if (_module is not null)
        {
            try
            {
                await _module.InvokeVoidAsync("dispose");
                await _module.DisposeAsync();
            }
            catch
            {
                // Ignore disposal errors
            }

            _module = null;
        }

        _dotNetRef?.Dispose();
        _dotNetRef = null;

        _isDisposed = true;
    }
}

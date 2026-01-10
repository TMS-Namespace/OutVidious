using Microsoft.Extensions.Logging;

namespace TMS.Apps.FrontTube.Backend.Repository.YouTubeRSS.Tests.Integration.Tools;

/// <summary>
/// Base class for integration tests providing common setup and teardown.
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    protected readonly YouTubeRssVideoFetcher Fetcher;
    protected readonly ILoggerFactory LoggerFactory;
    private readonly TestHttpClientFactory _httpClientFactory;

    protected IntegrationTestBase()
    {
        LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        _httpClientFactory = new TestHttpClientFactory();
        var httpClient = _httpClientFactory.CreateClient("YouTubeRSS");

        Fetcher = new YouTubeRssVideoFetcher(httpClient, LoggerFactory);
    }

    public void Dispose()
    {
        LoggerFactory.Dispose();
        GC.SuppressFinalize(this);
    }
}

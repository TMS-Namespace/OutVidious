using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Configuration;
using TMS.Apps.FrontTube.Backend.Providers.Invidious;

namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.Tests.Integration.Tools;

/// <summary>
/// Base class for integration tests providing common setup and teardown.
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    protected readonly InvidiousVideoProvider Provider;
    protected readonly ILoggerFactory LoggerFactory;

    protected IntegrationTestBase()
    {
        LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        var httpClientFactory = new TestHttpClientFactory();
        var config = new ProviderConfig
        {
            BaseUri = new Uri(TestConstants.InvidiousInstanceUrl),
            TimeoutSeconds = 30,
            BypassSslValidation = true
        };

        Provider = new InvidiousVideoProvider(LoggerFactory, httpClientFactory, config);
    }

    public void Dispose()
    {
        Provider.Dispose();
        LoggerFactory.Dispose();
        GC.SuppressFinalize(this);
    }
}

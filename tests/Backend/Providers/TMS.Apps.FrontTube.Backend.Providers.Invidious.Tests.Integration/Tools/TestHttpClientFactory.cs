using System.Net;
using System.Net.Http;

namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.Tests.Integration.Tools;

/// <summary>
/// HTTP client factory for integration tests, configured to work with self-signed certificates.
/// </summary>
internal sealed class TestHttpClientFactory : IHttpClientFactory
{
    private readonly HttpClient _httpClient;

    public TestHttpClientFactory()
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        _httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/json, */*;q=0.8");
        _httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
        _httpClient.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
    }

    public HttpClient CreateClient(string name)
    {
        return _httpClient;
    }
}

using Microsoft.AspNetCore.Mvc.Testing;

namespace FirstContact.Api.Tests;

/// <summary>
/// Starts the real application in memory and calls it over HTTP.
/// This proves the whole pipeline works, not just one class.
/// </summary>
public class HealthEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Health_reports_that_the_api_is_running()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/health");
        string body = await response.Content.ReadAsStringAsync();

        Assert.True(response.IsSuccessStatusCode);
        Assert.Contains("ok", body, StringComparison.Ordinal);
    }
}

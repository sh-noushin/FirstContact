using FirstContact.Api.Tests.Fakes;

namespace FirstContact.Api.Tests;

/// <summary>
/// Starts the real application in memory and calls it over HTTP.
/// This proves the whole pipeline works, not just one class.
/// </summary>
public class HealthEndpointTests(TestApi api) : IClassFixture<TestApi>
{
    [Fact]
    public async Task Health_reports_that_the_api_is_running()
    {
        HttpClient client = api.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/health");
        string body = await response.Content.ReadAsStringAsync();

        Assert.True(response.IsSuccessStatusCode);
        Assert.Contains("ok", body, StringComparison.Ordinal);
    }
}

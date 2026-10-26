using System.Net;
using System.Net.Http.Json;
using FirstContact.Api.Contracts;
using FirstContact.Api.Tests.Fakes;
using FirstContact.Domain.Requests;

namespace FirstContact.Api.Tests.Endpoints;

/// <summary>The review queue and the decision an employee makes on it, over HTTP.</summary>
public class ReviewEndpointsTests : IClassFixture<TestApi>
{
    private readonly TestApi _api;
    private readonly HttpClient _client;

    public ReviewEndpointsTests(TestApi api)
    {
        _api = api;
        _client = api.CreateClient();
    }

    [Fact]
    public async Task An_unsure_request_appears_in_the_queue_and_leaves_it_once_resolved()
    {
        RequestView waiting = await UnsureAsync("Der Export dauert ewig, geht das schneller?");
        Assert.Equal("HumanReview", waiting.Destination);

        RequestView[] queue = await QueueAsync();
        Assert.Contains(queue, item => item.Id == waiting.Id);

        RequestView resolved = await ResolveAsync(waiting.Id, "Problem");
        Assert.Equal("SupportTeam", resolved.Destination);
        Assert.Equal("Routed", resolved.Status);
        Assert.True(resolved.DecidedByHuman);

        RequestView[] afterwards = await QueueAsync();
        Assert.DoesNotContain(afterwards, item => item.Id == waiting.Id);
    }

    [Fact]
    public async Task A_confident_request_never_reaches_the_queue()
    {
        _api.Classifier.Answer = Of(RequestType.Question, 0.95);
        RequestView routed = await PostAsync("Wie exportiere ich ein Dokument?");

        RequestView[] queue = await QueueAsync();

        Assert.DoesNotContain(queue, item => item.Id == routed.Id);
    }

    [Fact]
    public async Task An_unknown_request_returns_not_found()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/requests/9999/resolve", new ReviewDecision("Problem"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task An_unknown_type_is_rejected()
    {
        RequestView waiting = await UnsureAsync("Noch eine unklare Nachricht");

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            $"/api/requests/{waiting.Id}/resolve", new ReviewDecision("banana"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Resolving_the_same_request_twice_is_refused()
    {
        RequestView waiting = await UnsureAsync("Eine weitere unklare Nachricht");
        await ResolveAsync(waiting.Id, "Problem");

        HttpResponseMessage second = await _client.PostAsJsonAsync(
            $"/api/requests/{waiting.Id}/resolve", new ReviewDecision("Other"));

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    private async Task<RequestView> UnsureAsync(string message)
    {
        _api.Classifier.Answer = Of(RequestType.Other, 0.30);

        return await PostAsync(message);
    }

    private async Task<RequestView> PostAsync(string message)
    {
        HttpResponseMessage response =
            await _client.PostAsJsonAsync("/api/requests", new NewRequest(message));

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<RequestView>())!;
    }

    private async Task<RequestView[]> QueueAsync() =>
        (await _client.GetFromJsonAsync<RequestView[]>("/api/requests/review"))!;

    private async Task<RequestView> ResolveAsync(int id, string type)
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            $"/api/requests/{id}/resolve", new ReviewDecision(type));

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<RequestView>())!;
    }

    private static Classification Of(RequestType type, double confidence) =>
        Classification.Create(type, Confidence.Create(confidence), Urgency.Medium, "Testfall");
}

using System.Net;
using System.Net.Http.Json;
using FirstContact.Api.Contracts;
using FirstContact.Api.Tests.Fakes;
using FirstContact.Domain.Requests;

namespace FirstContact.Api.Tests.Endpoints;

/// <summary>
/// Calls the real API over HTTP: routing, the database and the JSON shape all take part.
/// Only the classifier is a stub, so no Azure call is made.
/// </summary>
public class RequestEndpointsTests : IClassFixture<TestApi>
{
    private readonly TestApi _api;
    private readonly HttpClient _client;

    public RequestEndpointsTests(TestApi api)
    {
        _api = api;
        _client = api.CreateClient();
    }

    [Fact]
    public async Task A_confident_question_comes_back_routed_to_self_service()
    {
        _api.Classifier.Answer = Of(RequestType.Question, 0.95);

        RequestView view = await PostAsync("Wie exportiere ich ein Dokument?");

        Assert.True(view.Id > 0);
        Assert.Equal("Question", view.Type);
        Assert.Equal("SelfService", view.Destination);
        Assert.Equal("Routed", view.Status);
        Assert.Equal(0.95, view.Confidence, 6);
        Assert.False(view.DecidedByHuman);
    }

    [Fact]
    public async Task An_unsure_message_comes_back_waiting_for_a_person()
    {
        _api.Classifier.Answer = Of(RequestType.Problem, 0.4);

        RequestView view = await PostAsync("Der Export dauert ewig, geht das schneller?");

        Assert.Equal("HumanReview", view.Destination);
        Assert.Equal("AwaitingReview", view.Status);
    }

    [Fact]
    public async Task The_reason_is_passed_through_so_staff_can_see_it()
    {
        _api.Classifier.Answer = Classification.Create(
            RequestType.Problem, Confidence.Create(0.88), Urgency.High, "Kunde nennt eine Fehlermeldung");

        RequestView view = await PostAsync("Zertifikat ungueltig!");

        Assert.Equal("Kunde nennt eine Fehlermeldung", view.Reason);
        Assert.Equal("High", view.Urgency);
    }

    [Fact]
    public async Task The_message_is_stored_trimmed()
    {
        RequestView view = await PostAsync("   Wie exportiere ich?   ");

        Assert.Equal("Wie exportiere ich?", view.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task An_empty_message_is_rejected(string message)
    {
        HttpResponseMessage response =
            await _client.PostAsJsonAsync("/api/requests", new NewRequest(message));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task A_message_that_is_too_long_is_rejected()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/requests", new NewRequest(new string('a', 4001)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<RequestView> PostAsync(string message)
    {
        HttpResponseMessage response =
            await _client.PostAsJsonAsync("/api/requests", new NewRequest(message));

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<RequestView>())!;
    }

    private static Classification Of(RequestType type, double confidence) =>
        Classification.Create(type, Confidence.Create(confidence), Urgency.Medium, "Testfall");
}

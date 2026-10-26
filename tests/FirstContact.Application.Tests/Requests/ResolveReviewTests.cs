using FirstContact.Application.Requests;
using FirstContact.Application.Tests.Fakes;
using FirstContact.Domain.Requests;

namespace FirstContact.Application.Tests.Requests;

/// <summary>
/// After a person decides, the request takes the ordinary route for that type, so the
/// manual and the automatic path cannot drift apart.
/// </summary>
public class ResolveReviewTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 26, 9, 0, 0, TimeSpan.Zero);

    private readonly InMemoryRequestRepository _repository = new();

    [Theory]
    [InlineData(RequestType.Problem, Destination.SupportTeam)]
    [InlineData(RequestType.FeatureRequest, Destination.ProductTeam)]
    [InlineData(RequestType.Question, Destination.SelfService)]
    [InlineData(RequestType.Other, Destination.SupportTeam)]
    public async Task A_resolved_request_goes_where_its_chosen_type_belongs(
        RequestType chosen, Destination expected)
    {
        await WaitingForReviewAsync();

        CustomerRequest resolved = await Resolve(1, chosen);

        Assert.Equal(expected, resolved.Destination);
        Assert.Equal(RequestStatus.Routed, resolved.Status);
        Assert.True(resolved.DecidedByHuman);
    }

    [Fact]
    public async Task The_original_confidence_is_kept_so_the_disagreement_stays_visible()
    {
        await WaitingForReviewAsync();

        CustomerRequest resolved = await Resolve(1, RequestType.Problem);

        Assert.Equal(0.30, resolved.Classification!.Confidence.Value, 6);
    }

    [Fact]
    public async Task Resolving_saves_the_change()
    {
        await WaitingForReviewAsync();
        int savesBefore = _repository.Saves;

        await Resolve(1, RequestType.Problem);

        Assert.Equal(savesBefore + 1, _repository.Saves);
    }

    [Fact]
    public async Task An_unknown_request_is_reported_as_missing()
    {
        await Assert.ThrowsAsync<RequestNotFoundException>(() => Resolve(99, RequestType.Problem));
    }

    [Fact]
    public async Task A_request_that_was_already_routed_cannot_be_resolved()
    {
        CustomerRequest routed = CustomerRequest.Receive("Eine klare Frage", Now);
        routed.Classify(Of(RequestType.Question, 0.95));
        routed.RouteTo(Destination.SelfService);
        await _repository.AddAsync(routed, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(() => Resolve(1, RequestType.Problem));
    }

    [Fact]
    public async Task The_queue_lists_only_requests_waiting_for_review_oldest_first()
    {
        await WaitingForReviewAsync("Zweite Nachricht", Now.AddHours(1));
        await WaitingForReviewAsync("Erste Nachricht", Now);

        CustomerRequest answered = CustomerRequest.Receive("Beantwortet", Now);
        answered.Classify(Of(RequestType.Question, 0.95));
        answered.RouteTo(Destination.SelfService);
        await _repository.AddAsync(answered, CancellationToken.None);

        IReadOnlyList<CustomerRequest> queue =
            await new GetReviewQueue(_repository).HandleAsync(CancellationToken.None);

        Assert.Equal(2, queue.Count);
        Assert.Equal("Erste Nachricht", queue[0].Message);
        Assert.Equal("Zweite Nachricht", queue[1].Message);
    }

    private Task<CustomerRequest> Resolve(int id, RequestType type) =>
        new ResolveReview(_repository).HandleAsync(id, type, CancellationToken.None);

    private async Task WaitingForReviewAsync(string message = "Unklare Nachricht", DateTimeOffset? at = null)
    {
        CustomerRequest request = CustomerRequest.Receive(message, at ?? Now);
        request.Classify(Of(RequestType.Other, 0.30));
        request.SendToHumanReview();

        await _repository.AddAsync(request, CancellationToken.None);
    }

    private static Classification Of(RequestType type, double confidence) =>
        Classification.Create(type, Confidence.Create(confidence), Urgency.Medium, "Testfall");
}

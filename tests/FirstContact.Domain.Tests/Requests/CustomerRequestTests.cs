using FirstContact.Domain.Requests;

namespace FirstContact.Domain.Tests.Requests;

/// <summary>
/// The entity exists to make two mistakes impossible: acting on an unclassified
/// request, and routing one twice.
/// </summary>
public class CustomerRequestTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 25, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void A_new_request_is_not_classified_and_has_no_destination()
    {
        CustomerRequest request = Received();

        Assert.Equal(RequestStatus.New, request.Status);
        Assert.Null(request.Classification);
        Assert.Null(request.Destination);
        Assert.False(request.DecidedByHuman);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void A_request_needs_a_message(string? message)
    {
        Assert.Throws<ArgumentException>(() => CustomerRequest.Receive(message!, Now));
    }

    [Fact]
    public void Surrounding_whitespace_is_removed_from_the_message()
    {
        Assert.Equal("Wie exportiere ich?", CustomerRequest.Receive("  Wie exportiere ich?  ", Now).Message);
    }

    [Fact]
    public void Classifying_records_the_result()
    {
        CustomerRequest request = Received();

        request.Classify(Of(RequestType.Problem, 0.91));

        Assert.Equal(RequestStatus.Classified, request.Status);
        Assert.Equal(RequestType.Problem, request.Classification!.Type);
    }

    [Fact]
    public void An_unclassified_request_cannot_be_routed()
    {
        CustomerRequest request = Received();

        Assert.Throws<InvalidOperationException>(() => request.RouteTo(Destination.SupportTeam));
        Assert.Throws<InvalidOperationException>(request.SendToHumanReview);
    }

    [Fact]
    public void A_routed_request_cannot_be_routed_again()
    {
        CustomerRequest request = Classified();
        request.RouteTo(Destination.SupportTeam);

        Assert.Equal(RequestStatus.Routed, request.Status);
        Assert.Equal(Destination.SupportTeam, request.Destination);
        Assert.Throws<InvalidOperationException>(() => request.RouteTo(Destination.ProductTeam));
    }

    [Fact]
    public void A_routed_request_cannot_be_reclassified()
    {
        CustomerRequest request = Classified();
        request.RouteTo(Destination.SelfService);

        Assert.Throws<InvalidOperationException>(() => request.Classify(Of(RequestType.Problem, 0.9)));
    }

    [Fact]
    public void An_unsure_request_waits_for_a_person_and_can_then_be_routed()
    {
        CustomerRequest request = Classified(RequestType.Other, 0.30);

        request.SendToHumanReview();
        Assert.Equal(RequestStatus.AwaitingReview, request.Status);
        Assert.Equal(Destination.HumanReview, request.Destination);

        request.ClassifyByHuman(RequestType.FeatureRequest);
        Assert.True(request.DecidedByHuman);
        Assert.Equal(RequestType.FeatureRequest, request.Classification!.Type);

        request.RouteTo(Destination.ProductTeam);
        Assert.Equal(RequestStatus.Routed, request.Status);
    }

    [Fact]
    public void Only_a_request_waiting_for_review_can_be_reclassified_by_a_person()
    {
        CustomerRequest request = Classified();

        Assert.Throws<InvalidOperationException>(() => request.ClassifyByHuman(RequestType.Problem));
    }

    [Fact]
    public void A_human_decision_keeps_the_original_confidence_and_reason()
    {
        CustomerRequest request = Classified(RequestType.Other, 0.30);
        request.SendToHumanReview();

        request.ClassifyByHuman(RequestType.Problem);

        Assert.Equal(0.30, request.Classification!.Confidence.Value, 6);
        Assert.Equal("Testfall", request.Classification.Reason);
    }

    private static CustomerRequest Received() =>
        CustomerRequest.Receive("Wie exportiere ich ein Dokument?", Now);

    private static CustomerRequest Classified(RequestType type = RequestType.Question, double confidence = 0.95)
    {
        CustomerRequest request = Received();
        request.Classify(Of(type, confidence));

        return request;
    }

    private static Classification Of(RequestType type, double confidence) =>
        Classification.Create(type, Confidence.Create(confidence), Urgency.Medium, "Testfall");
}

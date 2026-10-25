using FirstContact.Domain.Requests;

namespace FirstContact.Domain.Tests.Requests;

/// <summary>
/// The confidence rule is the system's main safeguard, so its boundary is pinned
/// in both directions rather than left to the implementation.
/// </summary>
public class RoutingPolicyTests
{
    [Theory]
    [InlineData(0.00)]
    [InlineData(0.50)]
    [InlineData(0.69)]
    [InlineData(0.6999)]
    public void Below_the_threshold_a_person_decides(double confidence)
    {
        Classification classification = Of(RequestType.Question, confidence);

        Assert.True(RoutingPolicy.NeedsHuman(classification));
        Assert.Equal(Destination.HumanReview, RoutingPolicy.Decide(classification));
    }

    [Fact]
    public void Exactly_at_the_threshold_the_system_acts()
    {
        // The configured number reads as "at least this sure", so 0.70 must not go to review.
        Classification classification = Of(RequestType.Question, 0.70);

        Assert.False(RoutingPolicy.NeedsHuman(classification));
        Assert.Equal(Destination.SelfService, RoutingPolicy.Decide(classification));
    }

    [Theory]
    [InlineData(RequestType.Question, Destination.SelfService)]
    [InlineData(RequestType.Problem, Destination.SupportTeam)]
    [InlineData(RequestType.FeatureRequest, Destination.ProductTeam)]
    [InlineData(RequestType.Other, Destination.SupportTeam)]
    public void Each_type_has_a_destination(RequestType type, Destination expected)
    {
        Assert.Equal(expected, RoutingPolicy.DestinationFor(type));
        Assert.Equal(expected, RoutingPolicy.Decide(Of(type, 0.95)));
    }

    [Fact]
    public void The_threshold_can_be_configured()
    {
        Classification classification = Of(RequestType.Question, 0.80);

        Assert.Equal(Destination.SelfService, RoutingPolicy.Decide(classification, threshold: 0.70));
        Assert.Equal(Destination.HumanReview, RoutingPolicy.Decide(classification, threshold: 0.90));
    }

    [Fact]
    public void An_unclear_classification_always_reaches_a_person()
    {
        Assert.Equal(
            Destination.HumanReview,
            RoutingPolicy.Decide(Classification.Unclear("nicht auswertbar"), threshold: 0.01));
    }

    private static Classification Of(RequestType type, double confidence) =>
        Classification.Create(type, Confidence.Create(confidence), Urgency.Medium, "Testfall");
}

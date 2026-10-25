using FirstContact.Domain.Requests;

namespace FirstContact.Domain.Tests.Requests;

public class ClassificationTests
{
    [Fact]
    public void It_keeps_what_the_classifier_decided()
    {
        Classification classification = Classification.Create(
            RequestType.Problem, Confidence.Create(0.91), Urgency.High, "  Fehlermeldung genannt  ");

        Assert.Equal(RequestType.Problem, classification.Type);
        Assert.Equal(0.91, classification.Confidence.Value, 6);
        Assert.Equal(Urgency.High, classification.Urgency);
        Assert.Equal("Fehlermeldung genannt", classification.Reason);
    }

    [Fact]
    public void A_missing_reason_becomes_an_empty_string_not_null()
    {
        Classification classification = Classification.Create(
            RequestType.Other, Confidence.None, Urgency.Low, null!);

        Assert.Equal(string.Empty, classification.Reason);
    }

    [Fact]
    public void An_unclear_result_has_no_confidence_so_a_person_decides()
    {
        Classification classification = Classification.Unclear("Antwort war nicht auswertbar");

        Assert.Equal(Confidence.None, classification.Confidence);
        Assert.Equal(RequestType.Other, classification.Type);
        Assert.True(RoutingPolicy.NeedsHuman(classification));
    }
}

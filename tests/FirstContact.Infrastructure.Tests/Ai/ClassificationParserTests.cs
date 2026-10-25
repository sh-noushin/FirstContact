using FirstContact.Domain.Requests;
using FirstContact.Infrastructure.Ai;

namespace FirstContact.Infrastructure.Tests.Ai;

/// <summary>
/// The model is untrusted input. Whatever it returns, the parser must produce a usable
/// classification, and anything it cannot read must end up with a person.
/// </summary>
public class ClassificationParserTests
{
    [Fact]
    public void It_reads_a_well_formed_answer()
    {
        Classification result = ClassificationParser.Parse(
            """{"type":"problem","confidence":0.91,"urgency":"high","reason":"Fehlermeldung genannt"}""");

        Assert.Equal(RequestType.Problem, result.Type);
        Assert.Equal(0.91, result.Confidence.Value, 6);
        Assert.Equal(Urgency.High, result.Urgency);
        Assert.Equal("Fehlermeldung genannt", result.Reason);
    }

    [Theory]
    [InlineData("question", RequestType.Question)]
    [InlineData("problem", RequestType.Problem)]
    [InlineData("feature_request", RequestType.FeatureRequest)]
    [InlineData("other", RequestType.Other)]
    [InlineData("QUESTION", RequestType.Question)]
    [InlineData("  problem  ", RequestType.Problem)]
    public void It_accepts_every_type_regardless_of_case_or_spacing(string value, RequestType expected)
    {
        Assert.Equal(expected, Parse(type: value).Type);
    }

    [Fact]
    public void An_unknown_type_becomes_other_rather_than_an_error()
    {
        Assert.Equal(RequestType.Other, Parse(type: "banana").Type);
    }

    [Theory]
    [InlineData("high", Urgency.High)]
    [InlineData("low", Urgency.Low)]
    [InlineData("medium", Urgency.Medium)]
    [InlineData("dringend", Urgency.Medium)]
    public void An_unknown_urgency_becomes_medium(string value, Urgency expected)
    {
        Assert.Equal(expected, Parse(urgency: value).Urgency);
    }

    [Fact]
    public void Prose_and_code_fences_around_the_json_are_ignored()
    {
        Classification result = ClassificationParser.Parse(
            """
            Gerne! Hier ist das Ergebnis:
            ```json
            {"type":"question","confidence":0.8,"urgency":"low","reason":"Bedienungsfrage"}
            ```
            Ich hoffe, das hilft.
            """);

        Assert.Equal(RequestType.Question, result.Type);
        Assert.Equal(0.8, result.Confidence.Value, 6);
    }

    [Fact]
    public void A_confidence_above_one_is_clamped_instead_of_rejected()
    {
        Assert.Equal(1.0, Parse(confidence: "1.4").Confidence.Value, 6);
    }

    [Fact]
    public void A_confidence_sent_as_text_is_still_read()
    {
        Assert.Equal(0.65, Parse(confidence: "\"0.65\"").Confidence.Value, 6);
    }

    [Theory]
    [InlineData("\"hoch\"")]
    [InlineData("null")]
    [InlineData("true")]
    public void An_unreadable_confidence_counts_as_zero_so_a_person_decides(string confidence)
    {
        Classification result = Parse(confidence: confidence);

        Assert.Equal(0, result.Confidence.Value);
        Assert.True(RoutingPolicy.NeedsHuman(result));
    }

    [Fact]
    public void A_missing_field_does_not_break_the_answer()
    {
        Classification result = ClassificationParser.Parse("""{"type":"problem"}""");

        Assert.Equal(RequestType.Problem, result.Type);
        Assert.Equal(0, result.Confidence.Value);
        Assert.Equal(Urgency.Medium, result.Urgency);
        Assert.Equal(string.Empty, result.Reason);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Ich kann das leider nicht beantworten.")]
    [InlineData("{ kaputt")]
    [InlineData("""{"type":"problem",}""")]
    public void Anything_unreadable_ends_up_with_a_person(string? answer)
    {
        Classification result = ClassificationParser.Parse(answer);

        Assert.Equal(Confidence.None, result.Confidence);
        Assert.Equal(Destination.HumanReview, RoutingPolicy.Decide(result));
        Assert.NotEqual(string.Empty, result.Reason);
    }

    private static Classification Parse(
        string type = "question",
        string confidence = "0.9",
        string urgency = "medium") =>
        ClassificationParser.Parse(
            $$"""{"type":"{{type}}","confidence":{{confidence}},"urgency":"{{urgency}}","reason":"Testfall"}""");
}

using FirstContact.Domain.Knowledge;

namespace FirstContact.Domain.Tests.Knowledge;

/// <summary>
/// An answer must be traceable to the handbook. These tests pin the one rule that
/// makes that true: no source, no answer.
/// </summary>
public class GroundedAnswerTests
{
    [Fact]
    public void An_answer_with_a_source_is_an_answer()
    {
        GroundedAnswer answer = GroundedAnswer.From("Waehlen Sie Export.", ["Dokumente exportieren"]);

        Assert.True(answer.Answered);
        Assert.Equal("Waehlen Sie Export.", answer.Text);
        Assert.Equal(["Dokumente exportieren"], answer.Sources);
    }

    [Fact]
    public void Text_without_a_source_is_not_an_answer()
    {
        Assert.False(GroundedAnswer.From("Vertrauen Sie mir einfach.", []).Answered);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void A_source_without_text_is_not_an_answer(string? text)
    {
        Assert.False(GroundedAnswer.From(text, ["Dokumente exportieren"]).Answered);
    }

    [Fact]
    public void Blank_and_duplicate_sources_are_dropped()
    {
        GroundedAnswer answer = GroundedAnswer.From(
            "Text", ["Export", "  ", "export", "Signatur", null!]);

        Assert.Equal(["Export", "Signatur"], answer.Sources);
    }

    [Fact]
    public void An_invented_source_is_removed_but_a_real_one_survives()
    {
        GroundedAnswer answer = GroundedAnswer
            .From("Waehlen Sie Export.", ["Dokumente exportieren", "Kapitel das es nicht gibt"])
            .KeepOnlyRealSources(["Dokumente exportieren"]);

        Assert.True(answer.Answered);
        Assert.Equal(["Dokumente exportieren"], answer.Sources);
    }

    [Fact]
    public void An_answer_whose_every_source_was_invented_becomes_no_answer()
    {
        GroundedAnswer answer = GroundedAnswer
            .From("Klicken Sie auf den blauen Knopf.", ["Erfundener Abschnitt"])
            .KeepOnlyRealSources(["Dokumente exportieren"]);

        Assert.False(answer.Answered);
        Assert.Equal(string.Empty, answer.Text);
        Assert.Empty(answer.Sources);
    }

    [Fact]
    public void Sources_are_matched_ignoring_case()
    {
        GroundedAnswer answer = GroundedAnswer
            .From("Text", ["DOKUMENTE EXPORTIEREN"])
            .KeepOnlyRealSources(["Dokumente exportieren"]);

        Assert.True(answer.Answered);
    }

    [Fact]
    public void A_missing_answer_stays_missing()
    {
        Assert.False(GroundedAnswer.NotFound.KeepOnlyRealSources(["Dokumente exportieren"]).Answered);
    }
}

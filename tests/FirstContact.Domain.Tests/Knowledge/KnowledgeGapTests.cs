using FirstContact.Domain.Knowledge;

namespace FirstContact.Domain.Tests.Knowledge;

public class KnowledgeGapTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 26, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void A_new_gap_has_been_asked_once()
    {
        KnowledgeGap gap = KnowledgeGap.Detect("  Gibt es eine SAP-Schnittstelle?  ", Now);

        Assert.Equal("Gibt es eine SAP-Schnittstelle?", gap.Question);
        Assert.Equal(1, gap.TimesAsked);
        Assert.Equal(Now, gap.FirstAskedAt);
        Assert.Equal(Now, gap.LastAskedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void A_gap_needs_the_question_that_caused_it(string? question)
    {
        Assert.Throws<ArgumentException>(() => KnowledgeGap.Detect(question!, Now));
    }

    [Fact]
    public void Asking_again_raises_the_count()
    {
        KnowledgeGap gap = KnowledgeGap.Detect("Frage", Now);

        gap.AskedAgain(Now.AddDays(2));

        Assert.Equal(2, gap.TimesAsked);
        Assert.Equal(Now.AddDays(2), gap.LastAskedAt);
        Assert.Equal(Now, gap.FirstAskedAt);
    }

    [Fact]
    public void An_earlier_timestamp_does_not_move_last_asked_backwards()
    {
        KnowledgeGap gap = KnowledgeGap.Detect("Frage", Now);

        gap.AskedAgain(Now.AddDays(-5));

        Assert.Equal(Now, gap.LastAskedAt);
    }
}

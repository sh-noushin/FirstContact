using FirstContact.Application.Knowledge;
using FirstContact.Application.Tests.Fakes;
using FirstContact.Domain.Knowledge;

namespace FirstContact.Application.Tests.Knowledge;

/// <summary>
/// The honesty rules: answer only from the handbook, admit when it cannot, and turn
/// every admission into a documentation task.
/// </summary>
public class AnswerFromHandbookTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 26, 9, 0, 0, TimeSpan.Zero);

    private readonly StubKnowledgeSearch _search = new();
    private readonly StubAnswerWriter _writer = new();
    private readonly InMemoryKnowledgeGapRepository _gaps = new();
    private readonly KnowledgeOptions _options = new();

    [Fact]
    public async Task A_question_the_handbook_covers_is_answered_with_its_source()
    {
        Found("Dokumente exportieren");
        _writer.Answer = GroundedAnswer.From("Waehlen Sie Export.", ["Dokumente exportieren"]);

        GroundedAnswer answer = await Answer("Wie exportiere ich?");

        Assert.True(answer.Answered);
        Assert.Equal("Waehlen Sie Export.", answer.Text);
        Assert.Equal(["Dokumente exportieren"], answer.Sources);
        Assert.Empty(_gaps.All);
    }

    [Fact]
    public async Task Nothing_found_means_no_answer_and_no_call_to_the_model()
    {
        GroundedAnswer answer = await Answer("Gibt es eine SAP-Schnittstelle?");

        Assert.False(answer.Answered);
        Assert.Equal(0, _writer.Calls);
        Assert.Single(_gaps.All);
    }

    [Fact]
    public async Task A_model_that_cannot_answer_creates_a_gap()
    {
        Found("Dokumente exportieren");
        _writer.Answer = GroundedAnswer.NotFound;

        GroundedAnswer answer = await Answer("Etwas ganz anderes");

        Assert.False(answer.Answered);
        Assert.Equal("Etwas ganz anderes", Assert.Single(_gaps.All).Question);
    }

    [Fact]
    public async Task An_invented_source_is_dropped_and_the_real_one_kept()
    {
        Found("Dokumente exportieren");
        _writer.Answer = GroundedAnswer.From(
            "Waehlen Sie Export.", ["Dokumente exportieren", "Kapitel das es nicht gibt"]);

        GroundedAnswer answer = await Answer("Wie exportiere ich?");

        Assert.True(answer.Answered);
        Assert.Equal(["Dokumente exportieren"], answer.Sources);
        Assert.Empty(_gaps.All);
    }

    [Fact]
    public async Task An_answer_whose_sources_were_all_invented_becomes_a_gap()
    {
        // This is the case that matters: the model sounded confident but pointed at
        // nothing real, so the customer must not see it.
        Found("Dokumente exportieren");
        _writer.Answer = GroundedAnswer.From("Klicken Sie auf den blauen Knopf.", ["Erfunden"]);

        GroundedAnswer answer = await Answer("Wie exportiere ich?");

        Assert.False(answer.Answered);
        Assert.Single(_gaps.All);
    }

    [Fact]
    public async Task The_same_unanswered_question_is_counted_on_one_gap()
    {
        await Answer("Gibt es eine SAP-Schnittstelle?");
        await Answer("Gibt es eine SAP-Schnittstelle?");
        await Answer("Gibt es eine SAP-Schnittstelle?");

        Assert.Equal(3, Assert.Single(_gaps.All).TimesAsked);
    }

    [Fact]
    public async Task Different_questions_create_different_gaps()
    {
        await Answer("Gibt es eine SAP-Schnittstelle?");
        await Answer("Was kostet eine Lizenz?");

        Assert.Equal(2, _gaps.All.Count);
    }

    [Fact]
    public async Task The_configured_number_of_articles_is_requested()
    {
        _options.ArticlesPerAnswer = 5;

        await Answer("Eine Frage");

        Assert.Equal(5, _search.RequestedCount);
    }

    private Task<GroundedAnswer> Answer(string question) =>
        new AnswerFromHandbook(_search, _writer, _gaps, new FixedClock(Now), _options)
            .HandleAsync(question, CancellationToken.None);

    private void Found(params string[] titles)
    {
        foreach (string title in titles)
        {
            _search.Found.Add(new ArticleMatch(KnowledgeArticle.Create(title, "Inhalt"), 0.8));
        }
    }
}

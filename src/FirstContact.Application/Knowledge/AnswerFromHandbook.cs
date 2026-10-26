using FirstContact.Application.Abstractions;
using FirstContact.Domain.Knowledge;

namespace FirstContact.Application.Knowledge;

/// <summary>
/// Answers a question using the handbook and nothing else. When no answer can be
/// traced back to an article, the question is recorded as a gap and the customer is
/// told plainly, rather than being given something invented.
/// </summary>
public class AnswerFromHandbook(
    IKnowledgeSearch search,
    IAnswerWriter writer,
    IKnowledgeGapRepository gaps,
    IClock clock,
    KnowledgeOptions options)
{
    public async Task<GroundedAnswer> HandleAsync(string question, CancellationToken cancellationToken)
    {
        IReadOnlyList<ArticleMatch> found =
            await search.FindAsync(question, options.ArticlesPerAnswer, cancellationToken);

        if (found.Count == 0)
        {
            await RecordGapAsync(question, cancellationToken);
            return GroundedAnswer.NotFound;
        }

        GroundedAnswer answer = await writer.WriteAsync(question, found, cancellationToken);

        // Only keep sources the search really returned, so an invented title cannot
        // make a wrong answer look traceable.
        GroundedAnswer verified = answer.KeepOnlyRealSources(found.Select(match => match.Article.Title));

        if (!verified.Answered)
        {
            await RecordGapAsync(question, cancellationToken);
        }

        return verified;
    }

    /// <summary>Counts a repeat on the existing gap instead of adding a second one.</summary>
    private async Task RecordGapAsync(string question, CancellationToken cancellationToken)
    {
        KnowledgeGap? existing = await gaps.FindByQuestionAsync(question, cancellationToken);

        if (existing is null)
        {
            await gaps.AddAsync(KnowledgeGap.Detect(question, clock.UtcNow), cancellationToken);
        }
        else
        {
            existing.AskedAgain(clock.UtcNow);
        }

        await gaps.SaveAsync(cancellationToken);
    }
}

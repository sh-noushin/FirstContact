using FirstContact.Domain.Knowledge;

namespace FirstContact.Application.Abstractions;

/// <summary>
/// Writes an answer using only the articles it is given. Implementations must return
/// <see cref="GroundedAnswer.NotFound"/> rather than inventing anything; the caller
/// still checks the sources afterwards.
/// </summary>
public interface IAnswerWriter
{
    Task<GroundedAnswer> WriteAsync(
        string question,
        IReadOnlyList<ArticleMatch> articles,
        CancellationToken cancellationToken);
}

using FirstContact.Application.Abstractions;
using FirstContact.Domain.Knowledge;

namespace FirstContact.Api.Tests.Fakes;

/// <summary>Stands in for the model that writes handbook answers.</summary>
public class StubAnswerWriter : IAnswerWriter
{
    public GroundedAnswer Answer { get; set; } = GroundedAnswer.NotFound;

    /// <summary>When true, the answer cites the first article the search actually returned.</summary>
    public bool UseFirstFoundArticle { get; set; } = true;

    public Task<GroundedAnswer> WriteAsync(
        string question, IReadOnlyList<ArticleMatch> articles, CancellationToken cancellationToken)
    {
        if (!UseFirstFoundArticle || articles.Count == 0)
        {
            return Task.FromResult(Answer);
        }

        return Task.FromResult(
            GroundedAnswer.From("Das steht so im Handbuch.", [articles[0].Article.Title]));
    }
}

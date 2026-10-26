using FirstContact.Application.Abstractions;
using FirstContact.Domain.Knowledge;

namespace FirstContact.Application.Tests.Fakes;

/// <summary>Returns whatever articles the test decides the search found.</summary>
public class StubKnowledgeSearch : IKnowledgeSearch
{
    public List<ArticleMatch> Found { get; } = [];

    public int? RequestedCount { get; private set; }

    public Task<IReadOnlyList<ArticleMatch>> FindAsync(
        string question, int take, CancellationToken cancellationToken)
    {
        RequestedCount = take;

        return Task.FromResult<IReadOnlyList<ArticleMatch>>(Found.Take(take).ToList());
    }
}

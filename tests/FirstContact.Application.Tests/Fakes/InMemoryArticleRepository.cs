using FirstContact.Application.Abstractions;
using FirstContact.Domain.Knowledge;

namespace FirstContact.Application.Tests.Fakes;

/// <summary>Keeps handbook articles in a list instead of SQLite.</summary>
public class InMemoryArticleRepository : IArticleRepository
{
    private readonly List<KnowledgeArticle> _articles = [];

    public IReadOnlyList<KnowledgeArticle> All => _articles;

    /// <summary>How often an article was re-embedded, which is the expensive part.</summary>
    public int Embeddings { get; private set; }

    public Task<KnowledgeArticle?> FindByTitleAsync(string title, CancellationToken cancellationToken) =>
        Task.FromResult(_articles.FirstOrDefault(article =>
            string.Equals(article.Title, title.Trim(), StringComparison.OrdinalIgnoreCase)));

    public Task AddAsync(KnowledgeArticle article, CancellationToken cancellationToken)
    {
        _articles.Add(article);
        Embeddings++;

        return Task.CompletedTask;
    }

    public Task UpdateAsync(KnowledgeArticle article, CancellationToken cancellationToken)
    {
        Embeddings++;

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<KnowledgeArticle>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<KnowledgeArticle>>(
            _articles.OrderBy(article => article.Title).ToList());

    public Task SaveAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

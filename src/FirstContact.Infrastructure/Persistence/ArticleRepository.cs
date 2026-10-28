using FirstContact.Application.Abstractions;
using FirstContact.Domain.Knowledge;
using FirstContact.Infrastructure.Ai;
using FirstContact.Infrastructure.Knowledge;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FirstContact.Infrastructure.Persistence;

/// <summary>
/// Stores handbook articles with their embeddings. An article is embedded when it is
/// written and whenever its text changes, never on every question.
/// </summary>
public class ArticleRepository(FirstContactDbContext database, IEmbedder embedder) : IArticleRepository
{
    public Task<KnowledgeArticle?> FindByTitleAsync(string title, CancellationToken cancellationToken)
    {
        string wanted = title.Trim();

        return database.Articles
            .FirstOrDefaultAsync(article => article.Title.ToLower() == wanted.ToLower(), cancellationToken);
    }

    public async Task AddAsync(KnowledgeArticle article, CancellationToken cancellationToken)
    {
        EntityEntry<KnowledgeArticle> entry = database.Articles.Add(article);

        await StoreEmbeddingAsync(entry, article, cancellationToken);
    }

    public async Task UpdateAsync(KnowledgeArticle article, CancellationToken cancellationToken)
    {
        EntityEntry<KnowledgeArticle> entry = database.Entry(article);

        await StoreEmbeddingAsync(entry, article, cancellationToken);
    }

    public async Task<IReadOnlyList<KnowledgeArticle>> ListAsync(CancellationToken cancellationToken) =>
        await database.Articles.OrderBy(article => article.Title).ToListAsync(cancellationToken);

    public Task SaveAsync(CancellationToken cancellationToken) =>
        database.SaveChangesAsync(cancellationToken);

    /// <summary>Title and content are embedded together, because the heading carries meaning too.</summary>
    private async Task StoreEmbeddingAsync(
        EntityEntry<KnowledgeArticle> entry, KnowledgeArticle article, CancellationToken cancellationToken)
    {
        float[] vector = await embedder.EmbedAsync(
            article.Title + Environment.NewLine + article.Content, cancellationToken);

        entry.Property<byte[]>(KnowledgeArticleConfiguration.EmbeddingColumn).CurrentValue =
            EmbeddingBytes.From(vector);
    }
}

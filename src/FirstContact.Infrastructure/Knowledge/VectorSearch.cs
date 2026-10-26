using FirstContact.Application.Abstractions;
using FirstContact.Domain.Knowledge;
using FirstContact.Infrastructure.Ai;
using FirstContact.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FirstContact.Infrastructure.Knowledge;

/// <summary>
/// Finds the articles closest in meaning to a question, by comparing embeddings.
/// The handbook is small, so every article is compared in memory. For a large one this
/// is the single class that would be replaced by Azure AI Search.
/// </summary>
public class VectorSearch(FirstContactDbContext database, IEmbedder embedder, KnowledgeSearchOptions options)
    : IKnowledgeSearch
{
    public async Task<IReadOnlyList<ArticleMatch>> FindAsync(
        string question, int take, CancellationToken cancellationToken)
    {
        float[] asked = await embedder.EmbedAsync(question, cancellationToken);

        if (asked.Length == 0)
        {
            return [];
        }

        var stored = await database.Articles
            .Select(article => new
            {
                Article = article,
                Embedding = EF.Property<byte[]>(article, KnowledgeArticleConfiguration.EmbeddingColumn),
            })
            .ToListAsync(cancellationToken);

        return stored
            .Select(row => new ArticleMatch(
                row.Article,
                CosineSimilarity.Between(asked, EmbeddingBytes.To(row.Embedding))))
            .Where(match => match.Score >= options.MinimumScore)
            .OrderByDescending(match => match.Score)
            .Take(take)
            .ToList();
    }
}

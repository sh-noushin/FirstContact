using FirstContact.Domain.Knowledge;

namespace FirstContact.Application.Abstractions;

/// <summary>Stores the handbook. Adding an article also stores the vector used to find it.</summary>
public interface IArticleRepository
{
    Task<KnowledgeArticle?> FindByTitleAsync(string title, CancellationToken cancellationToken);

    Task AddAsync(KnowledgeArticle article, CancellationToken cancellationToken);

    /// <summary>Re-embeds the article, because its text changed.</summary>
    Task UpdateAsync(KnowledgeArticle article, CancellationToken cancellationToken);

    Task<IReadOnlyList<KnowledgeArticle>> ListAsync(CancellationToken cancellationToken);

    Task SaveAsync(CancellationToken cancellationToken);
}

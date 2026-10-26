using FirstContact.Domain.Knowledge;

namespace FirstContact.Application.Abstractions;

/// <summary>
/// Finds the handbook articles closest in meaning to a question.
/// Implemented with embeddings in the Infrastructure layer.
/// </summary>
public interface IKnowledgeSearch
{
    Task<IReadOnlyList<ArticleMatch>> FindAsync(string question, int take, CancellationToken cancellationToken);
}

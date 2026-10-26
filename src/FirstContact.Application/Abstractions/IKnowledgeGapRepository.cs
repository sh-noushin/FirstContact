using FirstContact.Domain.Knowledge;

namespace FirstContact.Application.Abstractions;

/// <summary>Records the questions the handbook could not answer.</summary>
public interface IKnowledgeGapRepository
{
    /// <summary>An existing gap for the same question, so repeats are counted on one entry.</summary>
    Task<KnowledgeGap?> FindByQuestionAsync(string question, CancellationToken cancellationToken);

    Task AddAsync(KnowledgeGap gap, CancellationToken cancellationToken);

    /// <summary>The gaps worth writing an article about, most asked first.</summary>
    Task<IReadOnlyList<KnowledgeGap>> ListAsync(CancellationToken cancellationToken);

    Task SaveAsync(CancellationToken cancellationToken);
}

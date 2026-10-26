using FirstContact.Application.Abstractions;
using FirstContact.Domain.Knowledge;

namespace FirstContact.Application.Tests.Fakes;

/// <summary>Keeps knowledge gaps in a list instead of SQLite.</summary>
public class InMemoryKnowledgeGapRepository : IKnowledgeGapRepository
{
    private readonly List<KnowledgeGap> _gaps = [];

    public IReadOnlyList<KnowledgeGap> All => _gaps;

    public Task<KnowledgeGap?> FindByQuestionAsync(string question, CancellationToken cancellationToken) =>
        Task.FromResult(_gaps.FirstOrDefault(gap =>
            string.Equals(gap.Question, question.Trim(), StringComparison.OrdinalIgnoreCase)));

    public Task AddAsync(KnowledgeGap gap, CancellationToken cancellationToken)
    {
        _gaps.Add(gap);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<KnowledgeGap>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<KnowledgeGap>>(
            _gaps.OrderByDescending(gap => gap.TimesAsked).ToList());

    public Task SaveAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

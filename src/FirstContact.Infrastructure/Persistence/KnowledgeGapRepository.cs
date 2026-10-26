using FirstContact.Application.Abstractions;
using FirstContact.Domain.Knowledge;
using Microsoft.EntityFrameworkCore;

namespace FirstContact.Infrastructure.Persistence;

/// <summary>Stores the questions the handbook could not answer.</summary>
public class KnowledgeGapRepository(FirstContactDbContext database) : IKnowledgeGapRepository
{
    public Task<KnowledgeGap?> FindByQuestionAsync(string question, CancellationToken cancellationToken)
    {
        string wanted = question.Trim();

        return database.KnowledgeGaps
            .FirstOrDefaultAsync(gap => gap.Question.ToLower() == wanted.ToLower(), cancellationToken);
    }

    public async Task AddAsync(KnowledgeGap gap, CancellationToken cancellationToken) =>
        await database.KnowledgeGaps.AddAsync(gap, cancellationToken);

    public async Task<IReadOnlyList<KnowledgeGap>> ListAsync(CancellationToken cancellationToken) =>
        await database.KnowledgeGaps
            .OrderByDescending(gap => gap.TimesAsked)
            .ThenByDescending(gap => gap.LastAskedAt)
            .ToListAsync(cancellationToken);

    public Task SaveAsync(CancellationToken cancellationToken) =>
        database.SaveChangesAsync(cancellationToken);
}

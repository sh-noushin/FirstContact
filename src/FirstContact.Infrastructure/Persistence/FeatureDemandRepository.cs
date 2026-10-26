using FirstContact.Application.Abstractions;
using FirstContact.Domain.Features;
using FirstContact.Infrastructure.Ai;
using FirstContact.Infrastructure.Knowledge;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FirstContact.Infrastructure.Persistence;

/// <summary>Stores groups of feature wishes, together with the vector used to match them.</summary>
public class FeatureDemandRepository(FirstContactDbContext database, IEmbedder embedder)
    : IFeatureDemandRepository
{
    public Task<FeatureDemand?> FindAsync(int id, CancellationToken cancellationToken) =>
        database.FeatureDemands.FirstOrDefaultAsync(demand => demand.Id == id, cancellationToken);

    public async Task AddAsync(FeatureDemand demand, string matchText, CancellationToken cancellationToken)
    {
        float[] vector = await embedder.EmbedAsync(matchText, cancellationToken);

        EntityEntry<FeatureDemand> entry = database.FeatureDemands.Add(demand);
        entry.Property<byte[]>(FeatureDemandConfiguration.EmbeddingColumn).CurrentValue =
            EmbeddingBytes.From(vector);
    }

    public async Task<IReadOnlyList<FeatureDemand>> ListAsync(CancellationToken cancellationToken)
    {
        List<FeatureDemand> demands = await database.FeatureDemands.ToListAsync(cancellationToken);

        // Sorted here rather than in SQL, because the count comes from the loaded
        // wishes rather than from a column.
        return demands
            .OrderByDescending(demand => demand.CustomerCount)
            .ThenByDescending(demand => demand.LastSeen)
            .ToList();
    }

    public Task SaveAsync(CancellationToken cancellationToken) =>
        database.SaveChangesAsync(cancellationToken);
}

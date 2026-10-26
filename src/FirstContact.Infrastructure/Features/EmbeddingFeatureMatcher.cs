using FirstContact.Application.Abstractions;
using FirstContact.Infrastructure.Ai;
using FirstContact.Infrastructure.Knowledge;
using FirstContact.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FirstContact.Infrastructure.Features;

/// <summary>
/// Decides whether a new wish means the same as an existing group, by comparing embeddings.
/// No model call is needed: this is arithmetic on vectors we already have.
/// </summary>
public class EmbeddingFeatureMatcher(
    FirstContactDbContext database,
    IEmbedder embedder,
    FeatureMatchOptions options) : IFeatureMatcher
{
    public async Task<int?> FindSimilarAsync(string wish, CancellationToken cancellationToken)
    {
        float[] asked = await embedder.EmbedAsync(wish, cancellationToken);

        if (asked.Length == 0)
        {
            return null;
        }

        var stored = await database.FeatureDemands
            .Select(demand => new
            {
                demand.Id,
                Embedding = EF.Property<byte[]>(demand, FeatureDemandConfiguration.EmbeddingColumn),
            })
            .ToListAsync(cancellationToken);

        var closest = stored
            .Select(row => new
            {
                row.Id,
                Score = CosineSimilarity.Between(asked, EmbeddingBytes.To(row.Embedding)),
            })
            .OrderByDescending(row => row.Score)
            .FirstOrDefault();

        return closest is not null && closest.Score >= options.MinimumScore ? closest.Id : null;
    }
}

using FirstContact.Domain.Features;

namespace FirstContact.Application.Abstractions;

/// <summary>Stores the groups of similar feature wishes.</summary>
public interface IFeatureDemandRepository
{
    Task<FeatureDemand?> FindAsync(int id, CancellationToken cancellationToken);

    /// <summary>Adds a new group, together with the text it should be matched against later.</summary>
    Task AddAsync(FeatureDemand demand, string matchText, CancellationToken cancellationToken);

    /// <summary>The groups with the most customers behind them first.</summary>
    Task<IReadOnlyList<FeatureDemand>> ListAsync(CancellationToken cancellationToken);

    Task SaveAsync(CancellationToken cancellationToken);
}

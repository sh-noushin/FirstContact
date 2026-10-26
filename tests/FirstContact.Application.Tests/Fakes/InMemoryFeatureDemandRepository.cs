using FirstContact.Application.Abstractions;
using FirstContact.Domain.Features;

namespace FirstContact.Application.Tests.Fakes;

/// <summary>Keeps feature demands in a list instead of SQLite.</summary>
public class InMemoryFeatureDemandRepository : IFeatureDemandRepository
{
    private readonly List<FeatureDemand> _demands = [];
    private int _nextId = 1;

    public IReadOnlyList<FeatureDemand> All => _demands;

    /// <summary>The text each group was stored with, which the matcher would compare against.</summary>
    public List<string> MatchTexts { get; } = [];

    public Task<FeatureDemand?> FindAsync(int id, CancellationToken cancellationToken) =>
        Task.FromResult(_demands.ElementAtOrDefault(id - 1));

    public Task AddAsync(FeatureDemand demand, string matchText, CancellationToken cancellationToken)
    {
        // The database would assign the id; here the position in the list stands in for one.
        _nextId++;
        _demands.Add(demand);
        MatchTexts.Add(matchText);

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<FeatureDemand>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<FeatureDemand>>(
            _demands.OrderByDescending(demand => demand.CustomerCount).ToList());

    public Task SaveAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

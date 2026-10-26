using FirstContact.Domain.Features;

namespace FirstContact.Api.Contracts;

/// <summary>A grouped feature wish, with the customers' own words as evidence.</summary>
public record DemandView(
    int Id,
    string Title,
    int CustomerCount,
    IReadOnlyList<string> Quotes,
    DateTimeOffset LastSeen)
{
    /// <summary>How many quotes are worth showing before the list stops being readable.</summary>
    private const int MaxQuotes = 5;

    public static DemandView Of(FeatureDemand demand) => new(
        demand.Id,
        demand.Title,
        demand.CustomerCount,
        demand.Wishes.OrderByDescending(wish => wish.At).Take(MaxQuotes).Select(wish => wish.Quote).ToList(),
        demand.LastSeen);
}

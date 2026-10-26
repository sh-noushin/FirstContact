namespace FirstContact.Domain.Features;

/// <summary>
/// Several customers asking for the same thing, counted as one. Product management needs
/// demand per idea, not a pile of separate messages, so the count is the point of this class.
/// </summary>
public class FeatureDemand
{
    private readonly List<FeatureWish> _wishes = [];

    /// <summary>EF Core needs a parameterless constructor.</summary>
    private FeatureDemand()
    {
        Title = string.Empty;
    }

    private FeatureDemand(string title, DateTimeOffset at)
    {
        Title = title;
        FirstSeen = at;
        LastSeen = at;
    }

    public int Id { get; private set; }

    /// <summary>What the group is about, in the words of the customer who asked first.</summary>
    public string Title { get; private set; }

    public DateTimeOffset FirstSeen { get; private set; }

    public DateTimeOffset LastSeen { get; private set; }

    public IReadOnlyList<FeatureWish> Wishes => _wishes;

    /// <summary>How many separate customer requests are behind this idea.</summary>
    public int CustomerCount => _wishes.Count;

    public static FeatureDemand Start(string title, int requestId, string quote, DateTimeOffset at)
    {
        string cleanTitle = title?.Trim() ?? string.Empty;

        if (cleanTitle.Length == 0)
        {
            throw new ArgumentException("A demand needs a title.", nameof(title));
        }

        FeatureDemand demand = new(cleanTitle, at);
        demand.Record(requestId, quote, at);

        return demand;
    }

    /// <summary>
    /// Adds a wish to this group. The same request is never counted twice, so a retried
    /// or replayed message cannot inflate demand.
    /// </summary>
    public void Record(int requestId, string quote, DateTimeOffset at)
    {
        if (_wishes.Any(wish => wish.RequestId == requestId))
        {
            return;
        }

        string cleanQuote = quote?.Trim() ?? string.Empty;

        if (cleanQuote.Length == 0)
        {
            throw new ArgumentException("A wish needs the customer's words.", nameof(quote));
        }

        _wishes.Add(new FeatureWish(requestId, cleanQuote, at));
        LastSeen = at > LastSeen ? at : LastSeen;
    }

    /// <summary>Lets staff give the group a clearer name once it covers more than its first wish.</summary>
    public void Rename(string title)
    {
        string cleaned = title?.Trim() ?? string.Empty;

        if (cleaned.Length == 0)
        {
            throw new ArgumentException("A demand needs a title.", nameof(title));
        }

        Title = cleaned;
    }
}

namespace FirstContact.Domain.Features;

/// <summary>
/// One customer asking for something. It belongs to a <see cref="FeatureDemand"/> and is
/// only ever created through it, so the count and the quotes cannot drift apart.
/// </summary>
public class FeatureWish
{
    /// <summary>EF Core needs a parameterless constructor.</summary>
    private FeatureWish()
    {
        Quote = string.Empty;
    }

    internal FeatureWish(int requestId, string quote, DateTimeOffset at)
    {
        RequestId = requestId;
        Quote = quote;
        At = at;
    }

    public int Id { get; private set; }

    /// <summary>Which request this came from. Used to make sure nobody is counted twice.</summary>
    public int RequestId { get; private set; }

    /// <summary>The customer's own words, kept as evidence for the product discussion.</summary>
    public string Quote { get; private set; }

    public DateTimeOffset At { get; private set; }
}

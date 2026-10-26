using FirstContact.Application.Abstractions;
using FirstContact.Domain.Features;

namespace FirstContact.Application.Features;

/// <summary>
/// Puts a feature wish with the others that mean the same thing. Product management then
/// sees "three customers want this" instead of three separate messages.
/// </summary>
public class GroupFeatureWish(
    IFeatureMatcher matcher,
    IFeatureDemandRepository demands,
    IClock clock)
{
    /// <summary>A new group is named after the customer's own words, shortened.</summary>
    private const int TitleLength = 60;

    public async Task<FeatureDemand> HandleAsync(int requestId, string wish, CancellationToken cancellationToken)
    {
        int? similarId = await matcher.FindSimilarAsync(wish, cancellationToken);

        FeatureDemand? existing = similarId.HasValue
            ? await demands.FindAsync(similarId.Value, cancellationToken)
            : null;

        if (existing is not null)
        {
            existing.Record(requestId, wish, clock.UtcNow);
            await demands.SaveAsync(cancellationToken);

            return existing;
        }

        FeatureDemand started = FeatureDemand.Start(Shorten(wish), requestId, wish, clock.UtcNow);

        await demands.AddAsync(started, wish, cancellationToken);
        await demands.SaveAsync(cancellationToken);

        return started;
    }

    private static string Shorten(string wish)
    {
        string cleaned = wish.Trim();

        if (cleaned.Length <= TitleLength)
        {
            return cleaned;
        }

        string cut = cleaned[..TitleLength];
        int lastSpace = cut.LastIndexOf(' ');

        return (lastSpace > TitleLength / 2 ? cut[..lastSpace] : cut).TrimEnd() + " ...";
    }
}

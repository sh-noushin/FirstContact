using FirstContact.Application.Abstractions;

namespace FirstContact.Application.Tests.Fakes;

/// <summary>Decides, on the test's behalf, whether a wish matches an existing group.</summary>
public class StubFeatureMatcher : IFeatureMatcher
{
    public int? Match { get; set; }

    public string? SeenWish { get; private set; }

    public Task<int?> FindSimilarAsync(string wish, CancellationToken cancellationToken)
    {
        SeenWish = wish;

        return Task.FromResult(Match);
    }
}

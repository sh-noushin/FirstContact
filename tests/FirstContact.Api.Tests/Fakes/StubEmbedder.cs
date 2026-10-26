using FirstContact.Infrastructure.Ai;

namespace FirstContact.Api.Tests.Fakes;

/// <summary>
/// A tiny deterministic stand-in for text-embedding-3-small: texts that share words
/// end up pointing in a similar direction, which is enough to test the search.
/// </summary>
public class StubEmbedder : IEmbedder
{
    private const int Dimensions = 32;

    public Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken)
    {
        float[] vector = new float[Dimensions];

        foreach (string word in text.ToLowerInvariant().Split([' ', '\n', '\r', '.', ',', '?', '!', '-'],
                     StringSplitOptions.RemoveEmptyEntries))
        {
            vector[Math.Abs(word.GetHashCode(StringComparison.Ordinal)) % Dimensions] += 1;
        }

        return Task.FromResult(vector);
    }
}

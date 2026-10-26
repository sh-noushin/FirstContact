namespace FirstContact.Domain.Knowledge;

/// <summary>
/// An answer that rests on named handbook articles, or no answer at all.
/// There is no third option on purpose: the bot hands over to a person rather
/// than answering from something it cannot point at.
/// </summary>
public sealed record GroundedAnswer
{
    private GroundedAnswer(bool answered, string text, IReadOnlyList<string> sources)
    {
        Answered = answered;
        Text = text;
        Sources = sources;
    }

    /// <summary>The handbook did not cover the question.</summary>
    public static GroundedAnswer NotFound { get; } = new(false, string.Empty, []);

    public bool Answered { get; }

    public string Text { get; }

    /// <summary>Titles of the articles the answer came from.</summary>
    public IReadOnlyList<string> Sources { get; }

    /// <summary>
    /// Builds an answer. Text without a source counts as no answer, because an answer
    /// nobody can trace back to the handbook is exactly what this design avoids.
    /// </summary>
    public static GroundedAnswer From(string? text, IEnumerable<string> sources)
    {
        string cleaned = text?.Trim() ?? string.Empty;
        List<string> titles = sources
            .Select(source => source?.Trim() ?? string.Empty)
            .Where(source => source.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return cleaned.Length == 0 || titles.Count == 0
            ? NotFound
            : new GroundedAnswer(true, cleaned, titles);
    }

    /// <summary>
    /// Drops any source the search did not actually return. A model that invents a
    /// plausible title would otherwise produce an answer that only looks sourced.
    /// </summary>
    public GroundedAnswer KeepOnlyRealSources(IEnumerable<string> foundTitles)
    {
        if (!Answered)
        {
            return NotFound;
        }

        HashSet<string> real = foundTitles.ToHashSet(StringComparer.OrdinalIgnoreCase);

        return From(Text, Sources.Where(real.Contains));
    }
}

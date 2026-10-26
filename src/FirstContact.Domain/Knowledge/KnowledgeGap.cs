namespace FirstContact.Domain.Knowledge;

/// <summary>
/// A question the handbook could not answer, recorded instead of guessed at.
/// Repeats are counted on one gap, so the most-asked missing article rises to the top.
/// </summary>
public class KnowledgeGap
{
    /// <summary>EF Core needs a parameterless constructor.</summary>
    private KnowledgeGap()
    {
        Question = string.Empty;
    }

    private KnowledgeGap(string question, DateTimeOffset at)
    {
        Question = question;
        FirstAskedAt = at;
        LastAskedAt = at;
        TimesAsked = 1;
    }

    public int Id { get; private set; }

    public string Question { get; private set; }

    /// <summary>How often customers have run into this gap.</summary>
    public int TimesAsked { get; private set; }

    public DateTimeOffset FirstAskedAt { get; private set; }

    public DateTimeOffset LastAskedAt { get; private set; }

    public static KnowledgeGap Detect(string question, DateTimeOffset at)
    {
        string cleaned = question?.Trim() ?? string.Empty;

        if (cleaned.Length == 0)
        {
            throw new ArgumentException("A gap needs the question that caused it.", nameof(question));
        }

        return new KnowledgeGap(cleaned, at);
    }

    /// <summary>Records that someone ran into the same gap again.</summary>
    public void AskedAgain(DateTimeOffset at)
    {
        TimesAsked++;
        LastAskedAt = at > LastAskedAt ? at : LastAskedAt;
    }
}

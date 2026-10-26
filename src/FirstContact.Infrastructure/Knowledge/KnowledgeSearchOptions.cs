namespace FirstContact.Infrastructure.Knowledge;

/// <summary>Settings for the handbook search.</summary>
public class KnowledgeSearchOptions
{
    public const string SectionName = "KnowledgeSearch";

    /// <summary>
    /// How close an article must be before it counts as relevant. Below this the question
    /// is treated as not covered, which is better than answering from something unrelated.
    /// </summary>
    public double MinimumScore { get; set; } = 0.30;
}

namespace FirstContact.Application.Knowledge;

/// <summary>Settings for answering from the handbook.</summary>
public class KnowledgeOptions
{
    public const string SectionName = "Knowledge";

    /// <summary>How many articles are given to the model as context.</summary>
    public int ArticlesPerAnswer { get; set; } = 3;
}

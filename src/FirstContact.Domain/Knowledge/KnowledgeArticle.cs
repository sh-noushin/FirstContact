namespace FirstContact.Domain.Knowledge;

/// <summary>One section of the handbook. The only thing the bot is allowed to answer from.</summary>
public class KnowledgeArticle
{
    /// <summary>EF Core needs a parameterless constructor.</summary>
    private KnowledgeArticle()
    {
        Title = string.Empty;
        Content = string.Empty;
    }

    private KnowledgeArticle(string title, string content)
    {
        Title = title;
        Content = content;
    }

    public int Id { get; private set; }

    /// <summary>Shown to the customer as the source of an answer, so it must be meaningful.</summary>
    public string Title { get; private set; }

    public string Content { get; private set; }

    public static KnowledgeArticle Create(string title, string content)
    {
        string cleanTitle = title?.Trim() ?? string.Empty;
        string cleanContent = content?.Trim() ?? string.Empty;

        if (cleanTitle.Length == 0)
        {
            throw new ArgumentException("An article needs a title.", nameof(title));
        }

        if (cleanContent.Length == 0)
        {
            throw new ArgumentException("An article needs content.", nameof(content));
        }

        return new KnowledgeArticle(cleanTitle, cleanContent);
    }

    /// <summary>Replaces the text, for example when staff improve an article.</summary>
    public void Update(string content)
    {
        string cleaned = content?.Trim() ?? string.Empty;

        if (cleaned.Length == 0)
        {
            throw new ArgumentException("An article needs content.", nameof(content));
        }

        Content = cleaned;
    }
}

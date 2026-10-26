using FirstContact.Domain.Knowledge;
using FirstContact.Infrastructure.Ai;
using FirstContact.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FirstContact.Infrastructure.Knowledge;

/// <summary>
/// Loads the handbook from Markdown files on first start, so the app works straight
/// after cloning. Each "## " heading becomes one article, and each article is embedded
/// once and stored, rather than on every restart.
/// </summary>
public class HandbookSeeder(FirstContactDbContext database, IEmbedder embedder)
{
    public async Task SeedAsync(string folder, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(folder) || await database.Articles.AnyAsync(cancellationToken))
        {
            return;
        }

        foreach (string file in Directory.GetFiles(folder, "*.md").OrderBy(name => name))
        {
            foreach (KnowledgeArticle article in ReadSections(await File.ReadAllTextAsync(file, cancellationToken)))
            {
                await AddAsync(article, cancellationToken);
            }
        }

        await database.SaveChangesAsync(cancellationToken);
    }

    private async Task AddAsync(KnowledgeArticle article, CancellationToken cancellationToken)
    {
        float[] vector = await embedder.EmbedAsync(article.Title + "\n" + article.Content, cancellationToken);

        EntityEntry<KnowledgeArticle> entry = database.Articles.Add(article);
        entry.Property<byte[]>(KnowledgeArticleConfiguration.EmbeddingColumn).CurrentValue =
            EmbeddingBytes.From(vector);
    }

    /// <summary>Splits a Markdown file at every "## " heading.</summary>
    public static IReadOnlyList<KnowledgeArticle> ReadSections(string markdown)
    {
        var articles = new List<KnowledgeArticle>();
        string? title = null;
        var body = new List<string>();

        void Finish()
        {
            string content = string.Join('\n', body).Trim();

            if (title is not null && content.Length > 0)
            {
                articles.Add(KnowledgeArticle.Create(title, content));
            }

            body.Clear();
        }

        foreach (string line in markdown.Split('\n').Select(line => line.TrimEnd('\r')))
        {
            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                Finish();
                title = line[3..].Trim();
            }
            else if (!line.StartsWith("# ", StringComparison.Ordinal))
            {
                body.Add(line);
            }
        }

        Finish();

        return articles;
    }
}

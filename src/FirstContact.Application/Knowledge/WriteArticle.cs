using FirstContact.Application.Abstractions;
using FirstContact.Domain.Knowledge;

namespace FirstContact.Application.Knowledge;

/// <summary>
/// Staff add or improve a handbook article. This is what closes the loop: a question
/// the bot could not answer becomes an article, and the next customer gets an answer.
/// </summary>
public class WriteArticle(IArticleRepository articles)
{
    public async Task<KnowledgeArticle> HandleAsync(
        string title, string content, CancellationToken cancellationToken)
    {
        KnowledgeArticle? existing = await articles.FindByTitleAsync(title, cancellationToken);

        if (existing is not null)
        {
            // Same title means the same article, so this is an improvement rather than
            // a second copy competing with the first in search results.
            existing.Update(content);
            await articles.UpdateAsync(existing, cancellationToken);
            await articles.SaveAsync(cancellationToken);

            return existing;
        }

        KnowledgeArticle article = KnowledgeArticle.Create(title, content);

        await articles.AddAsync(article, cancellationToken);
        await articles.SaveAsync(cancellationToken);

        return article;
    }
}

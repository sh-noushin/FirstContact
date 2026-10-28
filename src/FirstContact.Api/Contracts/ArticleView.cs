using FirstContact.Domain.Knowledge;

namespace FirstContact.Api.Contracts;

/// <summary>One handbook article.</summary>
public record ArticleView(int Id, string Title, string Content)
{
    public static ArticleView Of(KnowledgeArticle article) =>
        new(article.Id, article.Title, article.Content);
}

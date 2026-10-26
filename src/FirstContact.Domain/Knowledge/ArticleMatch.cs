namespace FirstContact.Domain.Knowledge;

/// <summary>
/// An article the search considered relevant, with how close it was.
/// The score is kept so staff can see how strong the match was.
/// </summary>
public record ArticleMatch(KnowledgeArticle Article, double Score);

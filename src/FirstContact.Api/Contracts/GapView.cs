using FirstContact.Domain.Knowledge;

namespace FirstContact.Api.Contracts;

/// <summary>A question the handbook could not answer: a candidate for a new article.</summary>
public record GapView(int Id, string Question, int TimesAsked, DateTimeOffset LastAskedAt)
{
    public static GapView Of(KnowledgeGap gap) =>
        new(gap.Id, gap.Question, gap.TimesAsked, gap.LastAskedAt);
}

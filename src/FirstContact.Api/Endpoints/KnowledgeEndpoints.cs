using FirstContact.Api.Contracts;
using FirstContact.Application.Abstractions;
using FirstContact.Application.Knowledge;
using FirstContact.Domain.Knowledge;

namespace FirstContact.Api.Endpoints;

/// <summary>Everything staff need for the knowledge base.</summary>
public static class KnowledgeEndpoints
{
    public static void MapKnowledge(this WebApplication app)
    {
        app.MapGet("/api/knowledge/gaps", async (
            IKnowledgeGapRepository gaps,
            CancellationToken cancellationToken) =>
        {
            var found = await gaps.ListAsync(cancellationToken);

            return Results.Ok(found.Select(GapView.Of));
        });

        app.MapGet("/api/knowledge/articles", async (
            IArticleRepository articles,
            CancellationToken cancellationToken) =>
        {
            var found = await articles.ListAsync(cancellationToken);

            return Results.Ok(found.Select(ArticleView.Of));
        });

        app.MapPost("/api/knowledge/articles", async (
            NewArticle body,
            WriteArticle write,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(body.Title) || string.IsNullOrWhiteSpace(body.Content))
            {
                return Results.BadRequest(new { error = "Titel und Inhalt sind erforderlich." });
            }

            KnowledgeArticle article = await write.HandleAsync(body.Title, body.Content, cancellationToken);

            return Results.Ok(ArticleView.Of(article));
        });
    }
}

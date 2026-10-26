using FirstContact.Api.Contracts;
using FirstContact.Application.Abstractions;

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
    }
}

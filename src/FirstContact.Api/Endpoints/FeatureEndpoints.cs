using FirstContact.Api.Contracts;
using FirstContact.Application.Abstractions;

namespace FirstContact.Api.Endpoints;

/// <summary>What customers are asking for, grouped by idea.</summary>
public static class FeatureEndpoints
{
    public static void MapFeatures(this WebApplication app)
    {
        app.MapGet("/api/features", async (
            IFeatureDemandRepository demands,
            CancellationToken cancellationToken) =>
        {
            var found = await demands.ListAsync(cancellationToken);

            return Results.Ok(found.Select(DemandView.Of));
        });
    }
}

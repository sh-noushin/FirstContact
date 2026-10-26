using FirstContact.Application.Dashboard;

namespace FirstContact.Api.Endpoints;

/// <summary>The numbers behind the automation.</summary>
public static class DashboardEndpoints
{
    public static void MapDashboard(this WebApplication app)
    {
        app.MapGet("/api/dashboard", async (GetDashboard dashboard, CancellationToken cancellationToken) =>
            Results.Ok(await dashboard.HandleAsync(cancellationToken)));
    }
}

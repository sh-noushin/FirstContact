using FirstContact.Application.Abstractions;

namespace FirstContact.Application.Dashboard;

/// <summary>Serves the dashboard numbers.</summary>
public class GetDashboard(IDashboardQueries queries)
{
    public Task<DashboardView> HandleAsync(CancellationToken cancellationToken) =>
        queries.GetAsync(cancellationToken);
}

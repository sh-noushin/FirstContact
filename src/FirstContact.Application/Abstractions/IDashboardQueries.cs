using FirstContact.Application.Dashboard;

namespace FirstContact.Application.Abstractions;

/// <summary>
/// The read side. It returns the finished numbers rather than loading entities,
/// because reporting has no rules to protect and a very different access pattern.
/// </summary>
public interface IDashboardQueries
{
    Task<DashboardView> GetAsync(CancellationToken cancellationToken);
}

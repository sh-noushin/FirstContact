using FirstContact.Application.Abstractions;
using FirstContact.Application.Dashboard;
using FirstContact.Domain.Requests;
using FirstContact.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FirstContact.Infrastructure.Dashboard;

/// <summary>Counts the requests in SQLite and turns them into the dashboard numbers.</summary>
public class DashboardQueries(FirstContactDbContext database) : IDashboardQueries
{
    public async Task<DashboardView> GetAsync(CancellationToken cancellationToken)
    {
        // One read, then count in memory: the figures all come from the same snapshot,
        // so they cannot contradict each other.
        var requests = await database.Requests
            .Select(request => new
            {
                request.Destination,
                request.AnsweredByBot,
                Type = request.Classification != null ? request.Classification.Type : (RequestType?)null,
                Confidence = request.Classification != null ? request.Classification.Confidence.Value : (double?)null,
            })
            .ToListAsync(cancellationToken);

        int total = requests.Count;
        int answered = requests.Count(request => request.AnsweredByBot);
        List<double> confidences = requests.Where(r => r.Confidence.HasValue).Select(r => r.Confidence!.Value).ToList();

        return new DashboardView(
            total,
            answered,
            total == 0 ? 0 : Math.Round((double)answered / total, 3),
            requests.Count(request => request.Destination == Destination.SupportTeam),
            requests.Count(request => request.Destination == Destination.ProductTeam),
            requests.Count(request => request.Destination == Destination.HumanReview),
            confidences.Count == 0 ? 0 : Math.Round(confidences.Average(), 3),
            await database.KnowledgeGaps.CountAsync(cancellationToken),
            CountByType(requests.Select(request => request.Type)));
    }

    private static Dictionary<string, int> CountByType(IEnumerable<RequestType?> types) =>
        types.Where(type => type.HasValue)
            .GroupBy(type => type!.Value.ToString())
            .ToDictionary(group => group.Key, group => group.Count());
}

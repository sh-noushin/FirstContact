using FirstContact.Application.Dashboard;
using FirstContact.Domain.Knowledge;
using FirstContact.Domain.Requests;
using FirstContact.Infrastructure.Dashboard;
using FirstContact.Infrastructure.Persistence;
using FirstContact.Infrastructure.Tests.Persistence;

namespace FirstContact.Infrastructure.Tests.Dashboard;

/// <summary>Counts against a real SQLite database, so the queries are proven, not mocked.</summary>
public class DashboardQueriesTests : IDisposable
{
    private static readonly DateTimeOffset Now = new(2026, 10, 26, 9, 0, 0, TimeSpan.Zero);

    private readonly SqliteTestDatabase _database = new();

    [Fact]
    public async Task An_empty_database_reports_zeroes_rather_than_dividing_by_zero()
    {
        DashboardView view = await GetAsync();

        Assert.Equal(0, view.TotalRequests);
        Assert.Equal(0, view.SelfServiceRate);
        Assert.Equal(0, view.AverageConfidence);
        Assert.Empty(view.ByType);
    }

    [Fact]
    public async Task The_self_service_rate_counts_only_questions_the_bot_really_answered()
    {
        await StoreAsync(
            SelfService(answered: true),
            SelfService(answered: true),
            SelfService(answered: false),   // reached the bot, but the handbook had nothing
            Routed(RequestType.Problem, Destination.SupportTeam));

        DashboardView view = await GetAsync();

        Assert.Equal(4, view.TotalRequests);
        Assert.Equal(2, view.AnsweredByBot);
        Assert.Equal(0.5, view.SelfServiceRate);
    }

    [Fact]
    public async Task Each_destination_is_counted()
    {
        await StoreAsync(
            Routed(RequestType.Problem, Destination.SupportTeam),
            Routed(RequestType.Other, Destination.SupportTeam),
            Routed(RequestType.FeatureRequest, Destination.ProductTeam),
            AwaitingReview());

        DashboardView view = await GetAsync();

        Assert.Equal(2, view.ToSupport);
        Assert.Equal(1, view.ToProduct);
        Assert.Equal(1, view.AwaitingReview);
    }

    [Fact]
    public async Task The_average_confidence_is_reported()
    {
        await StoreAsync(
            Routed(RequestType.Problem, Destination.SupportTeam, 0.9),
            Routed(RequestType.Problem, Destination.SupportTeam, 0.7));

        Assert.Equal(0.8, (await GetAsync()).AverageConfidence, 3);
    }

    [Fact]
    public async Task Requests_are_grouped_by_type()
    {
        await StoreAsync(
            Routed(RequestType.Problem, Destination.SupportTeam),
            Routed(RequestType.Problem, Destination.SupportTeam),
            Routed(RequestType.FeatureRequest, Destination.ProductTeam));

        DashboardView view = await GetAsync();

        Assert.Equal(2, view.ByType["Problem"]);
        Assert.Equal(1, view.ByType["FeatureRequest"]);
    }

    [Fact]
    public async Task Open_knowledge_gaps_are_counted()
    {
        await using FirstContactDbContext writing = _database.NewContext();
        writing.KnowledgeGaps.Add(KnowledgeGap.Detect("Gibt es eine SAP-Schnittstelle?", Now));
        writing.KnowledgeGaps.Add(KnowledgeGap.Detect("Was kostet eine Lizenz?", Now));
        await writing.SaveChangesAsync();

        Assert.Equal(2, (await GetAsync()).OpenKnowledgeGaps);
    }

    public void Dispose() => _database.Dispose();

    private async Task<DashboardView> GetAsync()
    {
        await using FirstContactDbContext reading = _database.NewContext();

        return await new DashboardQueries(reading).GetAsync(CancellationToken.None);
    }

    private async Task StoreAsync(params CustomerRequest[] requests)
    {
        await using FirstContactDbContext writing = _database.NewContext();
        writing.Requests.AddRange(requests);

        await writing.SaveChangesAsync();
    }

    private static CustomerRequest SelfService(bool answered)
    {
        CustomerRequest request = Routed(RequestType.Question, Destination.SelfService);
        request.RecordSelfServiceAnswer(answered);

        return request;
    }

    private static CustomerRequest Routed(RequestType type, Destination destination, double confidence = 0.9)
    {
        CustomerRequest request = CustomerRequest.Receive("Eine Nachricht", Now);
        request.Classify(Classification.Create(
            type, Confidence.Create(confidence), Urgency.Medium, "Testfall"));
        request.RouteTo(destination);

        return request;
    }

    private static CustomerRequest AwaitingReview()
    {
        CustomerRequest request = CustomerRequest.Receive("Unklare Nachricht", Now);
        request.Classify(Classification.Create(
            RequestType.Other, Confidence.Create(0.3), Urgency.Medium, "Testfall"));
        request.SendToHumanReview();

        return request;
    }
}

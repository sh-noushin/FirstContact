namespace FirstContact.Application.Dashboard;

/// <summary>
/// The numbers staff use to judge whether the automation is working.
/// A read model: it is built straight from the database rather than from the entities.
/// </summary>
public record DashboardView(
    int TotalRequests,
    int AnsweredByBot,
    double SelfServiceRate,
    int ToSupport,
    int ToProduct,
    int AwaitingReview,
    double AverageConfidence,
    int OpenKnowledgeGaps,
    IReadOnlyDictionary<string, int> ByType);

using FirstContact.Application.Abstractions;
using FirstContact.Domain.Requests;

namespace FirstContact.Api.Tests.Fakes;

/// <summary>Stands in for gpt-5-mini, so the API tests never call Azure.</summary>
public class StubClassifier : IClassifier
{
    public Classification Answer { get; set; } =
        Classification.Create(RequestType.Question, Confidence.Create(0.95), Urgency.Medium, "Testfall");

    public Task<Classification> ClassifyAsync(string message, CancellationToken cancellationToken) =>
        Task.FromResult(Answer);
}

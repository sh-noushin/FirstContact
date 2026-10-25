using FirstContact.Application.Abstractions;
using FirstContact.Domain.Requests;

namespace FirstContact.Application.Tests.Fakes;

/// <summary>Stands in for gpt-5-mini and answers whatever the test decides.</summary>
public class FakeClassifier : IClassifier
{
    public Classification Answer { get; set; } =
        Classification.Create(RequestType.Question, Confidence.Create(0.95), Urgency.Medium, "Testfall");

    /// <summary>The message the pipeline passed in, so tests can check it was trimmed.</summary>
    public string? SeenMessage { get; private set; }

    public int Calls { get; private set; }

    public Task<Classification> ClassifyAsync(string message, CancellationToken cancellationToken)
    {
        Calls++;
        SeenMessage = message;

        return Task.FromResult(Answer);
    }
}

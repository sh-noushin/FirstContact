using FirstContact.Application.Abstractions;
using FirstContact.Domain.Knowledge;

namespace FirstContact.Application.Tests.Fakes;

/// <summary>Stands in for the model that writes the answer.</summary>
public class StubAnswerWriter : IAnswerWriter
{
    public GroundedAnswer Answer { get; set; } = GroundedAnswer.NotFound;

    public int Calls { get; private set; }

    public Task<GroundedAnswer> WriteAsync(
        string question, IReadOnlyList<ArticleMatch> articles, CancellationToken cancellationToken)
    {
        Calls++;

        return Task.FromResult(Answer);
    }
}

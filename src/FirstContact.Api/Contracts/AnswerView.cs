using FirstContact.Domain.Knowledge;

namespace FirstContact.Api.Contracts;

/// <summary>The answer the bot found in the handbook, with the articles it used.</summary>
public record AnswerView(bool Answered, string Text, IReadOnlyList<string> Sources)
{
    public static AnswerView Of(GroundedAnswer answer) =>
        new(answer.Answered, answer.Text, answer.Sources);
}

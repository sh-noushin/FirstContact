using FirstContact.Application.Abstractions;

namespace FirstContact.Application.Tests.Fakes;

/// <summary>
/// Stands in for the Language service. By default it hands the text back unchanged,
/// so a test only has to care about redaction when that is what it is testing.
/// </summary>
public class StubRedactor : IPersonalDataRedactor
{
    /// <summary>Replaces the text with this, when set.</summary>
    public string? Replacement { get; set; }

    public bool Fails { get; set; }

    public string? SeenText { get; private set; }

    public Task<RedactedText> RedactAsync(string text, CancellationToken cancellationToken)
    {
        SeenText = text;

        return Task.FromResult(Fails
            ? RedactedText.Failed
            : new RedactedText(Replacement ?? text, true));
    }
}

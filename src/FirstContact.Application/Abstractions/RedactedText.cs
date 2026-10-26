namespace FirstContact.Application.Abstractions;

/// <summary>
/// A message with personal data removed, and whether that worked.
/// </summary>
/// <param name="Text">The text with names, e-mail addresses and the like replaced.</param>
/// <param name="Succeeded">
/// False when the service could not be reached. The caller must then not send the text
/// to the model: no answer is better than leaking personal data.
/// </param>
public record RedactedText(string Text, bool Succeeded)
{
    public static RedactedText Failed { get; } = new(string.Empty, false);
}

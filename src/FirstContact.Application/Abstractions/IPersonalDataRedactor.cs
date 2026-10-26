namespace FirstContact.Application.Abstractions;

/// <summary>
/// Removes names, e-mail addresses, phone numbers and similar details from a message.
/// Everything sent to the model goes through this first, so personal data stays in our
/// own database and is not passed on.
/// </summary>
public interface IPersonalDataRedactor
{
    Task<RedactedText> RedactAsync(string text, CancellationToken cancellationToken);
}

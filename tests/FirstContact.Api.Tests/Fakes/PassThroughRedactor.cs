using FirstContact.Application.Abstractions;

namespace FirstContact.Api.Tests.Fakes;

/// <summary>Hands the text back unchanged, so the API tests never call the Language service.</summary>
public class PassThroughRedactor : IPersonalDataRedactor
{
    public Task<RedactedText> RedactAsync(string text, CancellationToken cancellationToken) =>
        Task.FromResult(new RedactedText(text, true));
}

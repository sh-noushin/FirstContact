using Azure;
using Azure.AI.TextAnalytics;
using Azure.Identity;
using FirstContact.Application.Abstractions;
using FirstContact.Infrastructure.Ai;
using Microsoft.Extensions.Logging;

namespace FirstContact.Infrastructure.Privacy;

/// <summary>
/// Uses the Azure Language service to find personal data and replace it, so names,
/// e-mail addresses, phone numbers and account details never reach the model.
/// </summary>
public class AzureLanguageRedactor : IPersonalDataRedactor
{
    private readonly TextAnalyticsClient _language;
    private readonly ILogger<AzureLanguageRedactor> _logger;

    public AzureLanguageRedactor(AzureAiOptions options, ILogger<AzureLanguageRedactor> logger)
    {
        _logger = logger;

        _language = options.UseApiKey
            ? new TextAnalyticsClient(new Uri(options.ServicesEndpoint), new AzureKeyCredential(options.ApiKey))
            : new TextAnalyticsClient(new Uri(options.ServicesEndpoint), new DefaultAzureCredential());
    }

    public async Task<RedactedText> RedactAsync(string text, CancellationToken cancellationToken)
    {
        try
        {
            Response<PiiEntityCollection> found =
                await _language.RecognizePiiEntitiesAsync(text, cancellationToken: cancellationToken);

            return new RedactedText(found.Value.RedactedText, true);
        }
        catch (Exception problem) when (problem is not OperationCanceledException)
        {
            // Failing closed on purpose: the request goes to a person rather than
            // sending text we could not check to the model.
            _logger.LogWarning(problem, "Personal data could not be removed.");

            return RedactedText.Failed;
        }
    }
}

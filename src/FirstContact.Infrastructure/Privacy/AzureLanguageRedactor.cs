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
    /// <summary>
    /// Only genuinely personal details are removed. Organisation names are deliberately
    /// left in: masking "SAP" would turn a useful knowledge gap into "Schnittstelle zu ***?"
    /// without protecting anybody.
    /// </summary>
    private static readonly PiiEntityCategory[] Personal =
    [
        PiiEntityCategory.Person,
        PiiEntityCategory.Email,
        PiiEntityCategory.PhoneNumber,
        PiiEntityCategory.Address,
        PiiEntityCategory.CreditCardNumber,
        PiiEntityCategory.InternationalBankingAccountNumber,
        PiiEntityCategory.IPAddress,
    ];

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
            RecognizePiiEntitiesOptions options = new();

            foreach (PiiEntityCategory category in Personal)
            {
                options.CategoriesFilter.Add(category);
            }

            Response<PiiEntityCollection> found = await _language.RecognizePiiEntitiesAsync(
                text, options: options, cancellationToken: cancellationToken);

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

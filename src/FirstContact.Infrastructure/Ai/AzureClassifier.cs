using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using FirstContact.Application.Abstractions;
using FirstContact.Domain.Requests;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace FirstContact.Infrastructure.Ai;

/// <summary>
/// Asks gpt-5-mini what a message is. Deliberately thin: the prompt lives in
/// <see cref="ClassificationPrompt"/> and the reading of the answer in
/// <see cref="ClassificationParser"/>, so both can be changed and tested on their own.
/// </summary>
public class AzureClassifier : IClassifier
{
    private readonly ChatClient _chat;
    private readonly ILogger<AzureClassifier> _logger;

    public AzureClassifier(AzureAiOptions options, ILogger<AzureClassifier> logger)
    {
        _logger = logger;

        // With a key, use it. Otherwise fall back to the local az login session.
        AzureOpenAIClient azure = options.UseApiKey
            ? new AzureOpenAIClient(new Uri(options.Endpoint), new AzureKeyCredential(options.ApiKey))
            : new AzureOpenAIClient(new Uri(options.Endpoint), new DefaultAzureCredential());

        _chat = azure.GetChatClient(options.ChatDeployment);
    }

    public async Task<Classification> ClassifyAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            var answer = await _chat.CompleteChatAsync(
                [
                    new SystemChatMessage(ClassificationPrompt.Instruction),
                    new UserChatMessage(ClassificationPrompt.For(message))
                ],
                options: null,
                cancellationToken);

            return ClassificationParser.Parse(answer.Value.Content[0].Text);
        }
        catch (Exception problem) when (problem is not OperationCanceledException)
        {
            // A failing model must not lose the customer's message: hand it to a person.
            _logger.LogWarning(problem, "The classifier could not be reached.");

            return Classification.Unclear("Der Dienst war nicht erreichbar.");
        }
    }
}

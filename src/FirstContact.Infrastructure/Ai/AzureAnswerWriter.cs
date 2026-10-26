using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using FirstContact.Application.Abstractions;
using FirstContact.Domain.Knowledge;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace FirstContact.Infrastructure.Ai;

/// <summary>
/// Asks gpt-5-mini to answer from the given articles. Thin on purpose: the wording is in
/// <see cref="AnswerPrompt"/> and the reading of the reply in <see cref="AnswerParser"/>.
/// </summary>
public class AzureAnswerWriter : IAnswerWriter
{
    private readonly ChatClient _chat;
    private readonly ILogger<AzureAnswerWriter> _logger;

    public AzureAnswerWriter(AzureAiOptions options, ILogger<AzureAnswerWriter> logger)
    {
        _logger = logger;

        AzureOpenAIClient azure = options.UseApiKey
            ? new AzureOpenAIClient(new Uri(options.Endpoint), new AzureKeyCredential(options.ApiKey))
            : new AzureOpenAIClient(new Uri(options.Endpoint), new DefaultAzureCredential());

        _chat = azure.GetChatClient(options.ChatDeployment);
    }

    public async Task<GroundedAnswer> WriteAsync(
        string question,
        IReadOnlyList<ArticleMatch> articles,
        CancellationToken cancellationToken)
    {
        try
        {
            var reply = await _chat.CompleteChatAsync(
                [
                    new SystemChatMessage(AnswerPrompt.Instruction),
                    new UserChatMessage(AnswerPrompt.For(question, articles))
                ],
                options: null,
                cancellationToken);

            return AnswerParser.Parse(reply.Value.Content[0].Text);
        }
        catch (Exception problem) when (problem is not OperationCanceledException)
        {
            // Saying nothing is better than guessing: the question becomes a knowledge gap.
            _logger.LogWarning(problem, "The answer writer could not be reached.");

            return GroundedAnswer.NotFound;
        }
    }
}

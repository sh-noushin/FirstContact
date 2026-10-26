using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using OpenAI.Embeddings;

namespace FirstContact.Infrastructure.Ai;

/// <summary>Calls text-embedding-3-small, which returns 1536 numbers per text.</summary>
public class AzureEmbedder : IEmbedder
{
    private readonly EmbeddingClient _embeddings;

    public AzureEmbedder(AzureAiOptions options)
    {
        AzureOpenAIClient azure = options.UseApiKey
            ? new AzureOpenAIClient(new Uri(options.Endpoint), new AzureKeyCredential(options.ApiKey))
            : new AzureOpenAIClient(new Uri(options.Endpoint), new DefaultAzureCredential());

        _embeddings = azure.GetEmbeddingClient(options.EmbeddingDeployment);
    }

    public async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken)
    {
        var result = await _embeddings.GenerateEmbeddingAsync(text, cancellationToken: cancellationToken);

        return result.Value.ToFloats().ToArray();
    }
}

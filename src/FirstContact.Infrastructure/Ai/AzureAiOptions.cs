namespace FirstContact.Infrastructure.Ai;

/// <summary>
/// How to reach Azure AI. Values come from configuration; no secrets are committed.
/// Leave the key empty to sign in with "az login" instead.
/// </summary>
public class AzureAiOptions
{
    public const string SectionName = "AzureAi";

    public string Endpoint { get; set; } = "";

    public string ChatDeployment { get; set; } = "gpt-5-mini";

    public string EmbeddingDeployment { get; set; } = "text-embedding-3-small";

    /// <summary>Only needed where there is no az login session, for example in a container.</summary>
    public string ApiKey { get; set; } = "";

    public bool UseApiKey => !string.IsNullOrWhiteSpace(ApiKey);
}

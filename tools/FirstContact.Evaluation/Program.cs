using System.Text.Json;
using FirstContact.Domain.Requests;
using FirstContact.Evaluation;
using FirstContact.Infrastructure.Ai;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

// Runs the labelled messages through the real classifier and reports how it did.
// This is the only part of the project that calls Azure outside the running app.

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

AzureAiOptions azure = configuration.GetSection(AzureAiOptions.SectionName).Get<AzureAiOptions>()
    ?? new AzureAiOptions();

if (string.IsNullOrWhiteSpace(azure.Endpoint))
{
    Console.Error.WriteLine(
        "No endpoint configured. Copy appsettings.Development.json from the Api project, "
        + "or set AzureAi__Endpoint.");

    return 1;
}

double threshold = configuration.GetValue("Triage:ConfidenceThreshold", RoutingPolicy.DefaultThreshold);

string path = Path.Combine(AppContext.BaseDirectory, "labelled-messages.json");
LabelledMessage[] labelled =
    JsonSerializer.Deserialize<LabelledMessage[]>(
        await File.ReadAllTextAsync(path),
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

Console.WriteLine($"Classifying {labelled.Length} messages against {azure.ChatDeployment} ...");
Console.WriteLine();

AzureClassifier classifier = new(azure, NullLogger<AzureClassifier>.Instance);
Scorecard scorecard = new(threshold);

foreach (LabelledMessage message in labelled)
{
    Classification classification = await classifier.ClassifyAsync(message.Message, CancellationToken.None);
    scorecard.Add(message, classification);

    string mark = classification.Type.ToString() == message.Expected ? "ok  " : "WRONG";
    Console.WriteLine($"  {mark} {classification.Confidence}  {Shorten(message.Message)}");
}

string report = scorecard.ToMarkdown();
string output = Path.Combine(FindRepositoryRoot(), "docs", "evaluation.md");

Directory.CreateDirectory(Path.GetDirectoryName(output)!);
await File.WriteAllTextAsync(output, report);

Console.WriteLine();
Console.WriteLine(report);
Console.WriteLine($"Written to {output}");

return 0;

static string Shorten(string text) => text.Length <= 58 ? text : text[..58] + " ...";

static string FindRepositoryRoot()
{
    DirectoryInfo? directory = new(AppContext.BaseDirectory);

    while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "src")))
    {
        directory = directory.Parent;
    }

    return directory?.FullName ?? AppContext.BaseDirectory;
}

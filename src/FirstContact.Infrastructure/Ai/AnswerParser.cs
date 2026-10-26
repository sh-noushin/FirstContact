using System.Text.Json;
using FirstContact.Domain.Knowledge;

namespace FirstContact.Infrastructure.Ai;

/// <summary>
/// Reads the model's answer. Anything it cannot understand becomes
/// <see cref="GroundedAnswer.NotFound"/>, so a broken reply never reaches a customer.
/// </summary>
public static class AnswerParser
{
    public static GroundedAnswer Parse(string? reply)
    {
        string json = ExtractJson(reply);

        if (json.Length == 0)
        {
            return GroundedAnswer.NotFound;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement root = document.RootElement;

            return GroundedAnswer.From(Text(root, "answer"), Sources(root));
        }
        catch (JsonException)
        {
            return GroundedAnswer.NotFound;
        }
    }

    private static string ExtractJson(string? reply)
    {
        if (string.IsNullOrWhiteSpace(reply))
        {
            return string.Empty;
        }

        int start = reply.IndexOf('{');
        int end = reply.LastIndexOf('}');

        return start >= 0 && end > start ? reply[start..(end + 1)] : string.Empty;
    }

    private static string Text(JsonElement root, string name) =>
        root.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;

    private static IEnumerable<string> Sources(JsonElement root)
    {
        if (!root.TryGetProperty("sources", out JsonElement sources) || sources.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return sources.EnumerateArray()
            .Where(source => source.ValueKind == JsonValueKind.String)
            .Select(source => source.GetString() ?? string.Empty);
    }
}

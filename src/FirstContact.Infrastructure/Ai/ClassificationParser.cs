using System.Text.Json;
using FirstContact.Domain.Requests;

namespace FirstContact.Infrastructure.Ai;

/// <summary>
/// Turns the model's answer into a <see cref="Classification"/>.
/// It never throws: anything it cannot read becomes Unclear, which sends the
/// request to a person rather than letting a broken answer look confident.
/// </summary>
public static class ClassificationParser
{
    public static Classification Parse(string? answer)
    {
        string json = ExtractJson(answer);

        if (json.Length == 0)
        {
            return Classification.Unclear("Das Modell hat kein JSON geliefert.");
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement root = document.RootElement;

            return Classification.Create(
                TypeOf(Text(root, "type")),
                Confidence.FromModel(Number(root, "confidence")),
                UrgencyOf(Text(root, "urgency")),
                Text(root, "reason"));
        }
        catch (JsonException)
        {
            return Classification.Unclear("Die Antwort des Modells war kein gueltiges JSON.");
        }
    }

    /// <summary>Models like to wrap JSON in prose or code fences, so take what is between the braces.</summary>
    private static string ExtractJson(string? answer)
    {
        if (string.IsNullOrWhiteSpace(answer))
        {
            return string.Empty;
        }

        int start = answer.IndexOf('{');
        int end = answer.LastIndexOf('}');

        return start >= 0 && end > start ? answer[start..(end + 1)] : string.Empty;
    }

    /// <summary>An unknown type becomes Other: the model is untrusted input, not a bug.</summary>
    private static RequestType TypeOf(string value) => value.Trim().ToLowerInvariant() switch
    {
        "question" => RequestType.Question,
        "problem" => RequestType.Problem,
        "feature_request" => RequestType.FeatureRequest,
        _ => RequestType.Other
    };

    private static Urgency UrgencyOf(string value) => value.Trim().ToLowerInvariant() switch
    {
        "high" => Urgency.High,
        "low" => Urgency.Low,
        _ => Urgency.Medium
    };

    private static string Text(JsonElement root, string name) =>
        root.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;

    /// <summary>A missing or unreadable confidence counts as zero, which means "ask a person".</summary>
    private static double Number(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out JsonElement value))
        {
            return 0;
        }

        return value.ValueKind switch
        {
            JsonValueKind.Number => value.GetDouble(),
            JsonValueKind.String => double.TryParse(
                value.GetString(),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out double parsed) ? parsed : 0,
            _ => 0
        };
    }
}

using FirstContact.Domain.Requests;

namespace FirstContact.Api.Contracts;

/// <summary>
/// What the browser gets back. A flat shape on purpose, so the domain classes are not
/// tied to the wire format and can change without breaking the frontend.
/// </summary>
public record RequestView(
    int Id,
    string Message,
    string Type,
    double Confidence,
    string Urgency,
    string Reason,
    string Destination,
    string Status,
    bool DecidedByHuman,
    bool AnsweredByBot,
    DateTimeOffset ReceivedAt,
    AnswerView? Answer = null)
{
    public static RequestView Of(CustomerRequest request, AnswerView? answer = null) => new(
        request.Id,
        request.Message,
        request.Classification?.Type.ToString() ?? "",
        request.Classification?.Confidence.Value ?? 0,
        request.Classification?.Urgency.ToString() ?? "",
        request.Classification?.Reason ?? "",
        request.Destination?.ToString() ?? "",
        request.Status.ToString(),
        request.DecidedByHuman,
        request.AnsweredByBot,
        request.ReceivedAt,
        answer);
}

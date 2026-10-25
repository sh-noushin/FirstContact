namespace FirstContact.Domain.Requests;

/// <summary>
/// What the classifier concluded about a message, and why. The reason is kept because
/// staff must be able to see how a request was categorised, not only the result.
/// </summary>
public sealed record Classification
{
    private Classification(RequestType type, Confidence confidence, Urgency urgency, string reason)
    {
        Type = type;
        Confidence = confidence;
        Urgency = urgency;
        Reason = reason;
    }

    public RequestType Type { get; }

    public Confidence Confidence { get; }

    public Urgency Urgency { get; }

    /// <summary>Why the classifier chose this type. Shown to staff.</summary>
    public string Reason { get; }

    public static Classification Create(RequestType type, Confidence confidence, Urgency urgency, string reason) =>
        new(type, confidence, urgency, reason?.Trim() ?? string.Empty);

    /// <summary>
    /// Used when the classifier returned nothing usable. Zero confidence always sends the
    /// request to a person, so a broken answer never becomes a silent guess.
    /// </summary>
    public static Classification Unclear(string reason) =>
        new(RequestType.Other, Confidence.None, Urgency.Medium, reason?.Trim() ?? string.Empty);
}

namespace FirstContact.Domain.Requests;

/// <summary>How quickly the request needs attention. Orders the support queue.</summary>
public enum Urgency
{
    Low,
    Medium,
    High
}

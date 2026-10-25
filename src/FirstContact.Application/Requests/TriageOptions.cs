using FirstContact.Domain.Requests;

namespace FirstContact.Application.Requests;

/// <summary>Settings for the triage pipeline. Bound from configuration by the API.</summary>
public class TriageOptions
{
    public const string SectionName = "Triage";

    /// <summary>
    /// The confidence a classification must reach to be acted on automatically.
    /// Below it, an employee decides.
    /// </summary>
    public double ConfidenceThreshold { get; set; } = RoutingPolicy.DefaultThreshold;
}

namespace FirstContact.Domain.Requests;

/// <summary>
/// Decides where a classified request goes. This is the main safeguard in the system:
/// below the threshold nothing is automated and a person decides instead.
/// </summary>
public static class RoutingPolicy
{
    /// <summary>The confidence a classification must reach to be acted on automatically.</summary>
    public const double DefaultThreshold = 0.70;

    public static Destination Decide(Classification classification, double threshold = DefaultThreshold) =>
        NeedsHuman(classification, threshold)
            ? Destination.HumanReview
            : DestinationFor(classification.Type);

    /// <summary>
    /// True when confidence is strictly below the threshold. A classification exactly at the
    /// threshold is accepted, so the configured number reads as "at least this sure".
    /// </summary>
    public static bool NeedsHuman(Classification classification, double threshold = DefaultThreshold) =>
        classification.Confidence.IsBelow(threshold);

    /// <summary>Where each type belongs, once we are confident enough to act.</summary>
    public static Destination DestinationFor(RequestType type) => type switch
    {
        RequestType.Question => Destination.SelfService,
        RequestType.FeatureRequest => Destination.ProductTeam,
        _ => Destination.SupportTeam
    };
}

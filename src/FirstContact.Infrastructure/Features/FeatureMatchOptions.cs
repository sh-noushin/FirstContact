namespace FirstContact.Infrastructure.Features;

/// <summary>Settings for deciding whether two wishes mean the same thing.</summary>
public class FeatureMatchOptions
{
    public const string SectionName = "FeatureMatching";

    /// <summary>
    /// How close two wishes must be to count as the same idea. Set high on purpose:
    /// merging two different wishes is worse than keeping them apart.
    /// </summary>
    public double MinimumScore { get; set; } = 0.55;
}

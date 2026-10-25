using System.Globalization;

namespace FirstContact.Domain.Requests;

/// <summary>How sure the classifier is, from 0 to 1.</summary>
public readonly record struct Confidence
{
    private Confidence(double value) => Value = value;

    /// <summary>No confidence at all. Always falls below the threshold.</summary>
    public static Confidence None => new(0);

    public double Value { get; }

    /// <summary>Throws if the value is outside 0 to 1. Used by our own code.</summary>
    public static Confidence Create(double value)
    {
        if (double.IsNaN(value) || value < 0 || value > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Confidence must be between 0 and 1.");
        }

        return new Confidence(value);
    }

    /// <summary>
    /// Clamps instead of throwing. Used for model output: an answer of 1.3 is imprecise,
    /// not a reason to drop the customer's message.
    /// </summary>
    public static Confidence FromModel(double value) =>
        new(double.IsNaN(value) ? 0 : Math.Clamp(value, 0, 1));

    public bool IsBelow(double threshold) => Value < threshold;

    public override string ToString() => Value.ToString("0.00", CultureInfo.InvariantCulture);
}

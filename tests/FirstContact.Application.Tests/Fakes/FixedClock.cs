using FirstContact.Application.Abstractions;

namespace FirstContact.Application.Tests.Fakes;

/// <summary>A clock the test controls, so timestamps are predictable.</summary>
public class FixedClock(DateTimeOffset now) : IClock
{
    public DateTimeOffset UtcNow { get; } = now;
}

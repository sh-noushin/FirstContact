using FirstContact.Application.Abstractions;

namespace FirstContact.Infrastructure;

/// <summary>The real clock. Tests use a fixed one instead.</summary>
public class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

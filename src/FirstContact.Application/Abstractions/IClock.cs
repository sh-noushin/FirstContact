namespace FirstContact.Application.Abstractions;

/// <summary>
/// Supplies the current time. Injected instead of calling DateTimeOffset.UtcNow directly,
/// so tests can decide what "now" means.
/// </summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

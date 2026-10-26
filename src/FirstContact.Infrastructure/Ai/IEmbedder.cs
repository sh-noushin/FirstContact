namespace FirstContact.Infrastructure.Ai;

/// <summary>
/// Turns text into numbers so meaning can be compared. Only the Infrastructure layer
/// needs this, so it is not a port in the Application layer.
/// </summary>
public interface IEmbedder
{
    Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken);
}

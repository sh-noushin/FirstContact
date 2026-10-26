namespace FirstContact.Application.Abstractions;

/// <summary>
/// Finds an existing demand that means the same as a new wish. Implemented with
/// embeddings, so "eine App fuers Handy" and "mobile Anwendung" land in one group.
/// </summary>
public interface IFeatureMatcher
{
    /// <summary>The id of the closest matching group, or null when none is close enough.</summary>
    Task<int?> FindSimilarAsync(string wish, CancellationToken cancellationToken);
}

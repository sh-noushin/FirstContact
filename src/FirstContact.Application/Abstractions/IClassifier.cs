using FirstContact.Domain.Requests;

namespace FirstContact.Application.Abstractions;

/// <summary>
/// Works out what a customer message is. Implemented with gpt-5-mini in the
/// Infrastructure layer, so nothing above this line knows about Azure.
/// </summary>
public interface IClassifier
{
    /// <summary>
    /// Never throws on a bad model answer. If the result cannot be understood,
    /// it returns <see cref="Classification.Unclear"/>, which sends the request to a person.
    /// </summary>
    Task<Classification> ClassifyAsync(string message, CancellationToken cancellationToken);
}

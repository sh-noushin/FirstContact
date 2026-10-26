using FirstContact.Domain.Requests;

namespace FirstContact.Application.Abstractions;

/// <summary>Stores customer requests. Backed by SQLite in the Infrastructure layer.</summary>
public interface IRequestRepository
{
    Task AddAsync(CustomerRequest request, CancellationToken cancellationToken);

    Task<CustomerRequest?> FindAsync(int id, CancellationToken cancellationToken);

    /// <summary>The requests waiting for an employee to choose their type, oldest first.</summary>
    Task<IReadOnlyList<CustomerRequest>> ListAwaitingReviewAsync(CancellationToken cancellationToken);

    Task SaveAsync(CancellationToken cancellationToken);
}

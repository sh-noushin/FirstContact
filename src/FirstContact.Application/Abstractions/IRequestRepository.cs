using FirstContact.Domain.Requests;

namespace FirstContact.Application.Abstractions;

/// <summary>Stores customer requests. Backed by SQLite in the Infrastructure layer.</summary>
public interface IRequestRepository
{
    Task AddAsync(CustomerRequest request, CancellationToken cancellationToken);

    Task<CustomerRequest?> FindAsync(int id, CancellationToken cancellationToken);

    Task SaveAsync(CancellationToken cancellationToken);
}

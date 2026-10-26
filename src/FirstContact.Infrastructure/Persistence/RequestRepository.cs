using FirstContact.Application.Abstractions;
using FirstContact.Domain.Requests;
using Microsoft.EntityFrameworkCore;

namespace FirstContact.Infrastructure.Persistence;

/// <summary>Stores customer requests in SQLite.</summary>
public class RequestRepository(FirstContactDbContext database) : IRequestRepository
{
    public async Task AddAsync(CustomerRequest request, CancellationToken cancellationToken) =>
        await database.Requests.AddAsync(request, cancellationToken);

    public Task<CustomerRequest?> FindAsync(int id, CancellationToken cancellationToken) =>
        database.Requests.FirstOrDefaultAsync(request => request.Id == id, cancellationToken);

    public async Task<IReadOnlyList<CustomerRequest>> ListAwaitingReviewAsync(CancellationToken cancellationToken) =>
        await database.Requests
            .Where(request => request.Status == RequestStatus.AwaitingReview)
            .OrderBy(request => request.ReceivedAt)
            .ToListAsync(cancellationToken);

    public Task SaveAsync(CancellationToken cancellationToken) =>
        database.SaveChangesAsync(cancellationToken);
}

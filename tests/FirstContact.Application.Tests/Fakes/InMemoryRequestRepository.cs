using FirstContact.Application.Abstractions;
using FirstContact.Domain.Requests;

namespace FirstContact.Application.Tests.Fakes;

/// <summary>Keeps requests in a list instead of SQLite.</summary>
public class InMemoryRequestRepository : IRequestRepository
{
    private readonly List<CustomerRequest> _requests = [];

    public IReadOnlyList<CustomerRequest> All => _requests;

    public int Saves { get; private set; }

    public Task AddAsync(CustomerRequest request, CancellationToken cancellationToken)
    {
        _requests.Add(request);
        return Task.CompletedTask;
    }

    /// <summary>Ids come from the database in real life, so the position in the list stands in for one.</summary>
    public Task<CustomerRequest?> FindAsync(int id, CancellationToken cancellationToken) =>
        Task.FromResult(_requests.ElementAtOrDefault(id - 1));

    public Task<IReadOnlyList<CustomerRequest>> ListAwaitingReviewAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<CustomerRequest>>(
            _requests.Where(request => request.Status == RequestStatus.AwaitingReview)
                .OrderBy(request => request.ReceivedAt)
                .ToList());

    public Task SaveAsync(CancellationToken cancellationToken)
    {
        Saves++;
        return Task.CompletedTask;
    }
}

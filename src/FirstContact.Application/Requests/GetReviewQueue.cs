using FirstContact.Application.Abstractions;
using FirstContact.Domain.Requests;

namespace FirstContact.Application.Requests;

/// <summary>Lists the requests the classifier was not sure enough about.</summary>
public class GetReviewQueue(IRequestRepository requests)
{
    public Task<IReadOnlyList<CustomerRequest>> HandleAsync(CancellationToken cancellationToken) =>
        requests.ListAwaitingReviewAsync(cancellationToken);
}

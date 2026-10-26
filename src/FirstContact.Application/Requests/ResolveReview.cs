using FirstContact.Application.Abstractions;
using FirstContact.Domain.Requests;

namespace FirstContact.Application.Requests;

/// <summary>
/// An employee picked the type for a request the classifier was unsure about.
/// From there it takes the same route as a confident one, so the human decision
/// and the automatic one cannot behave differently.
/// </summary>
public class ResolveReview(IRequestRepository requests)
{
    public async Task<CustomerRequest> HandleAsync(int id, RequestType type, CancellationToken cancellationToken)
    {
        CustomerRequest request = await requests.FindAsync(id, cancellationToken)
            ?? throw new RequestNotFoundException(id);

        request.ClassifyByHuman(type);
        request.RouteTo(RoutingPolicy.DestinationFor(type));

        await requests.SaveAsync(cancellationToken);

        return request;
    }
}

using FirstContact.Application.Abstractions;
using FirstContact.Domain.Requests;

namespace FirstContact.Application.Requests;

/// <summary>
/// Handles one customer message: classify it, apply the confidence rule, send it on.
/// The whole use case is four steps, so it reads like the rule it implements.
/// </summary>
public class HandleIncomingMessage(
    IClassifier classifier,
    IRequestRepository requests,
    IClock clock,
    TriageOptions options)
{
    public async Task<CustomerRequest> HandleAsync(string message, CancellationToken cancellationToken)
    {
        CustomerRequest request = CustomerRequest.Receive(message, clock.UtcNow);

        Classification classification = await classifier.ClassifyAsync(request.Message, cancellationToken);
        request.Classify(classification);

        Route(request, classification);

        await requests.AddAsync(request, cancellationToken);
        await requests.SaveAsync(cancellationToken);

        return request;
    }

    /// <summary>
    /// Below the threshold the request waits for a person; otherwise it goes straight
    /// to the team that handles its type.
    /// </summary>
    private void Route(CustomerRequest request, Classification classification)
    {
        Destination destination = RoutingPolicy.Decide(classification, options.ConfidenceThreshold);

        if (destination == Destination.HumanReview)
        {
            request.SendToHumanReview();
            return;
        }

        request.RouteTo(destination);
    }
}

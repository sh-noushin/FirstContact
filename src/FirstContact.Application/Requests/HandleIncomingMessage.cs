using FirstContact.Application.Abstractions;
using FirstContact.Application.Knowledge;
using FirstContact.Domain.Knowledge;
using FirstContact.Domain.Requests;

namespace FirstContact.Application.Requests;

/// <summary>
/// Handles one customer message: classify it, apply the confidence rule, send it on.
/// The whole use case is four steps, so it reads like the rule it implements.
/// </summary>
public class HandleIncomingMessage(
    IClassifier classifier,
    IRequestRepository requests,
    AnswerFromHandbook answerFromHandbook,
    IClock clock,
    TriageOptions options)
{
    public async Task<TriagedRequest> HandleAsync(string message, CancellationToken cancellationToken)
    {
        CustomerRequest request = CustomerRequest.Receive(message, clock.UtcNow);

        Classification classification = await classifier.ClassifyAsync(request.Message, cancellationToken);
        request.Classify(classification);

        Route(request, classification);

        GroundedAnswer answer = await AnswerAsync(request, cancellationToken);

        await requests.AddAsync(request, cancellationToken);
        await requests.SaveAsync(cancellationToken);

        return new TriagedRequest(request, answer);
    }

    /// <summary>
    /// A question that reached self-service is answered from the handbook straight away.
    /// Whether that worked is recorded on the request, so the self-service rate is measured
    /// rather than estimated.
    /// </summary>
    private async Task<GroundedAnswer> AnswerAsync(CustomerRequest request, CancellationToken cancellationToken)
    {
        if (request.Destination != Destination.SelfService)
        {
            return GroundedAnswer.NotFound;
        }

        GroundedAnswer answer = await answerFromHandbook.HandleAsync(request.Message, cancellationToken);
        request.RecordSelfServiceAnswer(answer.Answered);

        return answer;
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

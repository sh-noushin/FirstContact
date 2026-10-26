namespace FirstContact.Domain.Requests;

/// <summary>
/// One message from a customer and what became of it. The class protects two rules:
/// nothing happens before the message is classified, and it is routed only once.
/// </summary>
public class CustomerRequest
{
    /// <summary>EF Core needs a parameterless constructor.</summary>
    private CustomerRequest()
    {
        Message = string.Empty;
    }

    private CustomerRequest(string message, DateTimeOffset receivedAt)
    {
        Message = message;
        ReceivedAt = receivedAt;
        Status = RequestStatus.New;
    }

    /// <summary>Set by the database when the request is saved.</summary>
    public int Id { get; private set; }

    public string Message { get; private set; }

    public DateTimeOffset ReceivedAt { get; private set; }

    public RequestStatus Status { get; private set; }

    /// <summary>Null until the request has been classified.</summary>
    public Classification? Classification { get; private set; }

    /// <summary>Null until the request has been routed.</summary>
    public Destination? Destination { get; private set; }

    /// <summary>True when an employee chose the type instead of the classifier.</summary>
    public bool DecidedByHuman { get; private set; }

    /// <summary>
    /// True when the bot answered from the handbook, so nobody had to. This is what the
    /// self-service rate counts, which is why it is recorded rather than guessed at.
    /// </summary>
    public bool AnsweredByBot { get; private set; }

    public static CustomerRequest Receive(string message, DateTimeOffset receivedAt)
    {
        string trimmed = message?.Trim() ?? string.Empty;

        if (trimmed.Length == 0)
        {
            throw new ArgumentException("A request needs a message.", nameof(message));
        }

        return new CustomerRequest(trimmed, receivedAt);
    }

    public void Classify(Classification classification)
    {
        EnsureNotRouted(nameof(Classify));

        Classification = classification;
        Status = RequestStatus.Classified;
    }

    /// <summary>Parks the request because the classifier was not sure enough.</summary>
    public void SendToHumanReview()
    {
        EnsureClassified(nameof(SendToHumanReview));
        EnsureNotRouted(nameof(SendToHumanReview));

        Status = RequestStatus.AwaitingReview;
        Destination = Requests.Destination.HumanReview;
    }

    /// <summary>An employee picked the type. Only possible while the request waits for review.</summary>
    public void ClassifyByHuman(RequestType type)
    {
        Classification current = EnsureClassified(nameof(ClassifyByHuman));

        if (Status != RequestStatus.AwaitingReview)
        {
            throw new InvalidOperationException(
                $"Only a request waiting for review can be reclassified, but this one is {Status}.");
        }

        Classification = Classification.Create(type, current.Confidence, current.Urgency, current.Reason);
        DecidedByHuman = true;
    }

    /// <summary>Sends the request to its destination. A request is routed only once.</summary>
    public void RouteTo(Destination destination)
    {
        EnsureClassified(nameof(RouteTo));
        EnsureNotRouted(nameof(RouteTo));

        Destination = destination;
        Status = RequestStatus.Routed;
    }

    /// <summary>Records whether the handbook could answer, once the request reached self-service.</summary>
    public void RecordSelfServiceAnswer(bool answered)
    {
        if (Destination != Requests.Destination.SelfService)
        {
            throw new InvalidOperationException(
                "Only a request that reached self-service can record a handbook answer.");
        }

        AnsweredByBot = answered;
    }

    private Classification EnsureClassified(string action) =>
        Classification ?? throw new InvalidOperationException(
            $"The request must be classified before it can {action}.");

    private void EnsureNotRouted(string action)
    {
        if (Status == RequestStatus.Routed)
        {
            throw new InvalidOperationException($"The request is already routed and cannot {action} again.");
        }
    }
}

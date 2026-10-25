namespace FirstContact.Domain.Requests;

/// <summary>Where the request stands.</summary>
public enum RequestStatus
{
    /// <summary>Received but not classified yet.</summary>
    New,

    /// <summary>Classified, but not yet sent anywhere.</summary>
    Classified,

    /// <summary>Waiting for an employee to choose the type.</summary>
    AwaitingReview,

    /// <summary>Finished: the request reached its destination.</summary>
    Routed
}

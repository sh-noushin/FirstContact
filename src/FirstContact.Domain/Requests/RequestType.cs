namespace FirstContact.Domain.Requests;

/// <summary>What kind of message the customer sent.</summary>
public enum RequestType
{
    /// <summary>A question about how to use the product. The handbook can usually answer it.</summary>
    Question,

    /// <summary>Something is broken or behaving unexpectedly.</summary>
    Problem,

    /// <summary>A wish for something the product cannot do yet.</summary>
    FeatureRequest,

    /// <summary>Licences, invoices, contracts: a person has to handle it.</summary>
    Other
}

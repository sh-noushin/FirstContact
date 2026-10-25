namespace FirstContact.Domain.Requests;

/// <summary>Who deals with the request.</summary>
public enum Destination
{
    /// <summary>Answered automatically from the handbook. No human involved.</summary>
    SelfService,

    SupportTeam,

    ProductTeam,

    /// <summary>The classification was not certain enough, so a person decides.</summary>
    HumanReview
}

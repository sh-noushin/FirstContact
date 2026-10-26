using FirstContact.Domain.Knowledge;
using FirstContact.Domain.Requests;

namespace FirstContact.Application.Requests;

/// <summary>
/// The result of handling one message: where it went, and the handbook answer if the
/// bot was able to give one.
/// </summary>
public record TriagedRequest(CustomerRequest Request, GroundedAnswer Answer);

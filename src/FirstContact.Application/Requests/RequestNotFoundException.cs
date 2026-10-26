namespace FirstContact.Application.Requests;

/// <summary>
/// The request someone referred to does not exist. The API turns this into a 404,
/// which is different from a broken rule.
/// </summary>
public class RequestNotFoundException(int id)
    : Exception($"Request {id} does not exist.")
{
    public int Id { get; } = id;
}

using FirstContact.Api.Contracts;
using FirstContact.Application.Requests;
using FirstContact.Domain.Requests;

namespace FirstContact.Api.Endpoints;

/// <summary>The /api/requests endpoints.</summary>
public static class RequestEndpoints
{
    /// <summary>Longer than this is not a support request, it is a document.</summary>
    private const int MaxMessageLength = 4000;

    public static void MapRequests(this WebApplication app)
    {
        app.MapPost("/api/requests", async (
            NewRequest body,
            HandleIncomingMessage handle,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(body.Message))
            {
                return Results.BadRequest(new { error = "Bitte eine Nachricht eingeben." });
            }

            if (body.Message.Length > MaxMessageLength)
            {
                return Results.BadRequest(new { error = $"Die Nachricht ist zu lang (max. {MaxMessageLength} Zeichen)." });
            }

            CustomerRequest request = await handle.HandleAsync(body.Message, cancellationToken);

            return Results.Ok(RequestView.Of(request));
        });
    }
}

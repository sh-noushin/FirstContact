using FirstContact.Api.Contracts;
using FirstContact.Application.Knowledge;
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
            AnswerFromHandbook answerFromHandbook,
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

            // A question that went to self-service gets answered from the handbook.
            AnswerView? answer = request.Destination == Destination.SelfService
                ? AnswerView.Of(await answerFromHandbook.HandleAsync(request.Message, cancellationToken))
                : null;

            return Results.Ok(RequestView.Of(request, answer));
        });

        app.MapGet("/api/requests/review", async (
            GetReviewQueue queue,
            CancellationToken cancellationToken) =>
        {
            IReadOnlyList<CustomerRequest> waiting = await queue.HandleAsync(cancellationToken);

            return Results.Ok(waiting.Select(request => RequestView.Of(request)));
        });

        app.MapPost("/api/requests/{id:int}/resolve", async (
            int id,
            ReviewDecision body,
            ResolveReview resolve,
            CancellationToken cancellationToken) =>
        {
            if (!Enum.TryParse(body.Type, ignoreCase: true, out RequestType type))
            {
                return Results.BadRequest(new { error = $"'{body.Type}' ist kein gueltiger Typ." });
            }

            try
            {
                CustomerRequest request = await resolve.HandleAsync(id, type, cancellationToken);

                return Results.Ok(RequestView.Of(request));
            }
            catch (RequestNotFoundException)
            {
                return Results.NotFound(new { error = $"Anfrage {id} wurde nicht gefunden." });
            }
            catch (InvalidOperationException problem)
            {
                // The request was not waiting for review, so the rule in the domain refused.
                return Results.Conflict(new { error = problem.Message });
            }
        });
    }
}

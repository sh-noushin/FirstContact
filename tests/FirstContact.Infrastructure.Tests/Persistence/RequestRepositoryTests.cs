using FirstContact.Domain.Requests;
using FirstContact.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FirstContact.Infrastructure.Tests.Persistence;

/// <summary>
/// Saves a request and reads it back with a different context, so the values really
/// went through the database instead of being remembered in memory.
/// </summary>
public class RequestRepositoryTests : IDisposable
{
    private static readonly DateTimeOffset Now = new(2026, 10, 25, 9, 0, 0, TimeSpan.Zero);

    private readonly SqliteTestDatabase _database = new();

    [Fact]
    public async Task A_saved_request_can_be_read_back()
    {
        int id = await SaveAsync(Routed());

        await using FirstContactDbContext reading = _database.NewContext();
        CustomerRequest? found = await new RequestRepository(reading).FindAsync(id, CancellationToken.None);

        Assert.NotNull(found);
        Assert.Equal("Wie exportiere ich ein Dokument?", found.Message);
        Assert.Equal(Now, found.ReceivedAt);
    }

    [Fact]
    public async Task The_classification_survives_the_round_trip()
    {
        int id = await SaveAsync(Routed());

        await using FirstContactDbContext reading = _database.NewContext();
        CustomerRequest found = (await new RequestRepository(reading).FindAsync(id, CancellationToken.None))!;

        Assert.NotNull(found.Classification);
        Assert.Equal(RequestType.Question, found.Classification.Type);
        Assert.Equal(0.91, found.Classification.Confidence.Value, 6);
        Assert.Equal(Urgency.High, found.Classification.Urgency);
        Assert.Equal("Kunde fragt nach dem Export", found.Classification.Reason);
    }

    [Fact]
    public async Task The_status_and_destination_survive_the_round_trip()
    {
        int id = await SaveAsync(Routed());

        await using FirstContactDbContext reading = _database.NewContext();
        CustomerRequest found = (await new RequestRepository(reading).FindAsync(id, CancellationToken.None))!;

        Assert.Equal(RequestStatus.Routed, found.Status);
        Assert.Equal(Destination.SelfService, found.Destination);
    }

    [Fact]
    public async Task A_request_waiting_for_review_keeps_its_empty_destination_and_flag()
    {
        CustomerRequest request = Classified();
        request.SendToHumanReview();
        request.ClassifyByHuman(RequestType.Problem);

        int id = await SaveAsync(request);

        await using FirstContactDbContext reading = _database.NewContext();
        CustomerRequest found = (await new RequestRepository(reading).FindAsync(id, CancellationToken.None))!;

        Assert.Equal(RequestStatus.AwaitingReview, found.Status);
        Assert.Equal(Destination.HumanReview, found.Destination);
        Assert.True(found.DecidedByHuman);
        Assert.Equal(RequestType.Problem, found.Classification!.Type);
    }

    [Fact]
    public async Task Enums_are_stored_as_readable_text()
    {
        await SaveAsync(Routed());

        await using FirstContactDbContext reading = _database.NewContext();
        await using var command = reading.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT Status, Destination FROM Requests LIMIT 1";

        await using var row = await command.ExecuteReaderAsync();
        await row.ReadAsync();

        Assert.Equal("Routed", row.GetString(0));
        Assert.Equal("SelfService", row.GetString(1));
    }

    [Fact]
    public async Task An_unknown_id_returns_nothing()
    {
        await using FirstContactDbContext reading = _database.NewContext();

        Assert.Null(await new RequestRepository(reading).FindAsync(999, CancellationToken.None));
    }

    [Fact]
    public async Task Nothing_is_stored_until_save_is_called()
    {
        await using FirstContactDbContext writing = _database.NewContext();
        await new RequestRepository(writing).AddAsync(Routed(), CancellationToken.None);

        await using FirstContactDbContext reading = _database.NewContext();
        Assert.Empty(reading.Requests);
    }

    public void Dispose() => _database.Dispose();

    private async Task<int> SaveAsync(CustomerRequest request)
    {
        await using FirstContactDbContext writing = _database.NewContext();
        var repository = new RequestRepository(writing);

        await repository.AddAsync(request, CancellationToken.None);
        await repository.SaveAsync(CancellationToken.None);

        return request.Id;
    }

    private static CustomerRequest Classified()
    {
        CustomerRequest request = CustomerRequest.Receive("Wie exportiere ich ein Dokument?", Now);
        request.Classify(Classification.Create(
            RequestType.Question, Confidence.Create(0.91), Urgency.High, "Kunde fragt nach dem Export"));

        return request;
    }

    private static CustomerRequest Routed()
    {
        CustomerRequest request = Classified();
        request.RouteTo(Destination.SelfService);

        return request;
    }
}

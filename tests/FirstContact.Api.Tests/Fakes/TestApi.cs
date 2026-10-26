using FirstContact.Application.Abstractions;
using FirstContact.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FirstContact.Api.Tests.Fakes;

/// <summary>
/// Runs the real application in memory, with two things swapped: the classifier becomes a
/// stub, and the database becomes an in-memory SQLite one. Everything else is production code.
/// </summary>
public class TestApi : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public StubClassifier Classifier { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        builder.UseSetting("AzureAi:Endpoint", "https://example.invalid");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IClassifier>();
            services.AddScoped<IClassifier>(_ => Classifier);

            services.RemoveAll<DbContextOptions<FirstContactDbContext>>();
            services.AddDbContext<FirstContactDbContext>(options => options.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }
}

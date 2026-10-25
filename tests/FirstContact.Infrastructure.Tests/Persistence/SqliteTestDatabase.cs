using FirstContact.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FirstContact.Infrastructure.Tests.Persistence;

/// <summary>
/// A real SQLite database held in memory. Real, because the point of these tests is to
/// prove the mapping works; in memory, because they should still run in milliseconds.
/// The connection stays open: closing it would delete the database.
/// </summary>
public sealed class SqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteTestDatabase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        using FirstContactDbContext setup = NewContext();
        setup.Database.EnsureCreated();
    }

    /// <summary>A fresh context, so a test can save with one and read with another.</summary>
    public FirstContactDbContext NewContext()
    {
        DbContextOptions<FirstContactDbContext> options =
            new DbContextOptionsBuilder<FirstContactDbContext>()
                .UseSqlite(_connection)
                .Options;

        return new FirstContactDbContext(options);
    }

    public void Dispose() => _connection.Dispose();
}

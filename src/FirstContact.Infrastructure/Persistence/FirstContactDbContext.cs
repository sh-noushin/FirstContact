using FirstContact.Domain.Requests;
using Microsoft.EntityFrameworkCore;

namespace FirstContact.Infrastructure.Persistence;

/// <summary>The SQLite database. One table per thing we store.</summary>
public class FirstContactDbContext(DbContextOptions<FirstContactDbContext> options) : DbContext(options)
{
    public DbSet<CustomerRequest> Requests => Set<CustomerRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FirstContactDbContext).Assembly);
    }
}

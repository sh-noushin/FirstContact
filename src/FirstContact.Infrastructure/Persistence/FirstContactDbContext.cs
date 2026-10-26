using FirstContact.Domain.Features;
using FirstContact.Domain.Knowledge;
using FirstContact.Domain.Requests;
using Microsoft.EntityFrameworkCore;

namespace FirstContact.Infrastructure.Persistence;

/// <summary>The SQLite database. One table per thing we store.</summary>
public class FirstContactDbContext(DbContextOptions<FirstContactDbContext> options) : DbContext(options)
{
    public DbSet<CustomerRequest> Requests => Set<CustomerRequest>();

    public DbSet<KnowledgeArticle> Articles => Set<KnowledgeArticle>();

    public DbSet<KnowledgeGap> KnowledgeGaps => Set<KnowledgeGap>();

    public DbSet<FeatureDemand> FeatureDemands => Set<FeatureDemand>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FirstContactDbContext).Assembly);
    }
}

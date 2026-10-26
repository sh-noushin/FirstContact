using FirstContact.Domain.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FirstContact.Infrastructure.Persistence;

/// <summary>How a group of feature wishes is stored, including the vector used to match it.</summary>
public class FeatureDemandConfiguration : IEntityTypeConfiguration<FeatureDemand>
{
    /// <summary>The column holding the embedding of the first wish in the group.</summary>
    public const string EmbeddingColumn = "Embedding";

    public void Configure(EntityTypeBuilder<FeatureDemand> builder)
    {
        builder.ToTable("FeatureDemands");
        builder.HasKey(demand => demand.Id);

        builder.Property(demand => demand.Title).IsRequired().HasMaxLength(200);

        builder.Property(demand => demand.FirstSeen)
            .HasConversion(moment => moment.UtcTicks, ticks => new DateTimeOffset(ticks, TimeSpan.Zero));

        builder.Property(demand => demand.LastSeen)
            .HasConversion(moment => moment.UtcTicks, ticks => new DateTimeOffset(ticks, TimeSpan.Zero));

        builder.Property<byte[]>(EmbeddingColumn);

        // The wishes belong to the group and are only reached through it.
        builder.HasMany(demand => demand.Wishes)
            .WithOne()
            .HasForeignKey("FeatureDemandId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(demand => demand.Wishes).AutoInclude();
    }
}

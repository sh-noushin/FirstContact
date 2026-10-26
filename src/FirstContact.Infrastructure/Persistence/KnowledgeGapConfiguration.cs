using FirstContact.Domain.Knowledge;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FirstContact.Infrastructure.Persistence;

public class KnowledgeGapConfiguration : IEntityTypeConfiguration<KnowledgeGap>
{
    public void Configure(EntityTypeBuilder<KnowledgeGap> builder)
    {
        builder.ToTable("KnowledgeGaps");
        builder.HasKey(gap => gap.Id);

        builder.Property(gap => gap.Question).IsRequired().HasMaxLength(1000);
        builder.Property(gap => gap.TimesAsked);

        // SQLite cannot sort by DateTimeOffset, so times are stored as UTC ticks.
        builder.Property(gap => gap.FirstAskedAt)
            .HasConversion(moment => moment.UtcTicks, ticks => new DateTimeOffset(ticks, TimeSpan.Zero));

        builder.Property(gap => gap.LastAskedAt)
            .HasConversion(moment => moment.UtcTicks, ticks => new DateTimeOffset(ticks, TimeSpan.Zero));
    }
}

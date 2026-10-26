using FirstContact.Domain.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FirstContact.Infrastructure.Persistence;

/// <summary>
/// Tells EF Core how to store a request. Kept out of the domain class so the
/// rules stay free of database concerns.
/// </summary>
public class CustomerRequestConfiguration : IEntityTypeConfiguration<CustomerRequest>
{
    public void Configure(EntityTypeBuilder<CustomerRequest> builder)
    {
        builder.ToTable("Requests");
        builder.HasKey(request => request.Id);

        builder.Property(request => request.Message).IsRequired().HasMaxLength(4000);
        // SQLite cannot sort by DateTimeOffset, so the time is stored as UTC ticks.
        // Everything is recorded in UTC, so nothing is lost.
        builder.Property(request => request.ReceivedAt)
            .HasConversion(
                moment => moment.UtcTicks,
                ticks => new DateTimeOffset(ticks, TimeSpan.Zero))
            .IsRequired();

        // Enums are stored as text, so the database stays readable and survives
        // someone reordering the enum members.
        builder.Property(request => request.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(request => request.Destination).HasConversion<string>().HasMaxLength(30);
        builder.Property(request => request.DecidedByHuman);

        // The classification has no identity of its own: it is part of the request.
        builder.OwnsOne(request => request.Classification, classification =>
        {
            classification.Property(c => c.Type).HasConversion<string>().HasMaxLength(30).IsRequired();
            classification.Property(c => c.Urgency).HasConversion<string>().HasMaxLength(20).IsRequired();
            classification.Property(c => c.Reason).HasMaxLength(1000);

            classification.Property(c => c.Confidence)
                .HasConversion(confidence => confidence.Value, value => Confidence.Create(value))
                .HasColumnName("Confidence");
        });
    }
}

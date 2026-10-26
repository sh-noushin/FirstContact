using FirstContact.Domain.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FirstContact.Infrastructure.Persistence;

public class FeatureWishConfiguration : IEntityTypeConfiguration<FeatureWish>
{
    public void Configure(EntityTypeBuilder<FeatureWish> builder)
    {
        builder.ToTable("FeatureWishes");
        builder.HasKey(wish => wish.Id);

        builder.Property(wish => wish.RequestId).IsRequired();
        builder.Property(wish => wish.Quote).IsRequired().HasMaxLength(4000);

        builder.Property(wish => wish.At)
            .HasConversion(moment => moment.UtcTicks, ticks => new DateTimeOffset(ticks, TimeSpan.Zero));
    }
}

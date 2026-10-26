using FirstContact.Domain.Features;

namespace FirstContact.Domain.Tests.Features;

/// <summary>
/// Demand is the number product management acts on, so it has to survive retries:
/// the same request may never be counted twice.
/// </summary>
public class FeatureDemandTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 26, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void A_new_group_starts_with_its_first_customer()
    {
        FeatureDemand demand = Start();

        Assert.Equal("Mobile App", demand.Title);
        Assert.Equal(1, demand.CustomerCount);
        Assert.Equal("Eine App fuers Handy waere super", Assert.Single(demand.Wishes).Quote);
        Assert.Equal(Now, demand.FirstSeen);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void A_group_needs_a_title(string? title)
    {
        Assert.Throws<ArgumentException>(() => FeatureDemand.Start(title!, 1, "Ein Wunsch", Now));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void A_wish_needs_the_customers_words(string? quote)
    {
        Assert.Throws<ArgumentException>(() => FeatureDemand.Start("Titel", 1, quote!, Now));
    }

    [Fact]
    public void A_second_customer_raises_the_count()
    {
        FeatureDemand demand = Start();

        demand.Record(2, "Bitte eine App fuers Smartphone", Now.AddDays(1));

        Assert.Equal(2, demand.CustomerCount);
        Assert.Equal(Now.AddDays(1), demand.LastSeen);
    }

    [Fact]
    public void The_same_request_is_never_counted_twice()
    {
        FeatureDemand demand = Start();

        demand.Record(1, "Noch einmal dasselbe", Now);
        demand.Record(1, "Und noch einmal", Now);

        Assert.Equal(1, demand.CustomerCount);
    }

    [Fact]
    public void An_earlier_timestamp_does_not_move_last_seen_backwards()
    {
        FeatureDemand demand = Start();

        demand.Record(2, "Spaeter gemeldet", Now.AddDays(-5));

        Assert.Equal(Now, demand.LastSeen);
    }

    [Fact]
    public void Staff_can_give_the_group_a_clearer_name()
    {
        FeatureDemand demand = Start();

        demand.Rename("  Mobile App fuer Freigaben  ");

        Assert.Equal("Mobile App fuer Freigaben", demand.Title);
    }

    [Fact]
    public void A_group_cannot_lose_its_title()
    {
        FeatureDemand demand = Start();

        Assert.Throws<ArgumentException>(() => demand.Rename("   "));
        Assert.Equal("Mobile App", demand.Title);
    }

    private static FeatureDemand Start() =>
        FeatureDemand.Start("Mobile App", 1, "Eine App fuers Handy waere super", Now);
}

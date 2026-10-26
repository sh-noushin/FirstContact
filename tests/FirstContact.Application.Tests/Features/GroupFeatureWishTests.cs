using FirstContact.Application.Features;
using FirstContact.Application.Tests.Fakes;
using FirstContact.Domain.Features;

namespace FirstContact.Application.Tests.Features;

public class GroupFeatureWishTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 26, 9, 0, 0, TimeSpan.Zero);

    private readonly StubFeatureMatcher _matcher = new();
    private readonly InMemoryFeatureDemandRepository _demands = new();

    [Fact]
    public async Task A_wish_that_matches_nothing_starts_a_new_group()
    {
        FeatureDemand demand = await Group(1, "Eine App fuers Handy waere super");

        Assert.Single(_demands.All);
        Assert.Equal(1, demand.CustomerCount);
        Assert.Equal("Eine App fuers Handy waere super", demand.Title);
    }

    [Fact]
    public async Task A_matching_wish_joins_the_existing_group()
    {
        await Group(1, "Eine App fuers Handy waere super");
        _matcher.Match = 1;

        FeatureDemand demand = await Group(2, "Bitte eine mobile Anwendung");

        Assert.Single(_demands.All);
        Assert.Equal(2, demand.CustomerCount);
    }

    [Fact]
    public async Task The_same_request_arriving_twice_does_not_inflate_demand()
    {
        await Group(1, "Eine App fuers Handy waere super");
        _matcher.Match = 1;

        await Group(1, "Eine App fuers Handy waere super");

        Assert.Equal(1, _demands.All.Single().CustomerCount);
    }

    [Fact]
    public async Task A_long_wish_becomes_a_shortened_title()
    {
        string wish = "Es waere wirklich grossartig, wenn es eine mobile Anwendung gaebe, "
            + "mit der man Dokumente auch unterwegs freigeben koennte";

        FeatureDemand demand = await Group(1, wish);

        Assert.True(demand.Title.Length <= 66);
        Assert.EndsWith("...", demand.Title, StringComparison.Ordinal);
        Assert.StartsWith("Es waere wirklich grossartig", demand.Title, StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_full_wording_is_kept_even_when_the_title_is_shortened()
    {
        string wish = "Es waere wirklich grossartig, wenn es eine mobile Anwendung gaebe, "
            + "mit der man Dokumente auch unterwegs freigeben koennte";

        FeatureDemand demand = await Group(1, wish);

        Assert.Equal(wish, Assert.Single(demand.Wishes).Quote);
    }

    [Fact]
    public async Task A_new_group_is_stored_with_the_text_to_match_against()
    {
        await Group(1, "Eine App fuers Handy waere super");

        Assert.Equal("Eine App fuers Handy waere super", Assert.Single(_demands.MatchTexts));
    }

    [Fact]
    public async Task A_match_against_a_group_that_vanished_starts_a_new_one()
    {
        _matcher.Match = 99;

        FeatureDemand demand = await Group(1, "Eine App fuers Handy waere super");

        Assert.Single(_demands.All);
        Assert.Equal(1, demand.CustomerCount);
    }

    [Fact]
    public async Task Groups_are_listed_with_the_most_wanted_first()
    {
        await Group(1, "Eine App fuers Handy");
        _matcher.Match = null;
        await Group(2, "Export nach Excel");
        _matcher.Match = 1;
        await Group(3, "Eine mobile Anwendung");

        IReadOnlyList<FeatureDemand> listed = await _demands.ListAsync(CancellationToken.None);

        Assert.Equal(2, listed[0].CustomerCount);
        Assert.Equal(1, listed[1].CustomerCount);
    }

    private Task<FeatureDemand> Group(int requestId, string wish) =>
        new GroupFeatureWish(_matcher, _demands, new FixedClock(Now))
            .HandleAsync(requestId, wish, CancellationToken.None);
}

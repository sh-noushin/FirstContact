using FirstContact.Domain.Requests;

namespace FirstContact.Domain.Tests.Requests;

public class ConfidenceTests
{
    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void Create_accepts_values_between_zero_and_one(double value)
    {
        Assert.Equal(value, Confidence.Create(value).Value, 6);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    [InlineData(double.NaN)]
    public void Create_rejects_values_outside_the_range(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Confidence.Create(value));
    }

    [Theory]
    [InlineData(1.3, 1.0)]
    [InlineData(-2.0, 0.0)]
    [InlineData(double.NaN, 0.0)]
    [InlineData(0.84, 0.84)]
    public void Model_output_is_clamped_rather_than_rejected(double raw, double expected)
    {
        Assert.Equal(expected, Confidence.FromModel(raw).Value, 6);
    }

    [Fact]
    public void None_is_below_every_threshold()
    {
        Assert.True(Confidence.None.IsBelow(0.01));
    }

    [Fact]
    public void A_value_equal_to_the_threshold_is_not_below_it()
    {
        Assert.False(Confidence.Create(0.70).IsBelow(0.70));
    }

    [Fact]
    public void It_formats_with_two_decimals_regardless_of_culture()
    {
        Assert.Equal("0.85", Confidence.Create(0.85).ToString());
    }
}

using FluentAssertions;

namespace DepthCharts.Tests;

public class SportDefinitionTests
{
    private static readonly string[] NflPositions = ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"];

    private static readonly string[] MlbPositions = ["SP", "RP", "C", "1B", "2B", "3B", "SS", "LF", "RF", "CF", "DH"];

    [Fact]
    public void Given_nfl_positions_When_a_sport_definition_is_configured_Then_the_name_and_published_order_are_preserved()
    {
        var sport = new SportDefinition("NFL", NflPositions);

        sport.Name.Should().Be("NFL");
        sport.Positions.Should().Equal(NflPositions);
        sport.Should().BeOfType<SportDefinition>();
    }

    [Fact]
    public void Given_mlb_positions_When_a_sport_definition_is_configured_Then_the_name_and_published_order_are_preserved()
    {
        var sport = new SportDefinition("MLB", MlbPositions);

        sport.Name.Should().Be("MLB");
        sport.Positions.Should().Equal(MlbPositions);
        sport.Should().BeOfType<SportDefinition>();
    }

    [Theory]
    [InlineData("QB")]
    [InlineData("WR")]
    [InlineData("RB")]
    [InlineData("TE")]
    [InlineData("K")]
    [InlineData("P")]
    [InlineData("KR")]
    [InlineData("PR")]
    public void Given_an_exact_nfl_position_When_the_definition_is_asked_Then_the_position_is_supported(string position)
    {
        var sport = new SportDefinition("NFL", NflPositions);

        sport.Contains(position).Should().BeTrue();
    }

    [Theory]
    [InlineData("SP")]
    [InlineData("XX")]
    [InlineData("wr")]
    public void Given_a_code_that_is_not_an_exact_nfl_position_When_the_definition_is_asked_Then_the_position_is_not_supported(string position)
    {
        var sport = new SportDefinition("NFL", NflPositions);

        sport.Contains(position).Should().BeFalse();
    }

    [Fact]
    public void Given_a_null_position_When_the_definition_is_asked_Then_the_position_is_not_supported()
    {
        var sport = new SportDefinition("NFL", NflPositions);

        sport.Contains(null).Should().BeFalse();
    }

    [Fact]
    public void Given_a_duplicate_position_code_When_a_sport_definition_is_configured_Then_the_configuration_is_rejected()
    {
        var createSport = () => new SportDefinition("NFL", ["QB", "WR", "QB"]);

        createSport.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void Given_a_sport_that_is_neither_nfl_nor_mlb_When_it_is_configured_Then_it_uses_the_same_definition_model()
    {
        var nfl = new SportDefinition("NFL", NflPositions);
        var mlb = new SportDefinition("MLB", MlbPositions);
        var associationFootball = new SportDefinition("Association Football", ["GK", "ST"]);

        associationFootball.Should().BeOfType<SportDefinition>();
        associationFootball.GetType().Should().Be(nfl.GetType());
        associationFootball.GetType().Should().Be(mlb.GetType());
        associationFootball.Name.Should().Be("Association Football");
        associationFootball.Positions.Should().Equal("GK", "ST");
        associationFootball.Contains("GK").Should().BeTrue();
        associationFootball.Contains("ST").Should().BeTrue();
        associationFootball.Contains("WR").Should().BeFalse();
        associationFootball.Contains("SP").Should().BeFalse();
        nfl.Contains("GK").Should().BeFalse();
        mlb.Contains("GK").Should().BeFalse();
    }

    [Fact]
    public void Given_positions_supplied_to_a_definition_When_the_source_list_changes_Then_the_published_order_stays_unchanged()
    {
        var positions = new List<string> { "GK", "ST" };
        var sport = new SportDefinition("Association Football", positions);

        positions.Add("WR");
        positions[0] = "QB";

        sport.Positions.Should().Equal("GK", "ST");
        sport.Contains("GK").Should().BeTrue();
        sport.Contains("WR").Should().BeFalse();
    }
}
using FluentAssertions;

namespace DepthCharts.Unit.Tests;

public class ValidationTests
{
    [Theory]
    [InlineData("SP")]
    [InlineData("XX")]
    [InlineData("wr")]
    public void Given_an_nfl_chart_When_a_player_is_added_at_an_invalid_position_Then_the_position_is_rejected(string position)
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var bob = new Player(1, "Bob", "WR");
        chart.AddPlayer(bob, "WR", 0);
        var alice = new Player(2, "Alice", "WR");

        var addPlayer = () => chart.AddPlayer(alice, position);

        addPlayer.Should().ThrowExactly<ArgumentException>();
        var wideReceivers = chart.GetFullDepthChart().Should().ContainSingle().Subject;
        wideReceivers.Position.Should().Be("WR");
        wideReceivers.PlayerIds.Should().Equal(bob.PlayerId);
    }

    [Fact]
    public void Given_an_nfl_chart_When_a_player_is_added_at_a_null_position_Then_the_position_is_rejected()
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var bob = new Player(1, "Bob", "WR");
        chart.AddPlayer(bob, "WR", 0);

        var addPlayer = () => chart.AddPlayer(bob, null!);

        addPlayer.Should().ThrowExactly<ArgumentException>();
        var wideReceivers = chart.GetFullDepthChart().Should().ContainSingle().Subject;
        wideReceivers.Position.Should().Be("WR");
        wideReceivers.PlayerIds.Should().Equal(bob.PlayerId);
    }

    [Fact]
    public void Given_an_empty_wr_depth_chart_When_bob_is_added_at_a_negative_depth_Then_the_depth_is_rejected()
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var bob = new Player(1, "Bob", "WR");

        var addPlayer = () => chart.AddPlayer(bob, "WR", -1);

        addPlayer.Should().ThrowExactly<ArgumentOutOfRangeException>();
        chart.GetFullDepthChart().Should().BeEmpty();
    }

    [Fact]
    public void Given_bob_ranked_at_wr_When_bob_is_added_at_a_negative_depth_Then_the_ranking_stays_in_place()
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var bob = new Player(1, "Bob", "WR");
        chart.AddPlayer(bob, "WR", 0);

        var addPlayer = () => chart.AddPlayer(bob, "WR", -1);

        addPlayer.Should().ThrowExactly<ArgumentOutOfRangeException>();
        var wideReceivers = chart.GetFullDepthChart().Should().ContainSingle().Subject;
        wideReceivers.Position.Should().Be("WR");
        wideReceivers.PlayerIds.Should().Equal(bob.PlayerId);
    }

    [Fact]
    public void Given_bob_ranked_at_wr_When_a_null_player_is_added_at_an_unknown_position_Then_the_player_is_rejected_first()
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var bob = new Player(1, "Bob", "WR");
        chart.AddPlayer(bob, "WR", 0);

        var addPlayer = () => chart.AddPlayer(null!, "SP");

        addPlayer.Should().ThrowExactly<ArgumentNullException>();
        var wideReceivers = chart.GetFullDepthChart().Should().ContainSingle().Subject;
        wideReceivers.Position.Should().Be("WR");
        wideReceivers.PlayerIds.Should().Equal(bob.PlayerId);
    }

    [Fact]
    public void Given_bob_ranked_at_wr_When_a_null_player_is_added_at_a_negative_depth_Then_the_player_is_rejected_first()
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var bob = new Player(1, "Bob", "WR");
        chart.AddPlayer(bob, "WR", 0);

        var addPlayer = () => chart.AddPlayer(null!, "WR", -1);

        addPlayer.Should().ThrowExactly<ArgumentNullException>();
        var wideReceivers = chart.GetFullDepthChart().Should().ContainSingle().Subject;
        wideReceivers.Position.Should().Be("WR");
        wideReceivers.PlayerIds.Should().Equal(bob.PlayerId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("   ")]
    public void Given_bob_ranked_at_wr_When_a_player_with_a_blank_name_is_added_Then_the_name_is_rejected(string? name)
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var bob = new Player(1, "Bob", "WR");
        chart.AddPlayer(bob, "WR", 0);

        var addPlayer = () => chart.AddPlayer(new Player(2, name, "WR"), "WR");

        addPlayer.Should().ThrowExactly<ArgumentException>();
        var wideReceivers = chart.GetFullDepthChart().Should().ContainSingle().Subject;
        wideReceivers.Position.Should().Be("WR");
        wideReceivers.PlayerIds.Should().Equal(bob.PlayerId);
    }
}

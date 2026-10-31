using FluentAssertions;

namespace DepthCharts.Tests;

public class AddPlayerTests
{
    [Fact]
    public void Given_an_empty_wr_depth_chart_When_bob_is_added_at_depth_0_Then_wr_contains_bob_at_index_0()
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var bob = new Player(1, "Bob", "WR");

        chart.AddPlayer(bob, "WR", 0);

        var wideReceivers = chart.GetFullDepthChart().Should().ContainSingle().Subject;
        wideReceivers.Position.Should().Be("WR");
        wideReceivers.PlayerIds.Should().Equal(bob.PlayerId);
    }
}

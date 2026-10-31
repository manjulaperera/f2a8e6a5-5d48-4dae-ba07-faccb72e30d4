using FluentAssertions;

namespace DepthCharts.Unit.Tests;

public class FullChartTests
{
    [Fact]
    public void Given_the_published_sample_When_the_full_chart_is_retrieved_Then_wr_precedes_kr_with_the_sample_ids()
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var bob = new Player(1, "Bob", "WR");
        var alice = new Player(2, "Alice", "WR");
        var charlie = new Player(3, "Charlie", "WR");
        chart.AddPlayer(bob, "WR", 0);
        chart.AddPlayer(alice, "WR", 0);
        chart.AddPlayer(charlie, "WR", 2);
        chart.AddPlayer(bob, "KR");

        var rows = chart.GetFullDepthChart();

        rows.Select(row => row.Position).Should().Equal("WR", "KR");
        rows[0].PlayerIds.Should().Equal(alice.PlayerId, bob.PlayerId, charlie.PlayerId);
        rows[1].PlayerIds.Should().Equal(bob.PlayerId);
    }

    [Fact]
    public void Given_kr_added_before_wr_When_the_full_chart_is_retrieved_Then_wr_precedes_kr()
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var bob = new Player(1, "Bob", "WR");
        var alice = new Player(2, "Alice", "WR");
        chart.AddPlayer(bob, "KR");
        chart.AddPlayer(alice, "WR", 0);

        var rows = chart.GetFullDepthChart();

        rows.Select(row => row.Position).Should().Equal("WR", "KR");
        rows[0].PlayerIds.Should().Equal(alice.PlayerId);
        rows[1].PlayerIds.Should().Equal(bob.PlayerId);
    }

    [Fact]
    public void Given_only_wr_and_kr_are_filled_When_the_full_chart_is_retrieved_Then_empty_nfl_positions_are_omitted()
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var bob = new Player(1, "Bob", "WR");
        var alice = new Player(2, "Alice", "WR");
        chart.AddPlayer(bob, "KR");
        chart.AddPlayer(alice, "WR", 0);

        var rows = chart.GetFullDepthChart();

        rows.Select(row => row.Position).Should().Equal("WR", "KR");
        rows.Select(row => row.Position).Should().NotContain(["QB", "RB", "TE", "K", "P", "PR"]);
    }

    [Fact]
    public void Given_a_chart_with_no_players_When_the_full_chart_is_retrieved_Then_the_result_is_empty()
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);

        chart.GetFullDepthChart().Should().BeEmpty();
    }
}

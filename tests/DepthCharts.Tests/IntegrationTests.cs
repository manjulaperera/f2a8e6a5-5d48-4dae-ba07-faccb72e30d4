using FluentAssertions;

namespace DepthCharts.Tests;

public class IntegrationTests
{
    [Fact]
    public void Given_the_published_challenge_adds_When_the_chart_is_read_Then_wr_is_2_1_3_kr_is_1_and_players_below_alice_are_1_then_3()
    {
        var chart = new DepthChartService(SportDefinition.Nfl);
        var bob = new Player(1, "Bob", "WR");
        var alice = new Player(2, "Alice", "WR");
        var charlie = new Player(3, "Charlie", "WR");

        chart.AddPlayer(bob, "WR", 0);

        var afterBob = chart.GetFullDepthChart().Should().ContainSingle().Subject;
        afterBob.Position.Should().Be("WR");
        afterBob.PlayerIds.Should().Equal(bob.PlayerId);

        chart.AddPlayer(alice, "WR", 0);

        var afterAlice = chart.GetFullDepthChart().Should().ContainSingle().Subject;
        afterAlice.Position.Should().Be("WR");
        afterAlice.PlayerIds.Should().Equal(alice.PlayerId, bob.PlayerId);

        chart.AddPlayer(charlie, "WR", 2);

        var afterCharlie = chart.GetFullDepthChart().Should().ContainSingle().Subject;
        afterCharlie.Position.Should().Be("WR");
        afterCharlie.PlayerIds.Should().Equal(alice.PlayerId, bob.PlayerId, charlie.PlayerId);

        chart.AddPlayer(bob, "KR");

        var rows = chart.GetFullDepthChart();
        rows.Select(row => row.Position).Should().Equal("WR", "KR");
        rows[0].PlayerIds.Should().Equal(2, 1, 3);
        rows[1].PlayerIds.Should().Equal(1);
        chart.GetPlayersBelow(alice, "WR").Should().Equal(bob.PlayerId, charlie.PlayerId);
    }

    [Fact]
    public void Given_separate_nfl_and_mlb_charts_When_the_same_player_is_removed_from_nfl_Then_mlb_is_unchanged()
    {
        var nfl = new DepthChartService(SportDefinition.Nfl);
        var mlb = new DepthChartService(SportDefinition.Mlb);
        var bob = new Player(1, "Bob", "WR");
        var alice = new Player(2, "Alice", "C");
        nfl.AddPlayer(bob, "WR", 0);
        mlb.AddPlayer(bob, "CF", 0);
        mlb.AddPlayer(alice, "C", 0);

        nfl.RemovePlayer(bob, "WR");

        nfl.GetFullDepthChart().Should().BeEmpty();
        var rows = mlb.GetFullDepthChart();
        rows.Select(row => row.Position).Should().Equal("C", "CF");
        rows[0].PlayerIds.Should().Equal(alice.PlayerId);
        rows[1].PlayerIds.Should().Equal(bob.PlayerId);
    }

    [Fact]
    public void Given_bob_ranked_at_wr_and_kr_When_bob_is_removed_from_wr_Then_only_wr_changes()
    {
        var chart = new DepthChartService(SportDefinition.Nfl);
        var bob = new Player(1, "Bob", "WR");
        var alice = new Player(2, "Alice", "WR");
        var charlie = new Player(3, "Charlie", "WR");
        chart.AddPlayer(bob, "WR", 0);
        chart.AddPlayer(alice, "WR", 0);
        chart.AddPlayer(charlie, "WR", 2);
        chart.AddPlayer(bob, "KR");

        chart.RemovePlayer(bob, "WR");

        var rows = chart.GetFullDepthChart();
        rows.Select(row => row.Position).Should().Equal("WR", "KR");
        rows[0].PlayerIds.Should().Equal(alice.PlayerId, charlie.PlayerId);
        rows[1].PlayerIds.Should().Equal(bob.PlayerId);
        chart.GetPlayersBelow(alice, "WR").Should().Equal(charlie.PlayerId);
        chart.GetPlayersBelow(bob, "KR").Should().BeEmpty();
    }
}

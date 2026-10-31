using FluentAssertions;

namespace DepthCharts.Tests;

public class RemovePlayerTests
{
    [Fact]
    public void Given_wr_ranked_alice_dana_bob_charlie_When_bob_is_removed_Then_the_gap_closes()
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var alice = new Player(2, "Alice", "WR");
        var dana = new Player(4, "Dana", "WR");
        var bob = new Player(1, "Bob", "WR");
        var charlie = new Player(3, "Charlie", "WR");
        chart.AddPlayer(alice, "WR", 0);
        chart.AddPlayer(dana, "WR");
        chart.AddPlayer(bob, "WR");
        chart.AddPlayer(charlie, "WR");

        chart.RemovePlayer(bob, "WR");

        var wideReceivers = chart.GetFullDepthChart().Should().ContainSingle().Subject;
        wideReceivers.Position.Should().Be("WR");
        wideReceivers.PlayerIds.Should().Equal(alice.PlayerId, dana.PlayerId, charlie.PlayerId);
    }

    [Fact]
    public void Given_wr_ranked_alice_bob_charlie_When_alice_is_removed_Then_bob_and_charlie_remain()
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var alice = new Player(2, "Alice", "WR");
        var bob = new Player(1, "Bob", "WR");
        var charlie = new Player(3, "Charlie", "WR");
        chart.AddPlayer(alice, "WR", 0);
        chart.AddPlayer(bob, "WR");
        chart.AddPlayer(charlie, "WR");

        chart.RemovePlayer(alice, "WR");

        var wideReceivers = chart.GetFullDepthChart().Should().ContainSingle().Subject;
        wideReceivers.Position.Should().Be("WR");
        wideReceivers.PlayerIds.Should().Equal(bob.PlayerId, charlie.PlayerId);
    }

    [Fact]
    public void Given_wr_ranked_alice_bob_charlie_When_charlie_is_removed_Then_alice_and_bob_remain()
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var alice = new Player(2, "Alice", "WR");
        var bob = new Player(1, "Bob", "WR");
        var charlie = new Player(3, "Charlie", "WR");
        chart.AddPlayer(alice, "WR", 0);
        chart.AddPlayer(bob, "WR");
        chart.AddPlayer(charlie, "WR");

        chart.RemovePlayer(charlie, "WR");

        var wideReceivers = chart.GetFullDepthChart().Should().ContainSingle().Subject;
        wideReceivers.Position.Should().Be("WR");
        wideReceivers.PlayerIds.Should().Equal(alice.PlayerId, bob.PlayerId);
    }

    [Fact]
    public void Given_bob_ranked_at_wr_and_kr_When_bob_is_removed_from_wr_Then_bob_remains_at_kr()
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var alice = new Player(2, "Alice", "WR");
        var bob = new Player(1, "Bob", "WR");
        chart.AddPlayer(alice, "WR", 0);
        chart.AddPlayer(bob, "WR");
        chart.AddPlayer(bob, "KR");

        chart.RemovePlayer(bob, "WR");

        var rows = chart.GetFullDepthChart();
        rows.Should().HaveCount(2);
        rows[0].Position.Should().Be("WR");
        rows[0].PlayerIds.Should().Equal(alice.PlayerId);
        rows[1].Position.Should().Be("KR");
        rows[1].PlayerIds.Should().Equal(bob.PlayerId);
    }

    [Fact]
    public void Given_kr_ranked_bob_only_When_bob_is_removed_Then_kr_has_no_players()
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var alice = new Player(2, "Alice", "WR");
        var bob = new Player(1, "Bob", "WR");
        chart.AddPlayer(alice, "WR", 0);
        chart.AddPlayer(bob, "KR");

        chart.RemovePlayer(bob, "KR");

        var rows = chart.GetFullDepthChart();
        rows.Should().ContainSingle();
        rows[0].Position.Should().Be("WR");
        rows[0].PlayerIds.Should().Equal(alice.PlayerId);
    }
}

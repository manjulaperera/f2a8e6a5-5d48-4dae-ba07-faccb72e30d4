using FluentAssertions;

namespace DepthCharts.Tests;

public class PlayersBelowTests
{
    [Fact]
    public void Given_wr_ranked_alice_bob_charlie_When_players_below_alice_are_retrieved_Then_the_result_is_bob_then_charlie()
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var alice = new Player(2, "Alice", "WR");
        var bob = new Player(1, "Bob", "WR");
        var charlie = new Player(3, "Charlie", "WR");
        chart.AddPlayer(alice, "WR", 0);
        chart.AddPlayer(bob, "WR");
        chart.AddPlayer(charlie, "WR");

        chart.GetPlayersBelow(alice, "WR").Should().Equal(bob.PlayerId, charlie.PlayerId);
    }

    [Fact]
    public void Given_wr_ranked_alice_bob_charlie_When_players_below_bob_are_retrieved_Then_the_result_is_charlie()
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var alice = new Player(2, "Alice", "WR");
        var bob = new Player(1, "Bob", "WR");
        var charlie = new Player(3, "Charlie", "WR");
        chart.AddPlayer(alice, "WR", 0);
        chart.AddPlayer(bob, "WR");
        chart.AddPlayer(charlie, "WR");

        chart.GetPlayersBelow(bob, "WR").Should().Equal(charlie.PlayerId);
    }

    [Fact]
    public void Given_wr_ranked_alice_bob_charlie_When_players_below_charlie_are_retrieved_Then_the_result_is_empty()
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var alice = new Player(2, "Alice", "WR");
        var bob = new Player(1, "Bob", "WR");
        var charlie = new Player(3, "Charlie", "WR");
        chart.AddPlayer(alice, "WR", 0);
        chart.AddPlayer(bob, "WR");
        chart.AddPlayer(charlie, "WR");

        chart.GetPlayersBelow(charlie, "WR").Should().BeEmpty();
    }

    [Fact]
    public void Given_a_player_below_bob_only_at_kr_When_players_below_bob_at_wr_are_retrieved_Then_that_player_is_excluded()
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var alice = new Player(2, "Alice", "WR");
        var bob = new Player(1, "Bob", "WR");
        var charlie = new Player(3, "Charlie", "WR");
        var dana = new Player(4, "Dana", "WR");
        chart.AddPlayer(alice, "WR", 0);
        chart.AddPlayer(bob, "WR");
        chart.AddPlayer(charlie, "WR");
        chart.AddPlayer(bob, "KR");
        chart.AddPlayer(dana, "KR");

        chart.GetPlayersBelow(bob, "WR").Should().Equal(charlie.PlayerId);
    }

    [Fact]
    public void Given_alice_is_not_ranked_at_wr_When_players_below_alice_are_retrieved_Then_the_lookup_is_rejected()
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var alice = new Player(2, "Alice", "WR");
        var bob = new Player(1, "Bob", "WR");
        chart.AddPlayer(bob, "WR", 0);

        var playersBelow = () => chart.GetPlayersBelow(alice, "WR");

        playersBelow.Should().ThrowExactly<ArgumentException>();
        var wideReceivers = chart.GetFullDepthChart().Should().ContainSingle().Subject;
        wideReceivers.Position.Should().Be("WR");
        wideReceivers.PlayerIds.Should().Equal(bob.PlayerId);
    }

    [Fact]
    public void Given_bob_ranked_ahead_of_charlie_When_players_below_another_record_with_bobs_id_are_retrieved_Then_the_result_is_charlie()
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var bob = new Player(1, "Bob", "WR");
        var charlie = new Player(3, "Charlie", "WR");
        chart.AddPlayer(bob, "WR", 0);
        chart.AddPlayer(charlie, "WR");

        chart.GetPlayersBelow(new Player(bob.PlayerId, "Robert", "QB"), "WR").Should().Equal(charlie.PlayerId);
    }

    [Fact]
    public void Given_alice_ranked_at_wr_When_players_below_another_alex_with_a_different_id_are_retrieved_Then_the_lookup_is_rejected()
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var alice = new Player(2, "Alex", "WR");
        chart.AddPlayer(alice, "WR", 0);

        var playersBelow = () => chart.GetPlayersBelow(new Player(4, "Alex", "WR"), "WR");

        playersBelow.Should().ThrowExactly<ArgumentException>();
        var wideReceivers = chart.GetFullDepthChart().Should().ContainSingle().Subject;
        wideReceivers.Position.Should().Be("WR");
        wideReceivers.PlayerIds.Should().Equal(alice.PlayerId);
    }
}

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

    [Fact]
    public void Given_an_empty_wr_depth_chart_When_bob_is_added_with_depth_omitted_Then_wr_contains_bob_at_index_0()
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var bob = new Player(1, "Bob", "WR");

        chart.AddPlayer(bob, "WR");

        var wideReceivers = chart.GetFullDepthChart().Should().ContainSingle().Subject;
        wideReceivers.Position.Should().Be("WR");
        wideReceivers.PlayerIds.Should().Equal(bob.PlayerId);
    }

    [Fact]
    public void Given_wr_ranked_alice_then_bob_When_charlie_is_added_with_depth_omitted_Then_charlie_is_appended()
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var alice = new Player(2, "Alice", "WR");
        var bob = new Player(1, "Bob", "WR");
        var charlie = new Player(3, "Charlie", "WR");
        chart.AddPlayer(alice, "WR", 0);
        chart.AddPlayer(bob, "WR", 1);

        chart.AddPlayer(charlie, "WR");

        var wideReceivers = chart.GetFullDepthChart().Should().ContainSingle().Subject;
        wideReceivers.Position.Should().Be("WR");
        wideReceivers.PlayerIds.Should().Equal(alice.PlayerId, bob.PlayerId, charlie.PlayerId);
    }

    [Fact]
    public void Given_bob_at_wr_depth_0_When_alice_is_added_at_depth_0_Then_alice_is_first_and_bob_shifts_down()
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var bob = new Player(1, "Bob", "WR");
        var alice = new Player(2, "Alice", "WR");
        chart.AddPlayer(bob, "WR", 0);

        chart.AddPlayer(alice, "WR", 0);

        var wideReceivers = chart.GetFullDepthChart().Should().ContainSingle().Subject;
        wideReceivers.Position.Should().Be("WR");
        wideReceivers.PlayerIds.Should().Equal(alice.PlayerId, bob.PlayerId);
    }

    [Fact]
    public void Given_wr_ranked_alice_bob_charlie_When_dana_is_added_at_depth_1_Then_bob_and_charlie_shift_down()
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

        chart.AddPlayer(dana, "WR", 1);

        var wideReceivers = chart.GetFullDepthChart().Should().ContainSingle().Subject;
        wideReceivers.Position.Should().Be("WR");
        wideReceivers.PlayerIds.Should().Equal(alice.PlayerId, dana.PlayerId, bob.PlayerId, charlie.PlayerId);
    }

    [Fact]
    public void Given_wr_ranked_alice_bob_charlie_When_dana_is_added_at_depth_10_Then_dana_is_appended()
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

        chart.AddPlayer(dana, "WR", 10);

        var wideReceivers = chart.GetFullDepthChart().Should().ContainSingle().Subject;
        wideReceivers.Position.Should().Be("WR");
        wideReceivers.PlayerIds.Should().Equal(alice.PlayerId, bob.PlayerId, charlie.PlayerId, dana.PlayerId);
    }

    [Fact]
    public void Given_an_empty_wr_depth_chart_When_bob_is_added_at_depth_5_Then_bob_is_at_index_0_with_no_gap()
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var bob = new Player(1, "Bob", "WR");

        chart.AddPlayer(bob, "WR", 5);

        var wideReceivers = chart.GetFullDepthChart().Should().ContainSingle().Subject;
        wideReceivers.Position.Should().Be("WR");
        wideReceivers.PlayerIds.Should().Equal(bob.PlayerId);
    }

    [Fact]
    public void Given_wr_ranked_alice_bob_charlie_When_bob_is_added_at_depth_0_Then_bob_moves_to_the_front_once()
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var alice = new Player(2, "Alice", "WR");
        var bob = new Player(1, "Bob", "WR");
        var charlie = new Player(3, "Charlie", "WR");
        chart.AddPlayer(alice, "WR", 0);
        chart.AddPlayer(bob, "WR");
        chart.AddPlayer(charlie, "WR");

        chart.AddPlayer(new Player(bob.PlayerId, "Robert", "QB"), "WR", 0);

        var wideReceivers = chart.GetFullDepthChart().Should().ContainSingle().Subject;
        wideReceivers.Position.Should().Be("WR");
        wideReceivers.PlayerIds.Should().Equal(bob.PlayerId, alice.PlayerId, charlie.PlayerId);
    }

    [Fact]
    public void Given_wr_ranked_alice_bob_charlie_dana_When_dana_is_added_at_depth_1_Then_dana_moves_once()
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
        chart.AddPlayer(dana, "WR");

        chart.AddPlayer(dana, "WR", 1);

        var wideReceivers = chart.GetFullDepthChart().Should().ContainSingle().Subject;
        wideReceivers.Position.Should().Be("WR");
        wideReceivers.PlayerIds.Should().Equal(alice.PlayerId, dana.PlayerId, bob.PlayerId, charlie.PlayerId);
    }

    [Fact]
    public void Given_wr_ranked_alice_bob_charlie_When_alice_is_added_with_depth_omitted_Then_alice_moves_to_the_end_once()
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var alice = new Player(2, "Alice", "WR");
        var bob = new Player(1, "Bob", "WR");
        var charlie = new Player(3, "Charlie", "WR");
        chart.AddPlayer(alice, "WR", 0);
        chart.AddPlayer(bob, "WR");
        chart.AddPlayer(charlie, "WR");

        chart.AddPlayer(alice, "WR");

        var wideReceivers = chart.GetFullDepthChart().Should().ContainSingle().Subject;
        wideReceivers.Position.Should().Be("WR");
        wideReceivers.PlayerIds.Should().Equal(bob.PlayerId, charlie.PlayerId, alice.PlayerId);
    }

    [Fact]
    public void Given_wr_ranked_alice_bob_charlie_When_alice_is_added_at_depth_10_Then_alice_moves_to_the_end_once()
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var alice = new Player(2, "Alice", "WR");
        var bob = new Player(1, "Bob", "WR");
        var charlie = new Player(3, "Charlie", "WR");
        chart.AddPlayer(alice, "WR", 0);
        chart.AddPlayer(bob, "WR");
        chart.AddPlayer(charlie, "WR");

        chart.AddPlayer(alice, "WR", 10);

        var wideReceivers = chart.GetFullDepthChart().Should().ContainSingle().Subject;
        wideReceivers.Position.Should().Be("WR");
        wideReceivers.PlayerIds.Should().Equal(bob.PlayerId, charlie.PlayerId, alice.PlayerId);
    }

    [Fact]
    public void Given_bob_already_at_wr_depth_1_When_bob_is_added_at_depth_1_Then_the_ranking_is_unchanged()
    {
        var sport = new SportDefinition("NFL", ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);
        var chart = new DepthChartService(sport);
        var alice = new Player(2, "Alice", "WR");
        var bob = new Player(1, "Bob", "WR");
        var charlie = new Player(3, "Charlie", "WR");
        chart.AddPlayer(alice, "WR", 0);
        chart.AddPlayer(bob, "WR");
        chart.AddPlayer(charlie, "WR");

        chart.AddPlayer(bob, "WR", 1);

        var wideReceivers = chart.GetFullDepthChart().Should().ContainSingle().Subject;
        wideReceivers.Position.Should().Be("WR");
        wideReceivers.PlayerIds.Should().Equal(alice.PlayerId, bob.PlayerId, charlie.PlayerId);
    }
}

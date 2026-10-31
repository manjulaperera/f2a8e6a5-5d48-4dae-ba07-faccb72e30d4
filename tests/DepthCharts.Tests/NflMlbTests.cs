using FluentAssertions;

namespace DepthCharts.Tests;

public class NflMlbTests
{
    [Fact]
    public void Given_the_nfl_definition_When_its_positions_are_read_Then_they_match_the_published_list()
    {
        SportDefinition.Nfl.Name.Should().Be("NFL");
        SportDefinition.Nfl.Positions.Should().Equal("QB", "WR", "RB", "TE", "K", "P", "KR", "PR");
    }

    [Fact]
    public void Given_the_mlb_definition_When_its_positions_are_read_Then_they_match_the_published_list()
    {
        SportDefinition.Mlb.Name.Should().Be("MLB");
        SportDefinition.Mlb.Positions.Should().Equal("SP", "RP", "C", "1B", "2B", "3B", "SS", "LF", "RF", "CF", "DH");
    }

    [Fact]
    public void Given_an_nfl_chart_When_one_player_is_ranked_at_every_position_Then_the_published_order_is_preserved()
    {
        var chart = new DepthChartService(SportDefinition.Nfl);
        var bob = new Player(1, "Bob", "WR");
        foreach (var position in SportDefinition.Nfl.Positions.Reverse())
        {
            chart.AddPlayer(bob, position);
        }

        chart.GetFullDepthChart().Select(row => row.Position).Should().Equal(SportDefinition.Nfl.Positions);
        chart.GetFullDepthChart().Should().OnlyContain(row => row.PlayerIds.Single() == bob.PlayerId);
    }

    [Fact]
    public void Given_an_mlb_chart_When_one_player_is_ranked_at_every_position_Then_the_published_order_is_preserved()
    {
        var chart = new DepthChartService(SportDefinition.Mlb);
        var bob = new Player(1, "Bob", "SP");
        foreach (var position in SportDefinition.Mlb.Positions.Reverse())
        {
            chart.AddPlayer(bob, position);
        }

        chart.GetFullDepthChart().Select(row => row.Position).Should().Equal(SportDefinition.Mlb.Positions);
        chart.GetFullDepthChart().Should().OnlyContain(row => row.PlayerIds.Single() == bob.PlayerId);
    }

    [Fact]
    public void Given_an_nfl_chart_When_sp_is_added_Then_the_position_is_rejected()
    {
        var chart = new DepthChartService(SportDefinition.Nfl);
        var bob = new Player(1, "Bob", "WR");

        var addPlayer = () => chart.AddPlayer(bob, "SP");

        addPlayer.Should().ThrowExactly<ArgumentException>();
        chart.GetFullDepthChart().Should().BeEmpty();
    }

    [Fact]
    public void Given_an_mlb_chart_When_cf_is_added_before_c_Then_the_full_chart_lists_c_then_cf()
    {
        var chart = new DepthChartService(SportDefinition.Mlb);
        var bob = new Player(1, "Bob", "CF");
        var alice = new Player(2, "Alice", "C");
        chart.AddPlayer(bob, "CF");
        chart.AddPlayer(alice, "C");

        var rows = chart.GetFullDepthChart();

        rows.Select(row => row.Position).Should().Equal("C", "CF");
        rows[0].PlayerIds.Should().Equal(alice.PlayerId);
        rows[1].PlayerIds.Should().Equal(bob.PlayerId);
    }

    [Fact]
    public void Given_the_same_player_on_separate_nfl_and_mlb_charts_When_the_player_is_removed_from_nfl_Then_mlb_is_unchanged()
    {
        var nfl = new DepthChartService(SportDefinition.Nfl);
        var mlb = new DepthChartService(SportDefinition.Mlb);
        var bob = new Player(1, "Bob", "WR");
        nfl.AddPlayer(bob, "WR", 0);
        mlb.AddPlayer(bob, "CF", 0);

        nfl.RemovePlayer(bob, "WR");

        nfl.GetFullDepthChart().Should().BeEmpty();
        var centerField = mlb.GetFullDepthChart().Should().ContainSingle().Subject;
        centerField.Position.Should().Be("CF");
        centerField.PlayerIds.Should().Equal(bob.PlayerId);
    }

    [Fact]
    public void Given_a_sport_configured_with_gk_and_st_When_players_are_ranked_Then_the_same_service_applies_the_depth_rules()
    {
        var sport = new SportDefinition("Association Football", ["GK", "ST"]);
        var chart = new DepthChartService(sport);
        var bob = new Player(1, "Bob", null);
        var alice = new Player(2, "Alice", null);
        var charlie = new Player(3, "Charlie", null);
        chart.AddPlayer(bob, "GK", 0);
        chart.AddPlayer(alice, "GK", 0);
        chart.AddPlayer(charlie, "GK");
        chart.AddPlayer(bob, "ST");

        var addWideReceiver = () => chart.AddPlayer(alice, "WR");

        addWideReceiver.Should().ThrowExactly<ArgumentException>();
        chart.Should().BeOfType<DepthChartService>();
        var rows = chart.GetFullDepthChart();
        rows.Select(row => row.Position).Should().Equal("GK", "ST");
        rows[0].PlayerIds.Should().Equal(alice.PlayerId, bob.PlayerId, charlie.PlayerId);
        rows[1].PlayerIds.Should().Equal(bob.PlayerId);
    }
}

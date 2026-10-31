using FluentAssertions;

namespace DepthCharts.Tests;

public class PlayerTests
{
    [Fact]
    public void Given_an_integer_id_When_a_player_is_created_Then_the_player_id_is_exposed()
    {
        const int playerId = 7;

        var player = new Player(playerId, "Bob", "WR");

        player.PlayerId.Should().Be(playerId);
    }

    [Fact]
    public void Given_player_id_zero_When_a_player_is_created_Then_the_identifier_is_accepted()
    {
        var player = new Player(0, "Bob", "WR");

        player.PlayerId.Should().Be(0);
    }

    [Fact]
    public void Given_a_name_When_a_player_is_created_Then_the_name_is_exposed()
    {
        const string name = "Bob";

        var player = new Player(1, name, "WR");

        player.Name.Should().Be(name);
    }

    [Fact]
    public void Given_a_natural_position_When_a_player_is_created_Then_the_natural_position_is_exposed()
    {
        const string naturalPosition = "WR";

        var player = new Player(1, "Bob", naturalPosition);

        player.NaturalPosition.Should().Be(naturalPosition);
    }

    [Fact]
    public void Given_no_natural_position_When_a_player_is_created_Then_the_natural_position_is_absent()
    {
        var player = new Player(1, "Bob", null);

        player.NaturalPosition.Should().BeNull();
    }

    [Fact]
    public void Given_two_instances_with_the_same_player_id_When_they_are_compared_Then_they_are_the_same_player()
    {
        var first = new Player(1, "Bob", "WR");
        var second = new Player(1, "Robert", "QB");

        first.Should().Be(second);
        first.GetHashCode().Should().Be(second.GetHashCode());
        ReferenceEquals(first, second).Should().BeFalse();
    }

    [Fact]
    public void Given_two_instances_with_different_player_ids_When_their_names_match_Then_they_are_different_players()
    {
        var first = new Player(1, "Alex", "WR");
        var second = new Player(4, "Alex", "WR");

        first.Should().NotBe(second);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Given_a_negative_player_id_When_a_player_is_created_Then_the_identifier_is_rejected(int playerId)
    {
        var createPlayer = () => new Player(playerId, "Bob", "WR");

        createPlayer.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("   ")]
    public void Given_an_empty_or_whitespace_name_When_a_player_is_created_Then_the_name_is_rejected(string? name)
    {
        var createPlayer = () => new Player(1, name!, "WR");

        createPlayer.Should().ThrowExactly<ArgumentException>();
    }
}

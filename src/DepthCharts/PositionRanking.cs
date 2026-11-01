namespace DepthCharts;

/// <summary>
/// The ordered players at one position. Index 0 is depth 0.
/// </summary>
internal sealed class PositionRanking
{
    private readonly List<Player> _players = [];

    /// <summary>
    /// Gets the number of players at this position.
    /// </summary>
    public int Count => _players.Count;

    /// <summary>
    /// Places the player at a depth. A player already in this ranking is moved.
    /// A null depth, or a depth at or past the end, adds the player last.
    /// </summary>
    /// <param name="player">The player to place. Matching uses <see cref="Player.PlayerId"/>.</param>
    /// <param name="depth">Zero-based depth. Null adds the player at the end.</param>
    public void Place(Player player, int? depth)
    {
        _players.Remove(player);
        _players.Insert(IndexFor(depth), player);
    }

    /// <summary>
    /// Removes the player and closes the gap.
    /// </summary>
    /// <param name="player">The player to remove. Matching uses <see cref="Player.PlayerId"/>.</param>
    /// <returns>True when a matching player was removed. False when this ranking does not contain that id.</returns>
    public bool Remove(Player player) => _players.Remove(player);

    /// <summary>
    /// Returns the player ids from depth 0 downward.
    /// </summary>
    /// <returns>A copy of the ids. The ranking itself is unchanged.</returns>
    public IReadOnlyList<int> PlayerIds() => _players.ConvertAll(player => player.PlayerId);

    /// <summary>
    /// Returns the player ids ranked strictly below the player.
    /// </summary>
    /// <param name="player">The player to look up. Matching uses <see cref="Player.PlayerId"/>.</param>
    /// <param name="playerIds">The ids below that player, or an empty list when the player is absent.</param>
    /// <returns>True when the player is in this ranking. The last player returns true and an empty list.</returns>
    public bool TryPlayerIdsBelow(Player player, out IReadOnlyList<int> playerIds)
    {
        var index = _players.FindIndex(rankedPlayer => rankedPlayer.PlayerId == player.PlayerId);
        if (index < 0)
        {
            playerIds = [];
            return false;
        }

        playerIds = _players.Skip(index + 1).Select(rankedPlayer => rankedPlayer.PlayerId).ToList();
        return true;
    }

    private int IndexFor(int? depth) =>
        depth is int requested && requested < _players.Count
            ? requested
            : _players.Count;
}

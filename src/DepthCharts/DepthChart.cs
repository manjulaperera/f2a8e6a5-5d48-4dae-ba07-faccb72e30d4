namespace DepthCharts;

/// <summary>
/// One sport's rankings. This type owns insert, move, remove, and chart order.
/// </summary>
internal sealed class DepthChart
{
    private const string PlayerNotRanked = "Player is not ranked at this position.";

    private readonly SportDefinition _sport;
    private readonly Dictionary<string, PositionRanking> _rankings = new();

    /// <summary>
    /// Creates an empty chart for the given sport.
    /// </summary>
    /// <param name="sport">The sport whose position codes and order this chart uses.</param>
    public DepthChart(SportDefinition sport)
    {
        _sport = sport;
    }

    /// <summary>
    /// Adds a player at a depth, or at the end when <paramref name="depth"/> is omitted.
    /// A player already at the position is moved. An invalid call leaves the chart unchanged.
    /// </summary>
    /// <param name="player">The player to rank.</param>
    /// <param name="position">A position code from this chart's sport.</param>
    /// <param name="depth">Zero-based depth. Null adds the player at the end.</param>
    public void AddPlayer(Player player, string position, int? depth)
    {
        RequirePlayer(player);
        RequireSupportedPosition(position);
        RequireNonNegativeDepth(depth);

        RankingFor(position).Place(player, depth);
    }

    /// <summary>
    /// Removes the player from one position and closes the gap.
    /// Removing the last player drops that position from the chart.
    /// </summary>
    /// <param name="player">The player to remove.</param>
    /// <param name="position">A position code from this chart's sport.</param>
    public void RemovePlayer(Player player, string position)
    {
        RequirePlayer(player);
        RequireSupportedPosition(position);

        if (!_rankings.TryGetValue(position, out var ranking) || !ranking.Remove(player))
        {
            throw new ArgumentException(PlayerNotRanked, nameof(player));
        }

        if (ranking.Count == 0)
        {
            _rankings.Remove(position);
        }
    }

    /// <summary>
    /// Returns the populated positions in the sport's published order.
    /// Each row lists player ids from depth 0 downward.
    /// </summary>
    /// <returns>A copy of the populated rows. An empty chart returns an empty list.</returns>
    public IReadOnlyList<DepthChartRow> GetFullDepthChart()
    {
        var rows = new List<DepthChartRow>();
        foreach (var position in _sport.Positions)
        {
            if (!_rankings.TryGetValue(position, out var ranking) || ranking.Count == 0)
            {
                continue;
            }

            rows.Add(new DepthChartRow(position, ranking.PlayerIds()));
        }

        return rows;
    }

    /// <summary>
    /// Returns the player ids ranked strictly below the player at one position.
    /// </summary>
    /// <param name="player">The player to look up.</param>
    /// <param name="position">A position code from this chart's sport.</param>
    /// <returns>A copy of the ids below that player. The last player returns an empty list.</returns>
    public IReadOnlyList<int> GetPlayersBelow(Player player, string position)
    {
        RequirePlayer(player);
        RequireSupportedPosition(position);

        if (!_rankings.TryGetValue(position, out var ranking) || !ranking.TryPlayerIdsBelow(player, out var playerIds))
        {
            throw new ArgumentException(PlayerNotRanked, nameof(player));
        }

        return playerIds;
    }

    private PositionRanking RankingFor(string position)
    {
        if (!_rankings.TryGetValue(position, out var ranking))
        {
            ranking = new PositionRanking();
            _rankings[position] = ranking;
        }

        return ranking;
    }

    private static void RequirePlayer(Player player)
    {
        if (player is null)
        {
            throw new ArgumentNullException(nameof(player));
        }

        if (string.IsNullOrWhiteSpace(player.Name))
        {
            throw new ArgumentException("Name is required.", nameof(player));
        }
    }

    private void RequireSupportedPosition(string position)
    {
        if (position is null || !_sport.Contains(position))
        {
            throw new ArgumentException("Position is not supported.", nameof(position));
        }
    }

    private static void RequireNonNegativeDepth(int? depth)
    {
        if (depth < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(depth));
        }
    }
}

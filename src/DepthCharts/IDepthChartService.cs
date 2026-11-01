namespace DepthCharts;

/// <summary>
/// The four depth-chart operations for one sport.
/// </summary>
public interface IDepthChartService
{
    /// <summary>
    /// Adds a player at a depth, or at the end when <paramref name="depth"/> is omitted.
    /// A player already at the position is moved.
    /// </summary>
    /// <param name="player">The player to rank.</param>
    /// <param name="position">A position code from this chart's sport.</param>
    /// <param name="depth">Zero-based depth. Null adds the player at the end.</param>
    void AddPlayer(Player player, string position, int? depth = null);

    /// <summary>
    /// Removes the player from one position and closes the gap.
    /// </summary>
    /// <param name="player">The player to remove.</param>
    /// <param name="position">A position code from this chart's sport.</param>
    void RemovePlayer(Player player, string position);

    /// <summary>
    /// Returns the populated positions in the sport's published order.
    /// Each row lists player ids from depth 0 downward.
    /// </summary>
    /// <returns>A copy of the populated rows. An empty chart returns an empty list.</returns>
    IReadOnlyList<DepthChartRow> GetFullDepthChart();

    /// <summary>
    /// Returns the player ids ranked strictly below the player at one position.
    /// </summary>
    /// <param name="player">The player to look up.</param>
    /// <param name="position">A position code from this chart's sport.</param>
    /// <returns>A copy of the ids below that player. The last player returns an empty list.</returns>
    IReadOnlyList<int> GetPlayersBelow(Player player, string position);
}

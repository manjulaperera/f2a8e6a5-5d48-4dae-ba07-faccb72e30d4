namespace DepthCharts;

/// <summary>
/// Ranks players for one sport. Each instance is an independent chart.
/// </summary>
public sealed class DepthChartService : IDepthChartService
{
    private readonly DepthChart _chart;

    /// <summary>
    /// Creates an empty chart for the given sport.
    /// </summary>
    /// <param name="sport">The sport whose position codes and order this chart uses.</param>
    public DepthChartService(SportDefinition sport)
    {
        _chart = new DepthChart(sport);
    }

    /// <summary>
    /// Adds a player at a depth, or at the end when <paramref name="depth"/> is omitted.
    /// A player already at the position is moved.
    /// </summary>
    /// <param name="player">The player to rank.</param>
    /// <param name="position">A position code from this chart's sport.</param>
    /// <param name="depth">Zero-based depth. Null adds the player at the end.</param>
    public void AddPlayer(Player player, string position, int? depth = null) =>
        _chart.AddPlayer(player, position, depth);

    /// <summary>
    /// Removes the player from one position and closes the gap.
    /// </summary>
    /// <param name="player">The player to remove.</param>
    /// <param name="position">A position code from this chart's sport.</param>
    public void RemovePlayer(Player player, string position) =>
        _chart.RemovePlayer(player, position);

    /// <summary>
    /// Returns the populated positions in the sport's published order.
    /// Each row lists player ids from depth 0 downward.
    /// </summary>
    /// <returns>A copy of the populated rows. An empty chart returns an empty list.</returns>
    public IReadOnlyList<DepthChartRow> GetFullDepthChart() => _chart.GetFullDepthChart();

    /// <summary>
    /// Returns the player ids ranked strictly below the player at one position.
    /// </summary>
    /// <param name="player">The player to look up.</param>
    /// <param name="position">A position code from this chart's sport.</param>
    /// <returns>A copy of the ids below that player. The last player returns an empty list.</returns>
    public IReadOnlyList<int> GetPlayersBelow(Player player, string position) =>
        _chart.GetPlayersBelow(player, position);
}

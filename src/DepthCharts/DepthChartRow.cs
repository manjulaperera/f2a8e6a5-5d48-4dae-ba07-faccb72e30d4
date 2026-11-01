namespace DepthCharts;

/// <summary>
/// One populated position and the player ids ranked there, from depth 0 downward.
/// </summary>
public sealed class DepthChartRow
{
    /// <summary>
    /// Creates a row for one populated position.
    /// </summary>
    /// <param name="position">The position code.</param>
    /// <param name="playerIds">Player ids from depth 0 downward.</param>
    public DepthChartRow(string position, IReadOnlyList<int> playerIds)
    {
        Position = position;
        PlayerIds = playerIds;
    }

    /// <summary>
    /// Gets the position code.
    /// </summary>
    public string Position { get; }

    /// <summary>
    /// Gets the player ids from depth 0 downward.
    /// </summary>
    public IReadOnlyList<int> PlayerIds { get; }
}

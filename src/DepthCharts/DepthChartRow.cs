namespace DepthCharts;

public sealed class DepthChartRow
{
    public DepthChartRow(string position, IReadOnlyList<int> playerIds)
    {
        Position = position;
        PlayerIds = playerIds;
    }

    public string Position { get; }

    public IReadOnlyList<int> PlayerIds { get; }
}
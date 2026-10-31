namespace DepthCharts;

public sealed class DepthChartService : IDepthChartService
{
    private readonly SportDefinition _sport;
    private readonly Dictionary<string, List<Player>> _rankings = new();

    public DepthChartService(SportDefinition sport)
    {
        _sport = sport;
    }

    public void AddPlayer(Player player, string position, int depth)
    {
        if (!_rankings.TryGetValue(position, out var ranking))
        {
            ranking = [];
            _rankings[position] = ranking;
        }

        ranking.Insert(depth, player);
    }

    public IReadOnlyList<DepthChartRow> GetFullDepthChart()
    {
        var rows = new List<DepthChartRow>();
        foreach (var position in _sport.Positions)
        {
            if (!_rankings.TryGetValue(position, out var ranking) || ranking.Count == 0)
            {
                continue;
            }

            rows.Add(new DepthChartRow(position, ranking.ConvertAll(rankedPlayer => rankedPlayer.PlayerId)));
        }

        return rows;
    }
}

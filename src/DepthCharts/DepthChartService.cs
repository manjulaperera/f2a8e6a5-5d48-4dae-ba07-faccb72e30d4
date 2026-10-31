namespace DepthCharts;

public sealed class DepthChartService : IDepthChartService
{
    private readonly SportDefinition _sport;
    private readonly Dictionary<string, List<Player>> _rankings = new();

    public DepthChartService(SportDefinition sport)
    {
        _sport = sport;
    }

    public void AddPlayer(Player player, string position, int? depth = null)
    {
        if (!_rankings.TryGetValue(position, out var ranking))
        {
            ranking = [];
            _rankings[position] = ranking;
        }

        ranking.Remove(player);

        var index = depth is int requested && requested < ranking.Count
            ? requested
            : ranking.Count;
        ranking.Insert(index, player);
    }

    public void RemovePlayer(Player player, string position)
    {
        if (!_rankings.TryGetValue(position, out var ranking))
        {
            return;
        }

        ranking.Remove(player);

        if (ranking.Count == 0)
        {
            _rankings.Remove(position);
        }
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
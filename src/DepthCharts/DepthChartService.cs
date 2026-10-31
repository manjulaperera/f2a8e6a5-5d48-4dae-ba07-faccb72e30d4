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
        if (position is null || !_sport.Contains(position))
        {
            throw new ArgumentException("Position is not supported.", nameof(position));
        }

        if (depth < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(depth));
        }

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
        if (!_rankings.TryGetValue(position, out var ranking) || !ranking.Remove(player))
        {
            throw new ArgumentException("Player is not ranked at this position.", nameof(player));
        }

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

    public IReadOnlyList<int> GetPlayersBelow(Player player, string position)
    {
        if (!_rankings.TryGetValue(position, out var ranking))
        {
            throw new ArgumentException("Player is not ranked at this position.", nameof(player));
        }

        var index = ranking.FindIndex(rankedPlayer => rankedPlayer.PlayerId == player.PlayerId);
        if (index < 0)
        {
            throw new ArgumentException("Player is not ranked at this position.", nameof(player));
        }

        return ranking.Skip(index + 1).Select(rankedPlayer => rankedPlayer.PlayerId).ToList();
    }
}

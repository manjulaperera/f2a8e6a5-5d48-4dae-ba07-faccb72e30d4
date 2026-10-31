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
        RequirePlayer(player);
        RequireSupportedPosition(position);
        RequireNonNegativeDepth(depth);

        var ranking = GetOrCreateRanking(position);
        ranking.Remove(player);
        ranking.Insert(InsertionIndex(depth, ranking.Count), player);
    }

    public void RemovePlayer(Player player, string position)
    {
        RequirePlayer(player);
        RequireSupportedPosition(position);

        var ranking = RankingOrThrow(position, player);
        if (!ranking.Remove(player))
        {
            throw NotRanked(player);
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

            var playerIds = ranking.ConvertAll(rankedPlayer => rankedPlayer.PlayerId);
            rows.Add(new DepthChartRow(position, playerIds));
        }

        return rows;
    }

    public IReadOnlyList<int> GetPlayersBelow(Player player, string position)
    {
        RequirePlayer(player);
        RequireSupportedPosition(position);

        var ranking = RankingOrThrow(position, player);
        var index = ranking.FindIndex(rankedPlayer => rankedPlayer.PlayerId == player.PlayerId);
        if (index < 0)
        {
            throw NotRanked(player);
        }

        return ranking.Skip(index + 1).Select(rankedPlayer => rankedPlayer.PlayerId).ToList();
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

    private List<Player> GetOrCreateRanking(string position)
    {
        if (!_rankings.TryGetValue(position, out var ranking))
        {
            ranking = [];
            _rankings[position] = ranking;
        }

        return ranking;
    }

    private List<Player> RankingOrThrow(string position, Player player)
    {
        if (!_rankings.TryGetValue(position, out var ranking))
        {
            throw NotRanked(player);
        }

        return ranking;
    }

    private static int InsertionIndex(int? depth, int count)
    {
        return depth is int requested && requested < count
            ? requested
            : count;
    }

    private static ArgumentException NotRanked(Player player)
    {
        return new ArgumentException("Player is not ranked at this position.", nameof(player));
    }
}

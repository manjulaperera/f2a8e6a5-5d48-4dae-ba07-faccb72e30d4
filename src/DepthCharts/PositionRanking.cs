namespace DepthCharts;

internal sealed class PositionRanking
{
    private readonly List<Player> _players = [];

    public int Count => _players.Count;

    public void Place(Player player, int? depth)
    {
        _players.Remove(player);
        _players.Insert(IndexFor(depth), player);
    }

    public bool Remove(Player player) => _players.Remove(player);

    public IReadOnlyList<int> PlayerIds() => _players.ConvertAll(player => player.PlayerId);

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

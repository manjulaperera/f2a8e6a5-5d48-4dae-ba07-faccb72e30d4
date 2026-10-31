namespace DepthCharts;

public sealed class Player : IEquatable<Player>
{
    public Player(int playerId, string? name, string? naturalPosition)
    {
        if (playerId < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(playerId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        PlayerId = playerId;
        Name = name;
        NaturalPosition = naturalPosition;
    }

    public int PlayerId { get; }

    public string Name { get; }

    public string? NaturalPosition { get; }

    public bool Equals(Player? other) => other is not null && PlayerId == other.PlayerId;

    public override bool Equals(object? obj) => Equals(obj as Player);

    public override int GetHashCode() => PlayerId.GetHashCode();
}
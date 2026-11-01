namespace DepthCharts;

/// <summary>
/// A person who can be ranked on a depth chart. Identity is <see cref="PlayerId"/>.
/// </summary>
public sealed class Player : IEquatable<Player>
{
    /// <summary>
    /// Creates a player.
    /// </summary>
    /// <param name="playerId">Identity. Zero is allowed. A negative id is rejected.</param>
    /// <param name="name">Required name. Null, empty, and whitespace are rejected.</param>
    /// <param name="naturalPosition">Descriptive position. It does not grant a place on a chart. Null means none.</param>
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

    /// <summary>
    /// Gets the identity used to match this player on a chart.
    /// </summary>
    public int PlayerId { get; }

    /// <summary>
    /// Gets the player's name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the descriptive natural position, or null when the player has none.
    /// </summary>
    public string? NaturalPosition { get; }

    /// <summary>
    /// Returns whether the other player has the same <see cref="PlayerId"/>.
    /// </summary>
    /// <param name="other">The player to compare.</param>
    /// <returns>True when both players share an id.</returns>
    public bool Equals(Player? other) => other is not null && PlayerId == other.PlayerId;

    /// <summary>
    /// Returns whether the object is a player with the same <see cref="PlayerId"/>.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns>True when <paramref name="obj"/> is a player with the same id.</returns>
    public override bool Equals(object? obj) => Equals(obj as Player);

    /// <summary>
    /// Returns a hash code based on <see cref="PlayerId"/>.
    /// </summary>
    /// <returns>The hash code of the player id.</returns>
    public override int GetHashCode() => PlayerId.GetHashCode();
}

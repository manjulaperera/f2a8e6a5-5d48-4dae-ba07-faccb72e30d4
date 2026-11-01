namespace DepthCharts;

/// <summary>
/// The name and published position codes for one sport.
/// </summary>
public sealed class SportDefinition
{
    /// <summary>
    /// Gets the NFL position list: QB, WR, RB, TE, K, P, KR, PR.
    /// </summary>
    public static SportDefinition Nfl { get; } = new(
        "NFL",
        ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);

    /// <summary>
    /// Gets the MLB position list: SP, RP, C, 1B, 2B, 3B, SS, LF, RF, CF, DH.
    /// </summary>
    public static SportDefinition Mlb { get; } = new(
        "MLB",
        ["SP", "RP", "C", "1B", "2B", "3B", "SS", "LF", "RF", "CF", "DH"]);

    private readonly IReadOnlyList<string> _positions;

    /// <summary>
    /// Creates a sport from a name and an ordered list of position codes.
    /// </summary>
    /// <param name="name">The sport name.</param>
    /// <param name="positions">Published order. Duplicate codes are rejected.</param>
    public SportDefinition(string name, IEnumerable<string> positions)
    {
        var published = new List<string>();
        foreach (var position in positions)
        {
            if (published.Contains(position))
            {
                throw new ArgumentException("Position codes must be unique.", nameof(positions));
            }

            published.Add(position);
        }

        Name = name;
        _positions = published.AsReadOnly();
    }

    /// <summary>
    /// Gets the sport name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the position codes in published order.
    /// </summary>
    public IReadOnlyList<string> Positions => _positions;

    /// <summary>
    /// Returns whether the code is one of this sport's positions.
    /// </summary>
    /// <param name="position">The code to test. Matching is exact, including capitalisation.</param>
    /// <returns>True when the code is published for this sport. Null returns false.</returns>
    public bool Contains(string? position) => position is not null && _positions.Contains(position);
}

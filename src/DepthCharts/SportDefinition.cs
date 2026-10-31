namespace DepthCharts;

public sealed class SportDefinition
{
    public static SportDefinition Nfl { get; } = new(
        "NFL",
        ["QB", "WR", "RB", "TE", "K", "P", "KR", "PR"]);

    public static SportDefinition Mlb { get; } = new(
        "MLB",
        ["SP", "RP", "C", "1B", "2B", "3B", "SS", "LF", "RF", "CF", "DH"]);

    private readonly IReadOnlyList<string> _positions;

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

    public string Name { get; }

    public IReadOnlyList<string> Positions => _positions;

    public bool Contains(string? position) => position is not null && _positions.Contains(position);
}

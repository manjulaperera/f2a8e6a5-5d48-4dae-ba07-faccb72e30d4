namespace DepthCharts;

public sealed class SportDefinition
{
    private readonly List<string> _positions;

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
        _positions = published;
    }

    public string Name { get; }

    public IReadOnlyList<string> Positions => _positions;

    public bool Contains(string? position) => position is not null && _positions.Contains(position);
}
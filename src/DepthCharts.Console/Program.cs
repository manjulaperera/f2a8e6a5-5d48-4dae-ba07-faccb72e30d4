using DepthCharts;

var chart = new DepthChartService(SportDefinition.Nfl);
var bob = new Player(1, "Bob", "WR");
var alice = new Player(2, "Alice", "WR");
var charlie = new Player(3, "Charlie", "WR");

chart.AddPlayer(bob, "WR", 0);
chart.AddPlayer(alice, "WR", 0);
chart.AddPlayer(charlie, "WR", 2);
chart.AddPlayer(bob, "KR");

foreach (var row in chart.GetFullDepthChart())
{
    Console.WriteLine($"{row.Position}: [{string.Join(", ", row.PlayerIds)}]");
}

Console.WriteLine($"[{string.Join(", ", chart.GetPlayersBelow(alice, "WR"))}]");

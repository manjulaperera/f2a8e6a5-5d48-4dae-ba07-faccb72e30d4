using DepthCharts;

internal static class DepthChartConsole
{
    public static int Run(TextReader input, TextWriter output, TextWriter error)
    {
        error.WriteLine("Depth chart. Type help.");
        var session = new Session();

        while (true)
        {
            error.Write("> ");
            error.Flush();
            var line = input.ReadLine();
            if (line is null || !session.Execute(line.Trim(), output))
            {
                return 0;
            }
        }
    }

    private sealed class Session
    {
        private readonly Dictionary<int, Player> _players = [];
        private DepthChartService _chart = new(SportDefinition.Nfl);

        public bool Execute(string line, TextWriter output)
        {
            if (line.Length == 0)
            {
                return true;
            }

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            switch (parts[0].ToLowerInvariant())
            {
                case "help":
                    if (RequireCount(parts, 1, output, "help"))
                    {
                        WriteHelp(output);
                    }

                    return true;
                case "quit":
                case "exit":
                    if (RequireCount(parts, 1, output, "quit"))
                    {
                        return false;
                    }

                    return true;
                case "sport":
                    Sport(parts, output);
                    return true;
                case "add":
                    Add(parts, output);
                    return true;
                case "remove":
                    Remove(parts, output);
                    return true;
                case "chart":
                    if (RequireCount(parts, 1, output, "chart"))
                    {
                        WriteChart(output);
                    }

                    return true;
                case "below":
                    Below(parts, output);
                    return true;
                default:
                    output.WriteLine("Error: Unknown command. Type help.");
                    return true;
            }
        }

        private void Sport(string[] parts, TextWriter output)
        {
            if (parts.Length == 2 && parts[1].Equals("nfl", StringComparison.OrdinalIgnoreCase))
            {
                UseSport(SportDefinition.Nfl, output);
                return;
            }

            if (parts.Length == 2 && parts[1].Equals("mlb", StringComparison.OrdinalIgnoreCase))
            {
                UseSport(SportDefinition.Mlb, output);
                return;
            }

            if (parts.Length == 3)
            {
                var codes = parts[2].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                try
                {
                    UseSport(new SportDefinition(parts[1], codes), output);
                }
                catch (ArgumentException exception)
                {
                    output.WriteLine($"Error: {exception.Message}");
                }

                return;
            }

            output.WriteLine("Error: Usage: sport nfl|mlb|<name> <code,code>");
        }

        private void UseSport(SportDefinition sport, TextWriter output)
        {
            _chart = new DepthChartService(sport);
            _players.Clear();
            output.WriteLine($"Sport: {sport.Name}.");
        }

        private void Add(string[] parts, TextWriter output)
        {
            if (parts.Length is not (5 or 6))
            {
                output.WriteLine("Error: Usage: add <id> <name> <naturalPosition> <position> [depth]");
                return;
            }

            if (!TryParseId(parts[1], output, out var id))
            {
                return;
            }

            int? depth = null;
            if (parts.Length == 6)
            {
                if (!int.TryParse(parts[5], out var parsedDepth))
                {
                    output.WriteLine("Error: Depth must be an integer.");
                    return;
                }

                depth = parsedDepth;
            }

            try
            {
                var naturalPosition = parts[3] == "-" ? null : parts[3];
                var player = new Player(id, parts[2], naturalPosition);
                _chart.AddPlayer(player, parts[4], depth);
                _players[id] = player;
                output.WriteLine($"Added {id} at {parts[4]}.");
            }
            catch (ArgumentException exception)
            {
                output.WriteLine($"Error: {exception.Message}");
            }
        }

        private void Remove(string[] parts, TextWriter output)
        {
            if (!TryPlayerCommand(parts, output, "remove <id> <position>", out var player, out var position) || player is null)
            {
                return;
            }

            try
            {
                _chart.RemovePlayer(player, position);
                output.WriteLine($"Removed {player.PlayerId} from {position}.");
            }
            catch (ArgumentException exception)
            {
                output.WriteLine($"Error: {exception.Message}");
            }
        }

        private void Below(string[] parts, TextWriter output)
        {
            if (!TryPlayerCommand(parts, output, "below <id> <position>", out var player, out var position) || player is null)
            {
                return;
            }

            try
            {
                var ids = _chart.GetPlayersBelow(player, position);
                output.WriteLine($"[{string.Join(", ", ids)}]");
            }
            catch (ArgumentException exception)
            {
                output.WriteLine($"Error: {exception.Message}");
            }
        }

        private void WriteChart(TextWriter output)
        {
            foreach (var row in _chart.GetFullDepthChart())
            {
                output.WriteLine($"{row.Position}: [{string.Join(", ", row.PlayerIds)}]");
            }
        }

        private bool TryPlayerCommand(
            string[] parts,
            TextWriter output,
            string usage,
            out Player? player,
            out string position)
        {
            player = null;
            position = string.Empty;
            if (!RequireCount(parts, 3, output, usage))
            {
                return false;
            }

            if (!TryParseId(parts[1], output, out var id))
            {
                return false;
            }

            if (!_players.TryGetValue(id, out var stored))
            {
                output.WriteLine($"Error: Add player {id} before using that id.");
                return false;
            }

            player = stored;
            position = parts[2];
            return true;
        }

        private static bool TryParseId(string text, TextWriter output, out int id)
        {
            if (int.TryParse(text, out id))
            {
                return true;
            }

            output.WriteLine("Error: Player id must be an integer.");
            return false;
        }

        private static bool RequireCount(string[] parts, int count, TextWriter output, string usage)
        {
            if (parts.Length == count)
            {
                return true;
            }

            output.WriteLine($"Error: Usage: {usage}");
            return false;
        }

        private static void WriteHelp(TextWriter output)
        {
            output.WriteLine(
                """
                sport nfl|mlb
                sport <name> <code,code>
                add <id> <name> <naturalPosition> <position> [depth]
                remove <id> <position>
                chart
                below <id> <position>
                help
                quit
                """);
        }
    }
}

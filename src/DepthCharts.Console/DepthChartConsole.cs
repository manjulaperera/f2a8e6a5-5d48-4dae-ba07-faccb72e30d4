using DepthCharts;

namespace DepthCharts.ConsoleHost;

internal static class DepthChartConsole
{
    public static int Run(TextReader input, TextWriter output, TextWriter error)
    {
        var session = new Session();

        while (true)
        {
            session.WriteMenu(error);
            WriteColored(error, "> ", ConsoleColor.Cyan, newLine: false);
            error.Flush();
            var line = input.ReadLine();
            if (line is null || !session.Execute(line.Trim(), input, output, error))
            {
                return 0;
            }
        }
    }

    private static void WriteColored(TextWriter writer, string text, ConsoleColor color, bool newLine = true)
    {
        var useColor = ReferenceEquals(writer, Console.Out)
            ? !Console.IsOutputRedirected
            : ReferenceEquals(writer, Console.Error) && !Console.IsErrorRedirected;

        if (!useColor)
        {
            if (newLine)
            {
                writer.WriteLine(text);
            }
            else
            {
                writer.Write(text);
            }

            return;
        }

        var previous = Console.ForegroundColor;
        try
        {
            Console.ForegroundColor = color;
            if (newLine)
            {
                writer.WriteLine(text);
            }
            else
            {
                writer.Write(text);
            }
        }
        finally
        {
            Console.ForegroundColor = previous;
        }
    }

    private sealed class Session
    {
        private readonly Dictionary<int, Player> _players = [];
        private SportDefinition _sport = SportDefinition.Nfl;
        private DepthChartService _chart = new(SportDefinition.Nfl);

        public void WriteMenu(TextWriter error)
        {
            error.WriteLine();
            WriteColored(error, "Depth chart", ConsoleColor.Cyan);
            WriteColored(error, $"Sport: {_sport.Name}", ConsoleColor.Yellow);
            WriteColored(error, "Positions: " + string.Join("  ", _sport.Positions), ConsoleColor.DarkCyan);
            error.WriteLine();
            WriteMenuItem(error, "1", "Add a player", "place a player at a position");
            WriteMenuItem(error, "2", "Remove a player", "take a player off one position");
            WriteMenuItem(error, "3", "Show the chart", "list each position from the top");
            WriteMenuItem(error, "4", "Players below", "list the ids under a player");
            WriteMenuItem(error, "5", "Change sport", "NFL, MLB, or your own positions");
            WriteMenuItem(error, "6", "Command help", "show the one-line commands");
            WriteMenuItem(error, "0", "Quit", "leave the console");
            error.WriteLine();
            WriteColored(error, "Choose a number, or type a command.", ConsoleColor.DarkGray);
        }

        public bool Execute(string line, TextReader input, TextWriter output, TextWriter error)
        {
            if (line.Length == 0)
            {
                return true;
            }

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            switch (parts[0].ToLowerInvariant())
            {
                case "0":
                case "quit":
                case "exit":
                    if (parts.Length == 1)
                    {
                        return false;
                    }

                    WriteMessage(output, "Error: Usage: quit", ConsoleColor.Red);
                    return true;
                case "1":
                    return AddFromMenu(parts, input, output, error);
                case "2":
                    return PlayerFromMenu(parts, input, output, error, "2", Remove);
                case "3":
                    if (parts.Length == 1)
                    {
                        WriteChart(output);
                    }
                    else
                    {
                        WriteMessage(output, "Error: Usage: chart", ConsoleColor.Red);
                    }

                    return true;
                case "4":
                    return PlayerFromMenu(parts, input, output, error, "4", Below);
                case "5":
                    return SportFromMenu(parts, input, output, error);
                case "6":
                case "help":
                    if (parts.Length == 1)
                    {
                        WriteCommandHelp(error);
                    }
                    else
                    {
                        WriteMessage(output, "Error: Usage: help", ConsoleColor.Red);
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
                    WriteMessage(output, "Error: Unknown command. Choose a number from the menu.", ConsoleColor.Red);
                    return true;
            }
        }

        private bool AddFromMenu(string[] parts, TextReader input, TextWriter output, TextWriter error)
        {
            if (parts.Length != 1)
            {
                WriteMessage(output, "Error: Enter 1, or use: add <id> <name> <naturalPosition> <position> [depth]", ConsoleColor.Red);
                return true;
            }

            var id = ReadPrompt(input, error, "Player id: ");
            var name = ReadPrompt(input, error, "Name: ");
            var naturalPosition = ReadPrompt(input, error, "Natural position (- if none): ");
            var position = ReadPrompt(input, error, "Chart position: ");
            var depth = ReadPrompt(input, error, "Depth (blank adds at the end): ");
            if (id is null || name is null || naturalPosition is null || position is null || depth is null)
            {
                return false;
            }

            Add(depth.Length == 0
                ? ["add", id, name, naturalPosition, position]
                : ["add", id, name, naturalPosition, position, depth], output);
            return true;
        }

        private static bool PlayerFromMenu(
            string[] parts,
            TextReader input,
            TextWriter output,
            TextWriter error,
            string number,
            Action<string[], TextWriter> command)
        {
            if (parts.Length != 1)
            {
                WriteMessage(output, $"Error: Enter {number}, or type the full command.", ConsoleColor.Red);
                return true;
            }

            var id = ReadPrompt(input, error, "Player id: ");
            var position = ReadPrompt(input, error, "Chart position: ");
            if (id is null || position is null)
            {
                return false;
            }

            command(number == "2" ? ["remove", id, position] : ["below", id, position], output);
            return true;
        }

        private bool SportFromMenu(string[] parts, TextReader input, TextWriter output, TextWriter error)
        {
            if (parts.Length != 1)
            {
                WriteMessage(output, "Error: Enter 5, or use: sport nfl|mlb|<name> <code,code>", ConsoleColor.Red);
                return true;
            }

            var sport = ReadPrompt(input, error, "Sport (nfl, mlb, or a new name): ");
            if (sport is null)
            {
                return false;
            }

            if (sport.Equals("nfl", StringComparison.OrdinalIgnoreCase) || sport.Equals("mlb", StringComparison.OrdinalIgnoreCase))
            {
                Sport(["sport", sport], output);
                return true;
            }

            var codes = ReadPrompt(input, error, "Position codes, separated by commas: ");
            if (codes is null)
            {
                return false;
            }

            Sport(["sport", sport, codes], output);
            return true;
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
                    WriteMessage(output, $"Error: {exception.Message}", ConsoleColor.Red);
                }

                return;
            }

            WriteMessage(output, "Error: Usage: sport nfl|mlb|<name> <code,code>", ConsoleColor.Red);
        }

        private void UseSport(SportDefinition sport, TextWriter output)
        {
            _sport = sport;
            _chart = new DepthChartService(sport);
            _players.Clear();
            WriteMessage(output, $"Sport: {sport.Name}.", ConsoleColor.Green);
        }

        private void Add(string[] parts, TextWriter output)
        {
            if (parts.Length is not (5 or 6))
            {
                WriteMessage(output, "Error: Usage: add <id> <name> <naturalPosition> <position> [depth]", ConsoleColor.Red);
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
                    WriteMessage(output, "Error: Depth must be an integer.", ConsoleColor.Red);
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
                WriteMessage(output, $"Added {id} at {parts[4]}.", ConsoleColor.Green);
            }
            catch (ArgumentException exception)
            {
                WriteLibraryError(output, exception, parts[4]);
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
                WriteMessage(output, $"Removed {player.PlayerId} from {position}.", ConsoleColor.Green);
            }
            catch (ArgumentException exception)
            {
                WriteLibraryError(output, exception, position);
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
                WriteMessage(output, $"[{string.Join(", ", ids)}]", ConsoleColor.Yellow);
            }
            catch (ArgumentException exception)
            {
                WriteLibraryError(output, exception, position);
            }
        }

        private void WriteChart(TextWriter output)
        {
            foreach (var row in _chart.GetFullDepthChart())
            {
                WriteMessage(output, $"{row.Position}: [{string.Join(", ", row.PlayerIds)}]", ConsoleColor.Cyan);
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
                WriteMessage(output, $"Error: Add player {id} before using that id.", ConsoleColor.Red);
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

            WriteMessage(output, "Error: Player id must be an integer.", ConsoleColor.Red);
            return false;
        }

        private static bool RequireCount(string[] parts, int count, TextWriter output, string usage)
        {
            if (parts.Length == count)
            {
                return true;
            }

            WriteMessage(output, $"Error: Usage: {usage}", ConsoleColor.Red);
            return false;
        }

        private static string? ReadPrompt(TextReader input, TextWriter error, string label)
        {
            WriteColored(error, label, ConsoleColor.DarkCyan, newLine: false);
            error.Flush();
            return input.ReadLine()?.Trim();
        }

        private static void WriteMenuItem(TextWriter error, string number, string title, string description)
        {
            WriteColored(error, $"  {number}  ", ConsoleColor.Yellow, newLine: false);
            WriteColored(error, title.PadRight(20), ConsoleColor.Green, newLine: false);
            WriteColored(error, description, ConsoleColor.DarkGray);
        }

        private static void WriteCommandHelp(TextWriter error)
        {
            error.WriteLine();
            WriteColored(error, "One-line commands", ConsoleColor.Cyan);
            WriteColored(error, "add <id> <name> <naturalPosition> <position> [depth]", ConsoleColor.Gray);
            WriteColored(error, "remove <id> <position>", ConsoleColor.Gray);
            WriteColored(error, "chart", ConsoleColor.Gray);
            WriteColored(error, "below <id> <position>", ConsoleColor.Gray);
            WriteColored(error, "sport nfl", ConsoleColor.Gray);
            WriteColored(error, "sport mlb", ConsoleColor.Gray);
            WriteColored(error, "sport <name> <code,code>", ConsoleColor.Gray);
            WriteColored(error, "A name is one word. Use - when there is no natural position.", ConsoleColor.DarkGray);
        }

        private static void WriteLibraryError(TextWriter output, ArgumentException exception, string chartPosition)
        {
            var message = exception.Message.StartsWith("Position is not supported.", StringComparison.Ordinal)
                ? DescribeChartPosition(chartPosition)
                : exception.Message;
            WriteMessage(output, $"Error: {message}", ConsoleColor.Red);
        }

        private static string DescribeChartPosition(string position) =>
            string.IsNullOrWhiteSpace(position)
                ? "Chart position is required."
                : $"Chart position '{position}' is not supported.";

        private static void WriteMessage(TextWriter output, string text, ConsoleColor color)
        {
            WriteColored(output, text, color);
        }
    }
}

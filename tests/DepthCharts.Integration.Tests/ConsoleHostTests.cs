using System.Diagnostics;

using FluentAssertions;

namespace DepthCharts.Integration.Tests;

public class ConsoleHostTests
{
    [Fact]
    public async Task Given_the_published_adds_When_the_console_runs_Then_it_prints_the_chart_and_the_players_below_alice()
    {
        var output = await RunAsync(
            """
            add 1 Bob WR WR 0
            add 2 Alice WR WR 0
            add 3 Charlie WR WR 2
            add 1 Bob WR KR
            chart
            below 2 WR
            quit
            """);

        output.Should().Be(
            """
            Added 1 at WR.
            Added 2 at WR.
            Added 3 at WR.
            Added 1 at KR.
            WR: [2, 1, 3]
            KR: [1]
            [1, 3]
            """);
    }

    [Fact]
    public async Task Given_an_unknown_position_When_the_console_adds_a_player_Then_it_names_the_chart_position()
    {
        var output = await RunAsync(
            """
            add 1 Bob WR XX
            quit
            """);

        output.Should().Be("Error: Chart position 'XX' is not supported.");
    }

    [Fact]
    public async Task Given_a_blank_depth_and_an_unknown_position_When_the_menu_adds_a_player_Then_it_names_the_chart_position()
    {
        var output = await RunAsync(
            """
            1
            1
            Bob
            WR
            XX

            quit
            """);

        output.Should().Be("Error: Chart position 'XX' is not supported.");
    }

    [Fact]
    public async Task Given_a_blank_chart_position_When_the_menu_adds_a_player_Then_it_says_the_chart_position_is_required()
    {
        var output = await RunAsync(
            """
            1
            1
            Bob
            WR


            quit
            """);

        output.Should().Be("Error: Chart position is required.");
    }

    [Fact]
    public async Task Given_menu_numbers_When_a_player_is_added_and_the_chart_is_shown_Then_the_library_result_is_printed()
    {
        var output = await RunAsync(
            """
            1
            1
            Bob
            WR
            WR

            3
            0
            """);

        output.Should().Be(
            """
            Added 1 at WR.
            WR: [1]
            """);
    }

    [Fact]
    public async Task Given_mlb_When_a_player_is_added_at_sp_Then_the_chart_shows_that_position()
    {
        var output = await RunAsync(
            """
            sport mlb
            add 1 Bob SP SP
            chart
            quit
            """);

        output.Should().Be(
            """
            Sport: MLB.
            Added 1 at SP.
            SP: [1]
            """);
    }

    private static async Task<string> RunAsync(string script)
    {
        var project = Path.Combine(FindRepositoryRoot(), "src", "DepthCharts.Console", "DepthCharts.Console.csproj");
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{project}\" --nologo",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        });

        process.Should().NotBeNull();
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        await process.StandardInput.WriteAsync(script.Replace("\r\n", "\n", StringComparison.Ordinal));
        process.StandardInput.Close();

        var exited = process.WaitForExit(TimeSpan.FromMinutes(2));
        var output = await outputTask;
        var error = await errorTask;

        exited.Should().BeTrue(error);
        process.ExitCode.Should().Be(0, error);
        return output.Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd('\n');
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "DepthCharts.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }
}

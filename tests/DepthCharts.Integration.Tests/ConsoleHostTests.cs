using System.Diagnostics;

using FluentAssertions;

namespace DepthCharts.Integration.Tests;

public class ConsoleHostTests
{
    [Fact]
    public void Given_the_published_sample_When_the_console_runs_Then_it_prints_the_chart_and_the_players_below_alice()
    {
        var project = Path.Combine(FindRepositoryRoot(), "src", "DepthCharts.Console", "DepthCharts.Console.csproj");
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{project}\" --nologo",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        });

        process.Should().NotBeNull();
        var exited = process.WaitForExit(TimeSpan.FromMinutes(2));
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();

        exited.Should().BeTrue(error);
        process.ExitCode.Should().Be(0, error);
        output.Replace("\r\n", "\n").TrimEnd('\n').Split('\n')
            .Should().Equal("WR: [2, 1, 3]", "KR: [1]", "[1, 3]");
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

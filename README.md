# Depth Chart Management

[![Build and Test](https://github.com/manjulaperera/f2a8e6a5-5d48-4dae-ba07-faccb72e30d4/actions/workflows/ci.yml/badge.svg)](https://github.com/manjulaperera/f2a8e6a5-5d48-4dae-ba07-faccb72e30d4/actions/workflows/ci.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=manjulaperera_4072c526-95a1-451d-816a-53cf61cf9e34&metric=alert_status)](https://sonarcloud.io/summary/overall?id=manjulaperera_4072c526-95a1-451d-816a-53cf61cf9e34)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=manjulaperera_4072c526-95a1-451d-816a-53cf61cf9e34&metric=coverage)](https://sonarcloud.io/summary/overall?id=manjulaperera_4072c526-95a1-451d-816a-53cf61cf9e34)

## Problem

Most team sports keep a depth chart: an ordered ranking of players at each position. Depth 0 is the top of that list. A player may be ranked at a position other than their natural position, and at more than one position.

The challenge asks for four operations: add a player at a depth, remove a player from one position, return the full chart, and return the players below a player. NFL and MLB are required, and another sport is supplied as a list of position codes. The original text is in [`docs/DepthChartChallenge.md`](docs/DepthChartChallenge.md).

## Solution

`DepthChartService` keeps one in-memory chart for a `SportDefinition`. The same add, remove, and read rules apply to every sport. There is no database, web API, or graphical interface.

NFL positions are QB, WR, RB, TE, K, P, KR, and PR. MLB positions are SP, RP, C, 1B, 2B, 3B, SS, LF, RF, CF, and DH. Codes match exactly, including capitalisation.

The published sample, Bob (id 1), Alice (id 2), and Charlie (id 3), ends as:

```text
WR: 2, 1, 3
KR: 1
```

Players below Alice at WR are `1, 3`.

Operations, edge cases, and a C# example are in [`docs/behaviour.md`](docs/behaviour.md). Storage and complexity are in [`docs/architecture.md`](docs/architecture.md).

## Out of scope

The challenge asks for the depth-chart library, its tests, and instructions to build and run it. These stay out of the solution:

1. A database or any saved chart. Rankings last only for the life of one `DepthChartService`.
2. A web API or other network endpoint.
3. A graphical user interface. The console only calls the library and prints the result.
4. A faster index or other structure for large charts.
5. Sport-specific ranking code, or one service that holds many charts. A new sport is another `SportDefinition` and another `DepthChartService`.
6. Continuous deployment. GitHub Actions builds and tests. It does not deploy.
7. Application logging, metrics, or tracing.
8. Authentication and access control.
9. Notifications when a ranking changes.
10. A dependency-injection container and extra projects for application layers.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Git

## Build

From the repository root:

```bash
dotnet restore DepthCharts.slnx
dotnet build DepthCharts.slnx --configuration Release
```

Nullable reference types are enabled. Compiler warnings are treated as errors. The class library has no package references.

## Format check

```bash
dotnet format DepthCharts.slnx --verify-no-changes
```

## Test

All tests:

```bash
dotnet test DepthCharts.slnx --configuration Release
```

Unit tests cover one type or one operation. Integration tests run the public operations together, including the published sample.

```bash
dotnet test tests/DepthCharts.Unit.Tests/DepthCharts.Unit.Tests.csproj --configuration Release
dotnet test tests/DepthCharts.Integration.Tests/DepthCharts.Integration.Tests.csproj --configuration Release
```

## SonarCloud

GitHub Actions publish coverage and quality metrics to SonarCloud when `SONAR_ENABLED` is `true`.

**Dashboard:** [SonarCloud project](https://sonarcloud.io/summary/overall?id=manjulaperera_4072c526-95a1-451d-816a-53cf61cf9e34)

Variables, coverage exclusions, and a local scanner run are in [`docs/sonar.md`](docs/sonar.md).

## Console

```bash
dotnet run --project src/DepthCharts.Console
```

The session starts as NFL. The menu lists the position codes for the current sport. A chart position must be one of those codes, such as `WR`. Leaving depth blank adds the player at the end.

```text
add 1 Bob WR WR 0
add 2 Alice WR WR 0
add 3 Charlie WR WR 2
add 1 Bob WR KR
chart
below 2 WR
quit
```

`chart` prints `WR: [2, 1, 3]` and `KR: [1]`. `below 2 WR` prints `[1, 3]`.

Menu, screenshots, and the one-line commands are in [`docs/console.md`](docs/console.md).

## Continuous integration

GitHub Actions runs on every push and pull request to `main`:

1. Restore dependencies
2. Release build with warnings as errors
3. Verify formatting (`dotnet format --verify-no-changes`)
4. Unit tests
5. Integration tests
6. SonarQube analysis (when `SONAR_ENABLED` is `true`)

Workflow file: [`.github/workflows/ci.yml`](.github/workflows/ci.yml)

## Development

Specification, Red-Green-Refactor, and review practice are in [`docs/development.md`](docs/development.md). A review of the library is in [`docs/code-review.md`](docs/code-review.md).

## Public API

```csharp
void AddPlayer(Player player, string position, int? depth = null);
void RemovePlayer(Player player, string position);
IReadOnlyList<DepthChartRow> GetFullDepthChart();
IReadOnlyList<int> GetPlayersBelow(Player player, string position);
```

`depth` is optional. Omitting it adds the player at the end of that position.

See [`specs/001-manage-depth-chart/contracts/depth-chart.md`](specs/001-manage-depth-chart/contracts/depth-chart.md) for the full contract.

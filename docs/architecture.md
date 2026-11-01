# Architecture

The production library is `src/DepthCharts`. `src/DepthCharts.Console` is an interactive host for calling that library. Unit tests are `tests/DepthCharts.Unit.Tests`. Integration and acceptance tests are `tests/DepthCharts.Integration.Tests`. The solution file is `DepthCharts.slnx`.

| Type | Responsibility |
| --- | --- |
| `Player` | Immutable person. `PlayerId` is the identity. `NaturalPosition` is descriptive and does not grant or deny a chart position. |
| `SportDefinition` | Immutable name and published position list. `Nfl` and `Mlb` are static instances of this type. |
| `DepthChart` | Internal aggregate. It owns the rankings and applies the same add, remove, and read rules for every sport. |
| `PositionRanking` | Internal ordered list at one position. It inserts, moves, and removes players. |
| `DepthChartService` | Public entry. It constructs one `DepthChart` and forwards the four operations. |
| `DepthChartRow` | One populated position and its player ids, from depth 0 downward. |
| `IDepthChartService` | The four operations as a public contract. |

There is one implementation of the interface. Sport differences are the position list, not subclasses or branches inside the service. There is no dependency-injection container.

A second sport, or a second team, is a second `DepthChartService`. Two services do not share rankings, even when they are given the same `SportDefinition` instance and the same player id.

## Data structures

`DepthChart` keeps a private `Dictionary<string, PositionRanking>`. Each `PositionRanking` keeps a `List<Player>`.

- The key is the position code.
- Index 0 is depth 0. There are no holes.
- A position with no players has no key. Removing the last player at a position removes the key.
- `List<T>.Insert` and `List<T>.Remove` shift the tail and close gaps.
- The full chart does not walk dictionary order. It walks `SportDefinition.Positions` and skips positions that have no players, so adding KR before WR still returns WR before KR.

`Player` equality and hash code use `PlayerId`, so a different object with the same id is the same player. `==` remains reference equality.

Returned id lists are copies. `SportDefinition.Positions` is a read-only wrapper over the list stored at construction. Changing the list that was passed into the constructor does not change the published order.

## Complexity

`n` is the number of players already at the named position. `P` is the number of positions in the sport. `N` is the number of players on the chart. These costs follow from the dictionary and list. They are not benchmark results.

| Operation | Time | Extra space |
| --- | --- | --- |
| Add a player who is not yet at that position | O(n) to shift the tail. Append is O(1) amortized. | O(1) |
| Move a player already at that position | O(n) to find and remove the id, then O(n) to insert. | O(1) |
| Remove a player | O(n) to find the id and close the gap. | O(1) |
| Players below a player | O(n) to find the id, then O(k) to copy the ids below. | O(k) |
| Full chart | O(P) position checks plus O(N) to copy the ranked ids. | O(N) |

A second index from player id to depth would make lookup faster and would have to be rewritten on every shift. The challenge does not require that. A linked list would still walk to a depth index.

## Layout

| Path | Contents |
| --- | --- |
| `src/DepthCharts` | Production library |
| `src/DepthCharts.Console` | Interactive console for calling the library |
| `coverage.runsettings` | Coverlet settings that exclude the console |
| `tests/DepthCharts.Unit.Tests` | Unit tests for one type or one operation |
| `tests/DepthCharts.Integration.Tests` | Acceptance tests that combine the public operations |
| `specs/001-manage-depth-chart` | Specification, plan, and tasks |
| `.github/workflows/ci.yml` | Build, test, and SonarQube analysis |
| `dotnet-tools.json` | Local SonarScanner for .NET |
| `docs/images` | Screenshots of the console menu, sample, and an error |
| `docs/DepthChartChallenge.md` | Original challenge |
| `docs/behaviour.md` | Operations, positions, assumptions, and examples |
| `docs/architecture.md` | Types, storage, and complexity |
| `docs/console.md` | Console menu and commands |
| `docs/sonar.md` | SonarQube setup |
| `docs/development.md` | Specification workflow and review practice |
| `docs/code-review.md` | Review notes |

Build outputs under `bin/` and `obj/` are not part of the solution.

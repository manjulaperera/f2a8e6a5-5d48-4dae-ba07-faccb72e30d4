# Depth Chart Management

A .NET 10 class library that ranks players by position for a team sport. It implements the four depth-chart operations from the coding challenge for NFL and MLB, and accepts another sport by configuration.

The library has no database, web API, or user interface. Rankings live in memory for the lifetime of one service instance.

## Problem

Team sports keep a depth chart: an ordered list of players at each position. Depth 0 is the top of that list. A player may be ranked at a position other than their natural position, and at more than one position.

The challenge asks for four operations:

| Challenge operation | Library method |
| --- | --- |
| `addPlayerToDepthChart(player, position, position_depth)` | `AddPlayer(player, position, depth)` |
| `removePlayerFromDepthChart(player, position)` | `RemovePlayer(player, position)` |
| `getFullDepthChart()` | `GetFullDepthChart()` |
| `getPlayersUnderPlayerInDepthChart(player, position)` | `GetPlayersBelow(player, position)` |

Method names follow C# conventions. `depth` is optional. Omitting it adds the player at the end of that position.

The published sample, Bob (id 1), Alice (id 2), and Charlie (id 3), ends as:

```text
WR: 2, 1, 3
KR: 1
```

Players below Alice at WR are `1, 3`.

## Implemented functionality

`DepthChartService` is constructed with one `SportDefinition` and implements `IDepthChartService`:

- Add a player at a zero-based depth, or at the end when depth is omitted.
- Insert at an occupied depth and shift the players at that depth and below down by one.
- When the same `PlayerId` is already at that position, move that single entry. Other players keep their relative order.
- Remove a player from one position and close the gap. Depths stay contiguous from 0.
- Return the full chart as position rows of player ids. Empty positions are omitted. Row order is the sport's published order.
- Return the player ids strictly below a player at one position.

Results identify players by `PlayerId`. Names are not part of a ranking result.

## Supported positions

Position codes match exactly, including capitalisation. `wr` is not `WR`.

**NFL** (`SportDefinition.Nfl`): QB, WR, RB, TE, K, P, KR, PR

**MLB** (`SportDefinition.Mlb`): SP, RP, C, 1B, 2B, 3B, SS, LF, RF, CF, DH

## Architecture

The production project is `src/DepthCharts`. Unit tests are `tests/DepthCharts.Unit.Tests`. Integration and acceptance tests are `tests/DepthCharts.Integration.Tests`. The solution file is `DepthCharts.slnx`.

| Type | Responsibility |
| --- | --- |
| `Player` | Immutable person. `PlayerId` is the identity. `NaturalPosition` is descriptive and does not grant or deny a chart position. |
| `SportDefinition` | Immutable name and published position list. `Nfl` and `Mlb` are static instances of this type. |
| `DepthChartService` | One chart. It owns the rankings and applies the same add, remove, and read rules for every sport. |
| `DepthChartRow` | One populated position and its player ids, from depth 0 downward. |
| `IDepthChartService` | The four operations as a public contract. |

There is one implementation of the interface. Sport differences are the position list, not subclasses or branches inside the service. There is no dependency-injection container.

A second sport, or a second team, is a second `DepthChartService`. Two services do not share rankings, even when they are given the same `SportDefinition` instance and the same player id.

## Data structures

Rankings are a private `Dictionary<string, List<Player>>`.

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

## Assumptions and edge cases

The challenge fixes the sample, the NFL and MLB codes, multi-position ranking, and "add at the end when depth is omitted." The behaviours below are decisions recorded in `specs/001-manage-depth-chart/spec.md` where the challenge is silent. The specification is the authority for those cases.

| Case | Behaviour |
| --- | --- |
| Depth omitted, player not yet at that position | Append. On an empty position the player is at depth 0. |
| Depth equal to the current count | Append. Charlie at WR depth 2, with two players already there, is the challenge sample. |
| Depth greater than the current count | Append. No empty depth is created. |
| Depth greater than 0 on an empty position | The player is placed at depth 0. |
| Occupied depth, player not yet at that position | The new player takes that depth. Everyone from there down shifts down one. |
| Same `PlayerId` added again at that position | One entry, moved to the requested depth. Others keep relative order. Omitted depth, or a depth at or past the current count, places that player last. |
| Re-add at the depth the player already occupies | The order stays the same. |
| Remove the top, middle, or last player | Remaining players keep relative order and are renumbered from 0. The last player removes that position from later full-chart results. |
| Remove, or players-below, when that id is not at that position | `ArgumentException`. The chart is unchanged. This includes a player ranked only at another position. An empty list is not used for a missing player. |
| Null player on add, remove, or players-below | `ArgumentNullException`, before position and depth checks. The chart is unchanged. |
| Null position, a code from another sport, an unknown code, or different capitalisation | `ArgumentException`. The chart is unchanged. |
| Depth below 0 | `ArgumentOutOfRangeException`. The chart is unchanged, including when that player is already ranked. |
| Blank name, or a negative player id | Rejected by the `Player` constructor. `0` is a valid id. A `Player` instance cannot carry a blank name. |
| Two records, same `PlayerId` | The same player, including when the name or natural position differs. |
| Same name, different `PlayerId` | Different players. |
| Same player id on two services | Each chart is independent. Removing the player from one leaves the other unchanged. |
| Natural position | Does not control eligibility. A player whose natural position is `SP` may be ranked at NFL `WR`. |
| Empty chart, full read | Empty collection. |
| Last player, players-below | Empty collection. |
| Full chart with at least one player | Populated positions only, in the sport's published order. |

Duplicate position codes in one `SportDefinition` are rejected with `ArgumentException`.

## Build

Requires the .NET 10 SDK.

```bash
dotnet restore DepthCharts.slnx
dotnet build DepthCharts.slnx --configuration Release
```

Nullable reference types are enabled. Compiler warnings are treated as errors. The class library has no package references.

GitHub Actions, in `.github/workflows/ci.yml`, restores, builds the Release configuration with warnings as errors, checks formatting, then runs the unit tests and the integration tests as separate steps. That workflow runs on pushes and pull requests to `main`.

## Test

```bash
dotnet test tests/DepthCharts.Unit.Tests/DepthCharts.Unit.Tests.csproj --configuration Release
dotnet test tests/DepthCharts.Integration.Tests/DepthCharts.Integration.Tests.csproj --configuration Release
```

`dotnet test DepthCharts.slnx --configuration Release` runs both projects.

Unit tests cover one type or one operation: `Player`, `SportDefinition`, and each chart operation. Integration tests run the public operations together, including the published challenge sample and more than one chart or sport.

Tests are xUnit, named Given/When/Then, and assert with FluentAssertions 7.2.2. `ThrowExactly` is used where a derived exception must not satisfy the test. Moq is referenced by the unit project for a test that needs to isolate a dependency. The current tests construct `DepthChartService` and players directly and do not use a mock.

Neither test project references NHibernate. The chart is in memory and has no database.

## Example

```csharp
using DepthCharts;

var chart = new DepthChartService(SportDefinition.Nfl);
var bob = new Player(1, "Bob", "WR");
var alice = new Player(2, "Alice", "WR");
var charlie = new Player(3, "Charlie", "WR");

chart.AddPlayer(bob, "WR", 0);
chart.AddPlayer(alice, "WR", 0);
chart.AddPlayer(charlie, "WR", 2);
chart.AddPlayer(bob, "KR");

foreach (DepthChartRow row in chart.GetFullDepthChart())
{
    // WR: 2, 1, 3
    // KR: 1
    Console.WriteLine($"{row.Position}: {string.Join(", ", row.PlayerIds)}");
}

IReadOnlyList<int> belowAlice = chart.GetPlayersBelow(alice, "WR");
// 1, 3
```

An MLB chart is `new DepthChartService(SportDefinition.Mlb)`.

## Adding another sport

Construct a `SportDefinition` and pass it to a new service. The ranking code stays the same.

```csharp
var football = new SportDefinition(
    "Association Football",
    ["GK", "ST"]);
var chart = new DepthChartService(football);

chart.AddPlayer(new Player(1, "Bob", "GK"), "GK", 0);
```

`WR` is rejected on that chart because it is not in the definition. Adding the sport does not change `SportDefinition.Nfl` or `SportDefinition.Mlb`.

## Specification-driven development

GitHub Spec Kit holds the agreed behaviour under `specs/001-manage-depth-chart`. The constitution is `.specify/memory/constitution.md`.

The artifacts were produced in this order:

1. **Constitution.** Engineering rules: C#/.NET 10, xUnit, specification first, Red-Green-Refactor, no database, API, or UI, and no AI co-author on commits.
2. **Specification** (`spec.md`). The challenge rewritten as user scenarios and functional requirements, including NFL, MLB, and a further sport supplied only as position names.
3. **Clarification.** Open cases were decided and written into the specification and the assumptions above, instead of being left for the implementation to invent.
4. **Plan** (`plan.md`, `data-model.md`, `research.md`, `contracts/depth-chart.md`). The types, the dictionary of lists, the validation order, and the complexity above.
5. **Tasks** (`tasks.md`). One Red, Green, or Refactor step per behaviour.

Production code was added only for behaviour those artifacts already described.

## Red-Green-Refactor

Each behaviour followed the same cycle:

1. Write a failing xUnit test for the observable outcome.
2. Run it and confirm it fails for the expected reason.
3. Write the minimum production code that makes it pass.
4. Run the suite.
5. Refactor only when the structure is clearer and the observable behaviour stays the same.
6. Run the suite again.

Tests assert returned ids, row order, and exception type. They do not read the private dictionary. Existing assertions were left in place when later code was added.

## Human-reviewed AI-assisted development

Cursor was the editor used to draft the Spec Kit artifacts and the implementation. Spec Kit supplied the constitution, specification, plan, and task workflow. The developer chose each slice, reviewed the proposed change before it was treated as accepted work, and controlled every commit.

The practice recorded in the constitution is:

- One task at a time, with the assumption and the trade-off stated before the edit.
- A human review at each Red-Green-Refactor checkpoint.
- Commits only when the developer asks for them.
- No `Co-authored-by` trailer for Cursor or any other assistant.

A review of the library against the challenge, constitution, specification, and plan is in `docs/code-review.md`.

## Layout

| Path | Contents |
| --- | --- |
| `src/DepthCharts` | Production library |
| `tests/DepthCharts.Unit.Tests` | Unit tests for one type or one operation |
| `tests/DepthCharts.Integration.Tests` | Acceptance tests that combine the public operations |
| `specs/001-manage-depth-chart` | Specification, plan, and tasks |
| `.github/workflows/ci.yml` | Build and test workflow |
| `docs/DepthChartChallenge.md` | Original challenge |
| `docs/code-review.md` | Review notes |

Build outputs under `bin/` and `obj/` are not part of the solution.

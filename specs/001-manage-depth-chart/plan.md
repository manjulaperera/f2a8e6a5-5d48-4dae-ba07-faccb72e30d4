# Implementation Plan: Manage Depth Chart

**Branch**: `001-manage-depth-chart` | **Date**: 2026-10-31 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-manage-depth-chart/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

Deliver an in-memory C# class library that ranks players on one sport's depth chart. Callers add a player at a zero-based depth, remove a player, read the full chart, and read the players ranked below a named player. NFL and MLB are supplied as `SportDefinition` configuration. A further sport is another `SportDefinition`. The ranking operations live in `DepthChartService` and do not change when a sport is added.

Storage is a private `Dictionary<string, List<Player>>`: the key is the position code, and the list index is the depth. Outputs are immutable copies of player ids. Invalid input is rejected with the exception named in [research.md](./research.md), and the chart is left unchanged.

## Technical Context

**Language/Version**: C# on .NET 10 (`net10.0`)

**Primary Dependencies**: None in the class library. The test project uses xUnit, Microsoft.NET.Test.Sdk, and the xUnit Visual Studio runner. No other packages.

**Storage**: In memory only. One `DepthChartService` instance holds one chart in a `Dictionary<string, List<Player>>`. Nothing is written to disk.

**Testing**: xUnit. Tests assert observable player ids, chart order, and exception types. They do not inspect the private dictionary.

**Target Platform**: Any platform supported by the .NET 10 SDK. Local runs and GitHub Actions both use `dotnet test`.

**Project Type**: Class library plus a test project. No web API, database, or UI.

**Performance Goals**: No latency target. A position ranking of 20 players must keep relative order after an insertion at depth 0 and after removal of the player at depth 0 (SC-006). List insertion and removal are O(n) in the size of that position and are accepted.

**Constraints**: Nullable reference types enabled. Implicit usings enabled. Compiler warnings are errors. Returned collections are immutable copies. No third-party domain packages. No persistence, network endpoint, or user interface.

**Scale/Scope**: One sport and one chart per service instance. NFL has 8 positions and MLB has 11. A caller that needs two sports holds two service instances. Position rankings stay small; the specification's volume check is 20 players at one position.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-design

| Gate | Result | Evidence |
| --- | --- | --- |
| I. Specification-first | Pass | This plan implements only [spec.md](./spec.md). It does not add persistence, a UI, or extra sports beyond configuration. |
| II. Strict TDD | Pass | Production code for a behaviour is written only after a failing xUnit test for that behaviour. See Test strategy. |
| III. SOLID without extra abstraction | Pass with justification | `SportDefinition` is the variation point for a new sport. `IDepthChartService` has one implementation. See Complexity Tracking. |
| IV. Clean domain model | Pass | `Player`, `SportDefinition`, and `DepthChartRow` are small. Outputs are copies. |
| V. Sport extensibility | Pass | NFL and MLB are configuration. The service depends on `SportDefinition`, not on sport-specific branches. |
| VI. Deterministic behaviour | Pass | Rank order is list index. Full-chart order is the sport's published position list, not dictionary order. |
| VII. Explicit validation | Pass | Exception mapping is fixed in [research.md](./research.md). Rejected calls do not mutate the chart. |
| VIII. No unnecessary infrastructure | Pass | In-memory library and tests only. No API, database, or UI. O(n) list edits are not optimised further. |
| IX. Automated tests and CI | Pass | xUnit locally. GitHub Actions runs the same build with warnings as errors. |
| X–XII. Review, commits, authorship | Pass | The developer reviews each TDD checkpoint and controls commits. Commit messages do not attribute AI tools. |
| Nullable, warnings, immutability | Pass | Project settings and output copies are required below. |

### Post-design

Re-checked after [research.md](./research.md), [data-model.md](./data-model.md), [contracts/depth-chart.md](./contracts/depth-chart.md), and [quickstart.md](./quickstart.md). The design still matches the gates above. The single service interface remains the justified item in Complexity Tracking. No new infrastructure or sport-specific algorithm was introduced.

## Project Structure

### Documentation (this feature)

```text
specs/001-manage-depth-chart/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command)
├── quickstart.md        # Phase 1 output (/speckit-plan command)
├── contracts/           # Phase 1 output (/speckit-plan command)
└── tasks.md             # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)

```text
src/DepthChart/
├── DepthChart.csproj
├── Player.cs
├── SportDefinition.cs
├── DepthChartRow.cs
├── IDepthChartService.cs
└── DepthChartService.cs

tests/DepthChart.Tests/
├── DepthChart.Tests.csproj
└── DepthChartServiceTests.cs

.github/workflows/
└── ci.yml
```

**Structure Decision**: One class library, `src/DepthChart`, and one xUnit project, `tests/DepthChart.Tests`. The library has no `cli`, web, or storage project. GitHub Actions is the CI workflow, not a runtime host. These source files are the intended layout. This plan does not create them.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
| --- | --- | --- |
| `IDepthChartService` has a single implementation, `DepthChartService`. Principle III discourages an abstraction that has no variation. | The approved architecture requires a public service contract for the four chart operations. Tests and callers depend on that contract. The interface does not vary by sport. | Using only the concrete class would hide the contract inside one type. A sport subclass of the service was rejected because NFL and MLB do not change the ranking rules. `SportDefinition` already isolates the variation that Open-Closed requires. |

## Domain model

Details and validation rules are in [data-model.md](./data-model.md).

- **Player**: Immutable person. `PlayerId` (`int`) is the identity. `Name` is required. `NaturalPosition` is descriptive and does not control chart membership. Two players with the same `PlayerId` are the same player even when the instances differ. Two players with different ids are different even when the names match.
- **SportDefinition**: Immutable configuration. It carries the sport name and the published position list. NFL is `QB, WR, RB, TE, K, P, KR, PR`. MLB is `SP, RP, C, 1B, 2B, 3B, SS, LF, RF, CF, DH`. A position matches only that exact code.
- **DepthChartService**: One chart for the `SportDefinition` it was given. It owns the private dictionary. It is the only type that mutates rankings.
- **DepthChartRow**: Immutable result row. `Position` plus the player ids from depth 0 downward. Used only for full-chart output.

`Player` does not know about a chart. `SportDefinition` does not store players. The service is the composition of those two.

## Public service contract

The contract is specified in [contracts/depth-chart.md](./contracts/depth-chart.md). The surface is:

- `AddPlayer(Player player, string position, int? depth = null)`
- `RemovePlayer(Player player, string position)`
- `GetFullDepthChart()` returns `IReadOnlyList<DepthChartRow>`
- `GetPlayersBelow(Player player, string position)` returns `IReadOnlyList<int>`

`depth` omitted (`null`) appends a new player, or moves an existing player to the last depth. Returned lists are copies. The caller cannot change the chart through a returned collection. Results identify players by `PlayerId`. Names are not part of the ranking result.

A second chart is a second `DepthChartService`. Removing a player from an NFL instance does not change an MLB instance that holds the same player id.

## Data structure selection

Private field:

```text
Dictionary<string, List<Player>>
```

The dictionary key is the position code. The list is that position's ranking. Index 0 is depth 0. A position with no players has no key. Removing the last player at a position removes the key, so an empty position cannot appear in the full chart.

The dictionary is not enumerated to order the full chart. `GetFullDepthChart` walks `SportDefinition` from first published position to last and includes a position only when its list exists and is non-empty. That keeps QB before WR even when KR was added first, and it does not depend on hash order.

`List<Player>` is the rank structure because depth is an index, insertion at an occupied index must shift the tail, and removal must close the gap. Those are the list operations `Insert` and `RemoveAt`. A linked list would make index-based depth an O(n) walk without improving the shift. A second index from player id to depth would make lookup O(1) and would have to be rewritten on every shift. The specification's largest ranking check is 20 players, so that index is premature.

## Algorithm complexity

`n` is the number of players already ranked at the named position. `P` is the number of positions in the sport definition. `N` is the number of players on the chart. Dictionary lookup by position is O(1) on average for these small key sets.

| Operation | Time | Extra space |
| --- | --- | --- |
| Add a player who is not yet at that position | O(n) to shift the tail. Append is O(1) amortized. | O(1) |
| Move a player already at that position | O(n) to find the id, O(n) to close the old gap, O(n) to insert at the new depth. | O(1) |
| Remove a player | O(n) to find the id and close the gap. | O(1) |
| Players below a player | O(n) to find the id, then O(k) to copy the ids below it. | O(k) for the copy |
| Full chart | O(P) position checks plus O(N) to copy the ranked ids. | O(N) for the copies |

Finding a player is a forward scan for `PlayerId`, not `ReferenceEquals`.

Move, after validation, is:

1. Find the current index by `PlayerId`.
2. If the depth is omitted, or the supplied depth is greater than or equal to the current count, the destination is the last depth after the player is taken out.
3. If the supplied depth is from 0 through one less than the current count, that depth is the destination.
4. Remove the player, which closes the gap.
5. Insert the same player at the destination in the shortened list.

That leaves the player once, at the required depth, and leaves every other player in the same relative order. Adding the player at the depth they already occupy removes them and inserts them back at that depth, so the ranking is unchanged.

A new player whose depth is greater than the current count is appended. No empty slots are created. On an empty position, a depth greater than 0 appends, and the player occupies depth 0. A depth equal to the current count also appends, which is the published case of Charlie at WR depth 2.

## Validation strategy

Checks run in this order. The first failure wins, and it happens before any mutation.

| Order | Condition | Exception |
| --- | --- | --- |
| 1 | `player` is null | `ArgumentNullException` |
| 2 | `Name` is null, empty, or whitespace | `ArgumentException` |
| 3 | `position` is null, or is not an exact code in the sport definition | `ArgumentException` |
| 4 | `depth` is supplied and is less than 0 | `ArgumentOutOfRangeException` |
| 5 | Remove, or players-below, and no ranked player at that position has that `PlayerId` | `ArgumentException` |

`GetFullDepthChart` takes no arguments. An empty chart returns an empty list. It does not throw.

A whitespace-only name is treated as blank. The specification requires a blank name to be rejected and leaves a name of only spaces unspecified. This plan closes that gap so validation tests have one outcome.

`PlayerId` is a non-nullable `int`. A missing id cannot be represented. The null-player check covers a missing player. `0` is a valid id because the specification does not reject it.

`NaturalPosition` is not validated against the chart's sport. A player whose natural position is `SP` may be ranked at NFL `WR`.

## Test strategy

Tests are written before the production code that makes them pass, one behaviour at a time: write the test, see it fail for the expected reason, implement the minimum code, then refactor under a green suite. Tests stay independent. Each test constructs its own `DepthChartService` and its own players. Tests call `IDepthChartService` and assert returned ids, row order, and exception type. They do not read the private dictionary.

Coverage required by the specification:

- Published sample: Bob, Alice, and Charlie produce WR ids `2, 1, 3` and KR id `1`. Players below Alice at WR are `1, 3`.
- Insert at the top, the middle, and the end. Omit the depth. Use a depth equal to the current count.
- Move an existing player up, down, to the same depth, to the end by omitting the depth, and to the end with a depth past the count.
- Append when the depth is past the end, including depth greater than 0 on an empty position.
- The same player id at two positions, and on two service instances for two sports. Removal on one instance leaves the other unchanged.
- Full chart order for NFL and MLB, including KR added before WR, and omission of empty positions.
- Remove the top, middle, and last player. Close the gap. Drop a position that becomes empty.
- Players below the top, a middle player, and the last player.
- Identity: a different instance with the same `PlayerId` matches. The same name with a different id does not.
- Rejections: null player, blank name, null position, wrong sport, wrong capitalisation, negative depth, remove of a player who is not there, players-below for a player who is not there. After each rejection the previous ranking is still intact.
- A custom `SportDefinition` with positions that are neither NFL nor MLB uses the same shift, append, and move outcomes. An NFL code is rejected on that chart.

The test project references xUnit only. It does not add an assertion library. GitHub Actions restores, builds with warnings as errors, and runs `dotnet test`. A warning or a failed test fails the job. The workflow file is part of implementation, not part of this plan's source output.

## Extensibility

`DepthChartService` receives a `SportDefinition`. It asks that object which positions exist and in what published order. It does not contain `if (sport == "NFL")` branches.

Adding a sport means constructing another `SportDefinition` with a name and an ordered position list, then constructing a service with it. The four operations and the dictionary layout stay the same. NFL and MLB are two static definitions in the library so callers do not retype the position lists. They are data, not subclasses.

The approved sports remain NFL and MLB. The custom-sport test proves the extension point. It does not add a third supported sport to the product.

## Design trade-offs

| Choice | Alternative | Why this plan chooses it |
| --- | --- | --- |
| `SportDefinition` data | A subclass per sport | The sports differ by position names and order, not by ranking rules. A subclass would add types with no behaviour. |
| `IDepthChartService` plus one class | Concrete class only | The approved architecture asks for a public contract. The justification is in Complexity Tracking. The algorithm is not virtual and not overridden per sport. |
| `Dictionary<string, List<Player>>` | `LinkedList<Player>`, or a player-id index | Depth is an index and shifts must preserve order. A linked list does not make index inserts cheaper. An id index is extra state for rankings of about 20 players. |
| Store only populated positions | Pre-create every sport position | The full chart omits empty positions. Missing keys represent that directly. Published order still comes from `SportDefinition`. |
| Return player ids | Return live `Player` lists | The specification's observable result is the player id order. Copies prevent callers from mutating the chart. |
| Exceptions for rejection | A result object with an error flag | The planning assumptions name `ArgumentNullException`, `ArgumentException`, and `ArgumentOutOfRangeException`. The chart stays unchanged because validation finishes before mutation. |
| One service instance per chart | A registry of many charts inside one service | The specification's chart belongs to one sport. Two sports are two instances. A registry would be storage the specification does not require. |
| O(n) scans and shifts | A structure tuned for large n | No measured need exists. The constitution forbids that optimisation before a specification records one. |

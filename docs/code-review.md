# Code Review: Depth Chart Management

Review date: 31 October 2026. Scope is the class library, the xUnit suite, the challenge, the constitution, `specs/001-manage-depth-chart`, and the CI workflow. No code was changed for this review.

Severity:

| Level | Meaning |
| --- | --- |
| High | A specified outcome is wrong, or a public call throws an exception the contract does not name. |
| Medium | The chart result is right, but a contract rule, shared immutable value, or submission deliverable is wrong. |
| Low | A gap a later change could miss. The current challenge sample still passes. |

## Assessment

The original four use cases are met. The published sequence returns WR `[2, 1, 3]` and KR `[1]`, and players below Alice at WR are `[1, 3]`. NFL and MLB are data on `SportDefinition`. A further sport is another position list. There is no database, API, or UI. Storage is `Dictionary<string, List<Player>>`. Full-chart order follows the sport list, so dictionary order cannot change it.

Add, shift, append, move, remove, and players-below match the specification for valid players. Identity is `PlayerId`. The same id at two positions, and on two services, stays independent for the cases the tests exercise. Complexity matches the plan: O(n) per position for insert, move, and remove, and O(P + N) for a full chart. A second index would be premature at the specified size of about 20 players. There is no performance defect.

`Player`, `SportDefinition`, `DepthChartRow`, and `DepthChartService` are small and cohesive. Open-Closed is the position list, which is the right extension point. `IDepthChartService` has one implementation. The plan records that as a justified constitution exception. It is a contract, and it does not branch by sport.

The suite (81 tests) covers the sample, shift, append, move, removal, full-chart order, players-below, NFL, MLB, and a custom sport. It does not cover the validation order in the plan.

## High

### H-1. Null player is not validated first, and players-below can throw `NullReferenceException`

FR-018, the spec edge cases, and the plan require this order, before any mutation: null player (`ArgumentNullException`), blank name (`ArgumentException`), invalid position (`ArgumentException`), negative depth (`ArgumentOutOfRangeException`).

`AddPlayer` checks position and depth first, and never checks the player. Observed results:

| Call | Required | Actual |
| --- | --- | --- |
| Null player, position `SP` | `ArgumentNullException` | `ArgumentException` (position) |
| Null player, depth `-1` | `ArgumentNullException` | `ArgumentOutOfRangeException` |
| `GetPlayersBelow` with a null player at a position that has a ranking | `ArgumentNullException`, chart unchanged | `NullReferenceException` on `PlayerId` |
| `RemovePlayer` with a null player when that position has no ranking | `ArgumentNullException` | `ArgumentException` ("not ranked") |

A null player on add at a valid position happens to throw `ArgumentNullException` from `List<Player>.Remove`. That is the list rejecting null, after `GetOrCreateRanking` has already stored a list. It does not show that the service check runs first.

Blank names cannot be constructed. `Player` is sealed and already rejects null, empty, and whitespace names. A service-level blank-name test is not reachable through the public type. The null-player paths are reachable with a null argument and are the defect.

## Medium

### M-1. A rejected null add can leave an empty ranking in the dictionary

When the position is valid and the depth is non-negative, `AddPlayer` creates the position list and then `List.Remove` throws on null. The plan says a rejected call does not mutate, and an empty position has no dictionary key. `GetFullDepthChart` skips empty lists, so a later full-chart read still omits that position. The private state rule is already broken.

### M-2. Null position on remove and players-below throws `ArgumentNullException`

Add rejects a null position with `ArgumentException`, as FR-017 and the README assumptions require for add, remove, and players-below. Remove and players-below pass the position to `Dictionary.TryGetValue`, which throws `ArgumentNullException`. The chart is unchanged. Callers written to the contract will not catch the specified exception.

An unknown code such as `SP` on remove still throws `ArgumentException`. The exception type matches "not ranked", so that case is a weaker gap and is listed under L-4.

### M-3. `SportDefinition.Positions` is the live internal list

The constitution and the plan require immutable output or a defensive copy. The constructor copies the caller's list, and that copy is tested. `Positions` then returns that internal `List<string>` as `IReadOnlyList<string>`. A cast can insert or replace a code. `Contains` and full-chart order follow that list. `SportDefinition.Nfl` and `SportDefinition.Mlb` are process-wide singletons, so one mutation changes every chart built from them.

Chart id lists are copies. Mutating a returned full-chart or players-below list does not change the next read. That part of the contract holds. The position list does not.

### M-4. The submission README does not say how to build or test

The challenge requires build and run instructions, plus assumptions, in the root `README.md`. Assumptions are present and match the specification. There is no `dotnet test` command, no .NET 10 prerequisite, no NFL or MLB position list, and no statement of what belongs in the submission zip. Tasks T064 and T065 are that documentation. The workflow in `.github/workflows/ci.yml` does build and test with warnings as errors, so the missing text is a deliverable gap, not a broken build.

## Low

### L-1. Specified edges with no test

The implementation behaves correctly for these. Nothing in the suite fails if a later edit breaks them.

- Player id `0` ranked on a chart. Construction allows `0`. No chart test uses it.
- A natural position from another sport, such as `SP` on NFL `WR` (FR-006). The service does not consult natural position. The custom-sport test uses a null natural position only.
- Moving a player at one position while that player is also ranked at another (FR-005). Removal across positions is tested. A move is not.
- Players-below on an empty chart. The missing-player test uses a `WR` list that already exists. A missing key is the other branch, and it is the branch most likely to return an empty list instead of `ArgumentException` (FR-020).
- A ranking of 20 players after insert at depth 0 and removal at depth 0 (SC-006). `List<T>` already preserves order. This is a scale check, not a new rule.

### L-2. `IDepthChartService` has a single implementation

Principle III says an abstraction with one implementation and no variation should not exist. The plan's complexity table keeps the interface as the public contract and keeps sport variation on `SportDefinition`. That trade-off is explicit. Removing the interface would be a plan change, not a cleanup.

### L-3. Moq is an unused package

No test creates a mock. Principle VIII says a dependency must earn its place. The plan still lists Moq for a future test that isolates a dependency. Until such a test exists, the package is unused weight.

### L-4. Remove and players-below do not run the position check

Unknown and wrong-case codes throw `ArgumentException` with the "not ranked" message. The public type matches the contract, so this is not a separate functional break. Null position is the case that changes the type (M-2). A test that only asserts `ArgumentException` for `SP` on remove would pass either way.

### L-5. `==` is reference equality; `Equals` is player id

`IEquatable<Player>` and `GetHashCode` use `PlayerId`. The operators are not overloaded. Chart remove and lookup use `Equals`, so rankings are correct. A caller who writes `==` will treat two records with the same id as different objects. That matches the decision to leave reference comparison unchanged. It is worth knowing at the boundary.

### L-6. Spec Kit status does not match the code

`tasks.md` leaves completed work unchecked, including behaviour that is already green, and also leaves T034–T036 and the null cases in T046 open, which matches H-1 and M-2. The constitution file still carries the Spec Kit sync-impact comment above the ratified text. Neither changes runtime behaviour. Both make the next review read a stale trail.

## Not defects

- The challenge's "print the full chart" is a returned list of rows. The specification says the sample describes the data, not a console or a screen.
- Omitted depth, depth past the end, and moving an existing player are specified assumptions. The code and the README agree with FR-013 and FR-014.
- There is no sport `switch` in `DepthChartService`. Adding a sport is a new `SportDefinition`.
- O(n) shifts are the appropriate cost. The plan rejects a player-to-depth index until a measured need exists. No such need is specified.

## Suggested order of work

1. Reject a null player with `ArgumentNullException` before position, depth, or dictionary access, on add, remove, and players-below. Confirm the chart is unchanged.
2. Reject a null position with `ArgumentException` on remove and players-below.
3. Return a read-only copy from `SportDefinition.Positions`.
4. Add the L-1 tests that protect id `0`, natural position, a cross-position move, and players-below on an empty chart.
5. Update the root README with build, test, position lists, and submission contents.

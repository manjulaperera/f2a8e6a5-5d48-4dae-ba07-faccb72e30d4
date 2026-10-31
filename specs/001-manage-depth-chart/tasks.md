---

description: "Task list for incremental depth chart implementation"
---

# Tasks: Manage Depth Chart

**Input**: Design documents from `/specs/001-manage-depth-chart/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Required. Every functional behaviour follows Red-Green-Refactor. Tests are written before the production code they describe.

**Organization**: Tasks are grouped by user story. Inside each story, behaviours follow the approved implementation order. Each behaviour is three tasks, and those three tasks are the commit boundaries:

1. **RED**: Write the failing test. Run it and confirm it fails for the expected reason.
2. **GREEN**: Write the minimum production code. Run it and confirm that test passes.
3. **REFACTOR**: Clean the code without changing observable behaviour. Run the whole suite and confirm it passes.

Do not commit from this task list until the developer asks. When a commit is requested, one task is one commit. Do not combine RED, GREEN, and REFACTOR into one commit.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- Library: `src/DepthChart/`
- Tests: `tests/DepthChart.Tests/`
- Solution: `DepthChart.sln` at the repository root
- CI: `.github/workflows/ci.yml`
- Paths match `plan.md`

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Solution and test project scaffolding. No chart behaviour yet.

- [ ] T001 Create `DepthChart.sln` and `src/DepthChart/DepthChart.csproj` for `net10.0` with nullable reference types enabled, implicit usings enabled, warnings treated as errors, and no package references
  - **Acceptance**: `dotnet build src/DepthChart/DepthChart.csproj` succeeds with no warnings. The project is a class library. It has no dependency on a database, web framework, or UI toolkit.
- [ ] T002 Create `tests/DepthChart.Tests/DepthChart.Tests.csproj` for `net10.0`, reference `src/DepthChart/DepthChart.csproj`, and reference only xUnit, Microsoft.NET.Test.Sdk, and xunit.runner.visualstudio. Add the project to `DepthChart.sln`
  - **Acceptance**: `dotnet test DepthChart.sln` succeeds. The test project has no tests yet and no assertion library beyond xUnit. Nullable reference types and warnings-as-errors match the library.
- [ ] T003 Add `Directory.Build.props` at the repository root so both projects keep nullable reference types, implicit usings, and warnings as errors
  - **Acceptance**: `dotnet test DepthChart.sln` still succeeds. Removing those settings from a project file does not turn the settings off while `Directory.Build.props` is present.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: `Player` and `SportDefinition`. No user story work starts before this checkpoint.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Player domain model

- [ ] T004 Write failing tests in `tests/DepthChart.Tests/PlayerTests.cs` for an immutable `Player` whose `PlayerId` is a non-nullable `int` (`0` is allowed), whose `Name` is a `string`, and whose `NaturalPosition` is a `string` or null
  - **Acceptance**: `dotnet test DepthChart.sln --filter FullyQualifiedName~PlayerTests` fails because `Player` does not exist. The failure is a compile error, not an assertion about ranking.
- [ ] T005 Implement the minimum `Player` type in `src/DepthChart/Player.cs` so `tests/DepthChart.Tests/PlayerTests.cs` passes. Do not add chart storage or sport rules
  - **Acceptance**: `dotnet test DepthChart.sln --filter FullyQualifiedName~PlayerTests` passes. `PlayerId`, `Name`, and `NaturalPosition` can be read after construction. The test project does not need a service.
- [ ] T006 Refactor `src/DepthChart/Player.cs` and `tests/DepthChart.Tests/PlayerTests.cs` without changing the observable fields
  - **Acceptance**: `dotnet test DepthChart.sln` passes. A reviewer can see that `Player` does not reference a chart, a sport list, or a collection.

### Sport configuration and validation

- [ ] T007 Write failing tests in `tests/DepthChart.Tests/SportDefinitionTests.cs` that a definition preserves the supplied position order, that `Contains` is true only for an exact code, that `wr` is not `WR`, that null is not contained, and that a duplicate code in one definition is invalid configuration
  - **Acceptance**: `dotnet test DepthChart.sln --filter FullyQualifiedName~SportDefinitionTests` fails because `SportDefinition` does not exist.
- [ ] T008 Implement the minimum `SportDefinition` in `src/DepthChart/SportDefinition.cs` for a caller-supplied name and ordered position list. Do not add the NFL or MLB static lists in this task
  - **Acceptance**: `dotnet test DepthChart.sln --filter FullyQualifiedName~SportDefinitionTests` passes. Position order is the list order given to the definition. `Contains` rejects a different capitalisation and null.
- [ ] T009 Refactor `src/DepthChart/SportDefinition.cs` and `tests/DepthChart.Tests/SportDefinitionTests.cs` without changing contains or order results
  - **Acceptance**: `dotnet test DepthChart.sln` passes. `SportDefinition` stores no players.

**Checkpoint**: Foundation ready. User story implementation can now begin.

---

## Phase 3: User Story 1 - Add a player to a depth chart (Priority: P1) 🎯 MVP

**Goal**: Place a player on one position, append when depth is omitted or past the end, shift players when the depth is occupied, and move a player who is already at that position.

**Independent Test**: On a new service, add players with depth 0, with no depth, at an occupied depth, and past the end. Read the player ids back in depth order. Move an existing player and confirm that player appears once.

### Add the first player

- [ ] T010 [US1] Write failing tests in `tests/DepthChart.Tests/AddPlayerTests.cs` that adding Bob (`PlayerId` 1) at `WR` depth 0 on an empty chart ranks Bob at depth 0, and that omitting the depth on an empty chart also ranks that player at depth 0
  - **Acceptance**: `dotnet test DepthChart.sln --filter FullyQualifiedName~AddPlayerTests` fails because the service types do not exist or the ranking is absent. No production ranking code is added in this task.
- [ ] T011 [US1] Implement the minimum `AddPlayer` and read path in `src/DepthChart/IDepthChartService.cs`, `src/DepthChart/DepthChartService.cs`, and `src/DepthChart/DepthChartRow.cs` so the first player is stored in a private `Dictionary<string, List<Player>>` and returned by player id
  - **Acceptance**: The first-player tests pass. The dictionary key is the position code and index 0 is depth 0. Append, mid-list insert, and move are not implemented beyond what the first-player tests require.
- [ ] T012 [US1] Refactor `src/DepthChart/DepthChartService.cs` and `tests/DepthChart.Tests/AddPlayerTests.cs` without changing the first-player ids
  - **Acceptance**: `dotnet test DepthChart.sln` passes. The returned id list is a copy, not the internal `List<Player>`.

### Append a player

- [ ] T013 [US1] Write failing tests in `tests/DepthChart.Tests/AddPlayerTests.cs` that omitting the depth appends after Alice then Bob, and that Charlie (`PlayerId` 3) added at `WR` depth 2 when two players are already ranked is appended
  - **Acceptance**: The new tests fail for the expected reason: the player is not last, or the depth-equal-to-count case is rejected or dropped. Existing first-player tests still pass.
- [ ] T014 [US1] Implement append in `src/DepthChart/DepthChartService.cs` for an omitted depth and for a depth equal to the current count when that player id is not already at the position
  - **Acceptance**: The append tests pass. The list has no empty slot between players. Earlier add tests still pass.
- [ ] T015 [US1] Refactor the append path in `src/DepthChart/DepthChartService.cs` and `tests/DepthChart.Tests/AddPlayerTests.cs` without changing append order
  - **Acceptance**: `dotnet test DepthChart.sln` passes.

### Insert a player at an occupied depth

- [ ] T016 [US1] Write failing tests in `tests/DepthChart.Tests/AddPlayerTests.cs` that Alice (`PlayerId` 2) added at `WR` depth 0 after Bob moves Bob to depth 1, and that Dana added at depth 1 of Alice, Bob, Charlie yields Alice, Dana, Bob, Charlie
  - **Acceptance**: The new tests fail because the new player does not take the requested depth or the tail does not keep its relative order.
- [ ] T017 [US1] Implement occupied-depth insert in `src/DepthChart/DepthChartService.cs` so the new player occupies that depth and every player already at that depth and below shifts down one
  - **Acceptance**: The occupied-depth tests pass. Relative order of the shifted players is unchanged. The player being inserted was not already at that position.
- [ ] T018 [US1] Refactor the insert path in `src/DepthChart/DepthChartService.cs` and `tests/DepthChart.Tests/AddPlayerTests.cs` without changing shift results
  - **Acceptance**: `dotnet test DepthChart.sln` passes.

### Insert a player at an arbitrary depth

- [ ] T019 [US1] Write failing tests in `tests/DepthChart.Tests/AddPlayerTests.cs` that a depth greater than the current count appends Dana after Alice, Bob, Charlie when the depth is 10, and that depth 5 on an empty position ranks that player at depth 0, with no empty depths created
  - **Acceptance**: The new tests fail because the call throws, creates a gap, or does not place the player last.
- [ ] T020 [US1] Implement past-the-end placement in `src/DepthChart/DepthChartService.cs` by appending when the player is not already at that position and the depth is greater than the current count
  - **Acceptance**: The arbitrary-depth tests pass. A depth equal to the count still appends. No unoccupied index is stored.
- [ ] T021 [US1] Refactor past-the-end handling in `src/DepthChart/DepthChartService.cs` and `tests/DepthChart.Tests/AddPlayerTests.cs` without changing those ids
  - **Acceptance**: `dotnet test DepthChart.sln` passes.

### Move an existing player

- [ ] T022 [US1] Write failing tests in `tests/DepthChart.Tests/AddPlayerTests.cs` that Bob already in Alice, Bob, Charlie and added again at depth 0 becomes Bob, Alice, Charlie once; Dana moved from the end to depth 1 becomes Alice, Dana, Bob, Charlie once; Alice added with no depth becomes last once; Alice added at depth 10 becomes last once; Bob added again at his current depth leaves the ranking unchanged
  - **Acceptance**: The new tests fail because a second entry appears, the destination depth is wrong, or the other players change relative order.
- [ ] T023 [US1] Implement the move in `src/DepthChart/DepthChartService.cs`: one entry remains, other players keep relative order, a depth from 0 through one less than the current count is the destination, and an omitted depth or a depth greater than or equal to the current count places the player last
  - **Acceptance**: The move tests pass. The list length does not grow. Matching uses `PlayerId`, not object reference.
- [ ] T024 [US1] Refactor the move path in `src/DepthChart/DepthChartService.cs` and `tests/DepthChart.Tests/AddPlayerTests.cs` without changing move results
  - **Acceptance**: `dotnet test DepthChart.sln` passes. Adding at the player's current depth is still a no-op ranking change.

**Checkpoint**: User Story 1 add behaviour is testable on its own.

---

## Phase 4: User Story 2 - Remove a player from a depth chart (Priority: P2)

**Goal**: Remove a player from one position, close the gap, and leave every other position unchanged.

**Independent Test**: Start from three or more players, remove the top, middle, or last player, and read that position back. Remove a player from only one of two positions.

- [ ] T025 [US2] Write failing tests in `tests/DepthChart.Tests/RemovePlayerTests.cs` that removing Bob from Alice, Dana, Bob, Charlie leaves Alice, Dana, Charlie at depths 0, 1, and 2; removing the first or last player preserves the others; removing Bob from `WR` leaves Bob at `KR`; removing the only player at `KR` leaves `KR` with no players
  - **Acceptance**: The new tests fail because `RemovePlayer` is missing or the gap remains. Add tests from Phase 3 still pass.
- [ ] T026 [US2] Implement `RemovePlayer` in `src/DepthChart/IDepthChartService.cs` and `src/DepthChart/DepthChartService.cs` so the matching `PlayerId` is removed, later players stay in order at contiguous depths from 0, and an empty position drops its dictionary key
  - **Acceptance**: The remove tests pass. A different position that still holds that `PlayerId` is unchanged.
- [ ] T027 [US2] Refactor `src/DepthChart/DepthChartService.cs` and `tests/DepthChart.Tests/RemovePlayerTests.cs` without changing removal order
  - **Acceptance**: `dotnet test DepthChart.sln` passes.

**Checkpoint**: User Stories 1 and 2 both work.

---

## Phase 5: User Story 3 - Retrieve the full depth chart (Priority: P3)

**Goal**: Return populated positions in the sport's published order, with player ids from depth 0 downward.

**Independent Test**: Reproduce the published sample and read the full chart. Add `KR` before `WR` and still receive `WR` then `KR`. An empty chart returns an empty collection.

- [ ] T028 [US3] Write failing tests in `tests/DepthChart.Tests/FullChartTests.cs` for the published sample (`WR` ids 2, 1, 3 then `KR` id 1), for `KR` added before `WR` still returning `WR` then `KR`, for omission of `QB`, `RB`, `TE`, `K`, `P`, and `PR` when only `WR` and `KR` are filled, and for an empty chart returning an empty collection
  - **Acceptance**: The new tests fail because row order follows dictionary order, empty positions appear, or the sample ids differ. The expected sample rows are `WR: 2, 1, 3` and `KR: 1`.
- [ ] T029 [US3] Implement `GetFullDepthChart` in `src/DepthChart/DepthChartService.cs` by walking `SportDefinition` published order and copying ids only for positions that have at least one player
  - **Acceptance**: The full-chart tests pass. Returned lists are copies. Names are not required in the row result. Dictionary enumeration order does not decide row order.
- [ ] T030 [US3] Refactor `src/DepthChart/DepthChartRow.cs`, `src/DepthChart/DepthChartService.cs`, and `tests/DepthChart.Tests/FullChartTests.cs` without changing row order or ids
  - **Acceptance**: `dotnet test DepthChart.sln` passes.

**Checkpoint**: The full chart matches the published sample and the published position order.

---

## Phase 6: User Story 4 - Retrieve players ranked below a player (Priority: P4)

**Goal**: Return the player ids strictly below a named player at one position, in rank order.

**Independent Test**: With `WR` ranked Alice (2), Bob (1), Charlie (3), players below Alice are 1 then 3, below Bob are 3, and below Charlie are none.

- [ ] T031 [US4] Write failing tests in `tests/DepthChart.Tests/PlayersBelowTests.cs` for those three results, and for a player below Bob only at `KR` being excluded when the question is `WR`
  - **Acceptance**: The new tests fail because `GetPlayersBelow` is missing, includes the named player, or includes another position.
- [ ] T032 [US4] Implement `GetPlayersBelow` in `src/DepthChart/IDepthChartService.cs` and `src/DepthChart/DepthChartService.cs` to return a copied id list strictly after the matched `PlayerId`
  - **Acceptance**: The players-below tests pass. The last player returns an empty list. The chart is unchanged by the read.
- [ ] T033 [US4] Refactor `src/DepthChart/DepthChartService.cs` and `tests/DepthChart.Tests/PlayersBelowTests.cs` without changing those id lists
  - **Acceptance**: `dotnet test DepthChart.sln` passes.

**Checkpoint**: All four chart operations work for the happy path.

---

## Phase 7: Validate edge cases

**Purpose**: Rejection paths. Each rejected call leaves every ranking unchanged. Checks run in this order: null player, blank name, invalid position, negative depth, then a player id that is not ranked at that position.

### Null player and blank name

- [ ] T034 [US1] Write failing tests in `tests/DepthChart.Tests/ValidationTests.cs` that a null player on add throws `ArgumentNullException`, and that a null, empty, or whitespace-only `Name` throws `ArgumentException`, with the previous ranking unchanged
  - **Acceptance**: The new tests fail because the exception type differs or the chart changes. `Name` must not be null, empty, or whitespace.
- [ ] T035 [US1] Implement those checks in `src/DepthChart/DepthChartService.cs` before any dictionary mutation
  - **Acceptance**: The null-player and blank-name tests pass. A valid player is still added.
- [ ] T036 [US1] Refactor the player checks in `src/DepthChart/DepthChartService.cs` and `tests/DepthChart.Tests/ValidationTests.cs` without changing the exception types
  - **Acceptance**: `dotnet test DepthChart.sln` passes.

### Invalid positions

- [ ] T037 [US1] Write failing tests in `tests/DepthChart.Tests/ValidationTests.cs` that a null position, `SP` on an NFL chart, an unknown code, and `wr` each throw `ArgumentException` and leave the chart unchanged
  - **Acceptance**: The new tests fail because the position is accepted or the exception type differs. `Contains` is true only for an exact code.
- [ ] T038 [US1] Implement position checks in `src/DepthChart/DepthChartService.cs` after the player checks and before mutation
  - **Acceptance**: The invalid-position tests pass. `WR` is still accepted on an NFL chart.
- [ ] T039 [US1] Refactor the position checks in `src/DepthChart/DepthChartService.cs` and `tests/DepthChart.Tests/ValidationTests.cs` without accepting a non-exact code
  - **Acceptance**: `dotnet test DepthChart.sln` passes.

### Negative depth

- [ ] T040 [US1] Write failing tests in `tests/DepthChart.Tests/ValidationTests.cs` that a depth below 0 throws `ArgumentOutOfRangeException` on an empty chart and when Bob is already ranked at `WR`, and that Bob's ranking stays in place
  - **Acceptance**: The new tests fail because the call succeeds, a different exception is thrown, or the existing ranking moves.
- [ ] T041 [US1] Implement the negative-depth check in `src/DepthChart/DepthChartService.cs` before mutation
  - **Acceptance**: The negative-depth tests pass. Depth 0 still ranks the first player.
- [ ] T042 [US1] Refactor the depth check in `src/DepthChart/DepthChartService.cs` and `tests/DepthChart.Tests/ValidationTests.cs` without changing the exception
  - **Acceptance**: `dotnet test DepthChart.sln` passes.

### Remove a player who is not ranked there

- [ ] T043 [US2] Write failing tests in `tests/DepthChart.Tests/RemovePlayerTests.cs` that removing a different player from a `WR` ranking of Alice throws `ArgumentException`, that removing Bob from `WR` when Bob is only at `KR` throws `ArgumentException` and leaves `KR` unchanged, and that removing from an empty chart throws `ArgumentException`
  - **Acceptance**: The new tests fail if the removal succeeds, returns without an exception, or changes a ranking. A no-op success is a failure.
- [ ] T044 [US2] Implement the missing-player check in `RemovePlayer` in `src/DepthChart/DepthChartService.cs` so the dictionary is unchanged when the id is absent
  - **Acceptance**: The missing-remove tests pass. Removing a player who is present still closes the gap.
- [ ] T045 [US2] Refactor the missing-remove path in `src/DepthChart/DepthChartService.cs` and `tests/DepthChart.Tests/RemovePlayerTests.cs` without turning it into a no-op success
  - **Acceptance**: `dotnet test DepthChart.sln` passes.

### Players below a player who is not ranked there

- [ ] T046 [US4] Write failing tests in `tests/DepthChart.Tests/PlayersBelowTests.cs` that asking for players below Alice when Alice is not at `WR` throws `ArgumentException` and does not return an empty list, and that a null player or a null position uses the same exception order as add
  - **Acceptance**: The new tests fail if the result is an empty list or the chart changes.
- [ ] T047 [US4] Implement the missing-player check in `GetPlayersBelow` in `src/DepthChart/DepthChartService.cs`
  - **Acceptance**: The missing-lookup tests pass. The last ranked player still produces an empty list.
- [ ] T048 [US4] Refactor the lookup checks in `src/DepthChart/DepthChartService.cs` and `tests/DepthChart.Tests/PlayersBelowTests.cs` without changing the last-player empty result
  - **Acceptance**: `dotnet test DepthChart.sln` passes.

### Player identity

- [ ] T049 [US2] Write failing tests in `tests/DepthChart.Tests/RemovePlayerTests.cs` that a different instance with Bob's `PlayerId` 1 and name Robert removes Bob from `WR`, and that player id 4 named Alex does not remove Alice (`PlayerId` 2, name Alex)
  - **Acceptance**: The new tests fail if reference equality keeps Bob on the chart or the shared name removes Alice.
- [ ] T050 [US2] Match removals by `PlayerId` value in `src/DepthChart/DepthChartService.cs`
  - **Acceptance**: The identity remove tests pass. `0` remains a valid `PlayerId` when a test uses it.
- [ ] T051 [US2] Refactor id matching in `src/DepthChart/DepthChartService.cs` and `tests/DepthChart.Tests/RemovePlayerTests.cs` without comparing players by reference
  - **Acceptance**: `dotnet test DepthChart.sln` passes.
- [ ] T052 [US4] Write failing tests in `tests/DepthChart.Tests/PlayersBelowTests.cs` that a different instance with `PlayerId` 1 returns Charlie's id 3 when Bob is ahead of Charlie, and that `PlayerId` 4 named Alex does not match Alice at `WR`
  - **Acceptance**: The new tests fail if the lookup uses object reference or matches on name.
- [ ] T053 [US4] Match players-below by `PlayerId` value in `src/DepthChart/DepthChartService.cs`
  - **Acceptance**: The identity lookup tests pass. A missing id still throws `ArgumentException` rather than returning an empty list.
- [ ] T054 [US4] Refactor lookup matching in `src/DepthChart/DepthChartService.cs` and `tests/DepthChart.Tests/PlayersBelowTests.cs` without changing the returned ids
  - **Acceptance**: `dotnet test DepthChart.sln` passes.

**Checkpoint**: Invalid input is rejected and the chart from the previous successful call is still intact.

---

## Phase 8: Verify NFL and MLB behaviour

**Goal**: The static NFL and MLB definitions match the specification, and both sports use the same ranking rules.

**Independent Test**: Add one player at every NFL position and every MLB position. An NFL chart rejects `SP`. An MLB chart of `C` and `CF` returns `C` then `CF`.

- [ ] T055 [US3] Write failing tests in `tests/DepthChart.Tests/NflMlbTests.cs` that `SportDefinition.Nfl` positions are exactly `QB, WR, RB, TE, K, P, KR, PR` and `SportDefinition.Mlb` positions are exactly `SP, RP, C, 1B, 2B, 3B, SS, LF, RF, CF, DH`, that one player can be ranked at each of those positions on a chart for that sport, that `SP` is rejected on NFL, and that an MLB chart with only `CF` and `C` returns `C` then `CF`
  - **Acceptance**: The new tests fail because the static NFL and MLB definitions are absent. Do not hard-code a second ranking algorithm in the test.
- [ ] T056 [US3] Add `SportDefinition.Nfl` and `SportDefinition.Mlb` in `src/DepthChart/SportDefinition.cs` using those exact position lists. Do not add sport branches to `src/DepthChart/DepthChartService.cs`
  - **Acceptance**: `dotnet test DepthChart.sln --filter FullyQualifiedName~NflMlbTests` passes. The service still asks `SportDefinition` which positions exist and in what order.
- [ ] T057 [US3] Refactor `src/DepthChart/SportDefinition.cs` and `tests/DepthChart.Tests/NflMlbTests.cs` without changing the NFL or MLB position lists
  - **Acceptance**: `dotnet test DepthChart.sln` passes.

**Checkpoint**: NFL and MLB are configuration. The ranking code is still sport-agnostic.

---

## Phase 9: User Story 5 - Integration and another sport (Priority: P5)

**Goal**: Prove the four operations together, including the published sample, two charts, and a sport that is neither NFL nor MLB.

**Independent Test**: Run the published sample end to end. Build a chart whose positions are `GK` and `ST`, shift and append there, reject `WR`, and remove a player from an NFL chart without changing an MLB chart that holds the same player id.

- [ ] T058 [US5] Write failing tests in `tests/DepthChart.Tests/IntegrationTests.cs` for the published sample, for `GK` then a second player at `GK` depth 0 ranking the second player first, for rejecting `WR` on that sport, and for the same `PlayerId` on an NFL service and an MLB service surviving removal from only the NFL service
  - **Acceptance**: The new tests fail if the sample is not `WR` 2, 1, 3 and `KR` 1, if players below Alice are not 1 then 3, or if the MLB chart changes when the NFL chart removes that id. Add the tests before changing production code.
- [ ] T059 [US5] Change `src/DepthChart/DepthChartService.cs` only if `tests/DepthChart.Tests/IntegrationTests.cs` is failing. The change must be the minimum that makes those tests pass
  - **Acceptance**: The integration tests pass. If they were already green, production files are unchanged. A custom `SportDefinition` needs no new service type.
- [ ] T060 [US5] Refactor `tests/DepthChart.Tests/IntegrationTests.cs` and any touched lines in `src/DepthChart/DepthChartService.cs` without changing the sample ids
  - **Acceptance**: `dotnet test DepthChart.sln` passes. Tests do not share a mutable service instance.

**Checkpoint**: The specification's cross-sport scenarios pass on the public contract.

---

## Phase 10: Polish - CI pipeline

**Purpose**: GitHub Actions runs the same test gate as a local `dotnet test`.

- [ ] T061 Add `.github/workflows/ci.yml` that checks out the repo, sets up the .NET 10 SDK, and runs `dotnet test DepthChart.sln` with warnings treated as errors on push and pull request
  - **Acceptance**: The workflow does not deploy, start a database, or start a web server. A failing test or a compiler warning fails the job. Local `dotnet test DepthChart.sln` still passes.

---

## Phase 11: Polish - Refactor and review

**Purpose**: Clean the library after the behaviours are green, then review it against the specification.

- [ ] T062 Refactor duplication in `src/DepthChart/DepthChartService.cs`, `src/DepthChart/Player.cs`, and `src/DepthChart/SportDefinition.cs` while keeping the public contract in `specs/001-manage-depth-chart/contracts/depth-chart.md`
  - **Acceptance**: `dotnet test DepthChart.sln` passes. Observable ids, order, and exception types are unchanged. No second index or collection type replaces `Dictionary<string, List<Player>>`.
- [ ] T063 Review `src/DepthChart/` and `tests/DepthChart.Tests/` against `specs/001-manage-depth-chart/spec.md` and `specs/001-manage-depth-chart/plan.md`
  - **Acceptance**: A human reviewer confirms the four operations, the validation order, NFL, MLB, and the in-memory boundary. Review notes are resolved or written down before the next task. No API, database, UI, or extra package is present.

---

## Phase 12: Polish - Documentation and submission

**Purpose**: Root instructions a reviewer can follow without the chat history.

- [ ] T064 Update `README.md` with build and test instructions for the .NET 10 solution, the NFL and MLB position lists, and a pointer to the assumptions already recorded there
  - **Acceptance**: From a clean tree, the README's commands match `specs/001-manage-depth-chart/quickstart.md`: restore and `dotnet test DepthChart.sln`. The README does not tell the reader to start a database, API, or UI.
- [ ] T065 Document the submission contents in `README.md`: source, tests, workflow, and docs, excluding build outputs such as `bin/` and `obj/`
  - **Acceptance**: A reader can assemble the review package from the README. The task does not add production ranking code and does not create a commit.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies. Start immediately.
- **Foundational (Phase 2)**: Depends on Setup. Blocks every user story.
- **User Story 1 (Phase 3)**: Depends on Foundational. This is the MVP.
- **User Story 2 (Phase 4)**: Depends on User Story 1, because removal tests build a ranking with add.
- **User Story 3 (Phase 5)**: Depends on User Story 1. Published order can be verified as soon as add works.
- **User Story 4 (Phase 6)**: Depends on User Story 1.
- **Edge cases (Phase 7)**: Depends on Phases 3 through 6 so each rejection can show an unchanged successful ranking.
- **NFL and MLB (Phase 8)**: Depends on Foundational sport configuration and on full-chart order from Phase 5.
- **Integration (Phase 9)**: Depends on Phases 3 through 8.
- **CI, refactor, documentation (Phases 10-12)**: Depend on Phase 9.

### User Story Dependencies

- **User Story 1 (P1)**: Starts after Phase 2. Does not need remove, full published order, or players-below beyond a minimal read of ids.
- **User Story 2 (P2)**: Starts after the add path can build a chart. Independently testable by arranging a ranking and removing.
- **User Story 3 (P3)**: Starts after add. Independently testable by reading rows.
- **User Story 4 (P4)**: Starts after add. Independently testable by reading the ids below one player.
- **User Story 5 (P5)**: Starts after the four operations and the NFL and MLB definitions exist.

### Within Each Behaviour

- RED task before GREEN task before REFACTOR task.
- The RED test must fail for the expected reason before production code for that behaviour is written.
- Do not weaken a test to make an incorrect implementation pass.
- Refactor only while the suite is green.

### Parallel Opportunities

- No `[P]` tasks. `DepthChartService.cs` is one file, and each GREEN task depends on the previous behaviour.
- `PlayerTests.cs` and `SportDefinitionTests.cs` are different files, but the approved order finishes `Player` before `SportDefinition`.
- Do not start User Story 2, 3, or 4 in parallel with an unfinished add task. They share the service file.

---

## Parallel Example: User Story 1

```text
User Story 1 is sequential:

T010 RED first player
T011 GREEN first player
T012 REFACTOR first player
T013 RED append
T014 GREEN append
T015 REFACTOR append
T016 RED occupied depth
T017 GREEN occupied depth
T018 REFACTOR occupied depth
T019 RED arbitrary depth
T020 GREEN arbitrary depth
T021 REFACTOR arbitrary depth
T022 RED move
T023 GREEN move
T024 REFACTOR move
```

There is no parallel launch for User Story 1. Each task uses the ranking produced by the task before it.

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup.
2. Complete Phase 2: Foundational.
3. Complete Phase 3: User Story 1 through T024.
4. **STOP and VALIDATE**: `dotnet test DepthChart.sln`.
5. Demonstrate add, append, shift, past-the-end placement, and move before starting removal.

### Incremental Delivery

1. Setup and foundation.
2. User Story 1 add behaviours, one RED / GREEN / REFACTOR triple at a time.
3. User Story 2 remove.
4. User Story 3 full chart.
5. User Story 4 players below.
6. Edge cases, then NFL and MLB, then integration.
7. CI, refactor and review, then README and submission notes.

### Commit Boundaries

When the developer requests a commit, commit exactly one completed task. The RED task commit contains tests and no production behaviour for that case. The GREEN task commit contains the minimum production change. The REFACTOR task commit contains cleanup with the same observable results. Do not add an AI co-author trailer.

---

## Notes

- `[Story]` maps a task to spec.md for traceability.
- Setup and foundational tasks have no story label.
- Verification commands are the acceptance criteria. A task is done only when its command result matches that criterion.
- Stop at a checkpoint to review the story before the next phase.
- Avoid editing the same production file in two unfinished tasks.

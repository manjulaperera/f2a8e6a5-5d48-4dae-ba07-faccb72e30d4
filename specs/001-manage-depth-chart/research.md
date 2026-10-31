# Research: Manage Depth Chart

## In-memory chart storage

- **Decision**: Store each chart in a private `Dictionary<string, List<Player>>`. The key is the position code. The list index is the zero-based depth. Positions with no players are absent.
- **Rationale**: The approved architecture requires this structure. Dictionary lookup finds a position without scanning other positions. `List<T>` supports index insertion, tail shift, and gap-closing removal, which are the ranking rules. Full-chart order is taken from `SportDefinition`, so dictionary enumeration order is never observable.
- **Alternatives considered**: A `LinkedList<Player>` makes an index-based depth a walk and does not remove the O(n) shift. A second map from player id to index is O(1) for lookup and must be rebuilt on every shift. That index is premature for a ranking of 20 players. A database or file was rejected because the chart stays in memory.

## Sport configuration

- **Decision**: `SportDefinition` holds the sport name and the published position list. NFL and MLB are static definitions. A new sport is a new instance passed to `DepthChartService`. The service has no sport-specific branches.
- **Rationale**: The specification says a further sport is supported by its position names alone. Configuration is the extension point. The ranking operations stay one implementation.
- **Alternatives considered**: An `NflDepthChart` and `MlbDepthChart` subclass pair would duplicate the algorithm or force virtual methods that have nothing to vary. Hard-coded position arrays inside the service would require a service edit for every new sport.

## Public service contract

- **Decision**: Callers use `IDepthChartService`. `DepthChartService` is the only implementation. The four operations are add, remove, full chart, and players below.
- **Rationale**: The approved architecture asks for this contract. Tests and callers depend on the operations rather than on the dictionary. Sport variation stays in `SportDefinition`, not in a second service class.
- **Alternatives considered**: A public concrete class only. That is simpler and matches Principle III more closely. It was rejected because this plan's architecture requires the interface. The justification is recorded in the plan's Complexity Tracking.

## Output shape

- **Decision**: Full-chart rows and players-below results expose `PlayerId` values in rank order. Returned collections are new read-only copies. Names are not part of the result.
- **Rationale**: The published sample shows ids. The specification says names are not part of the ranking result. Copies stop a caller from changing a list that the service still owns.
- **Alternatives considered**: Returning the live `List<Player>` would leak storage and let a caller reorder the chart. Returning full `Player` records would add name and natural position to a result the specification does not include.

## Identity

- **Decision**: `PlayerId` is a non-nullable `int`. The service matches players with that value, including when the caller passes a different instance. Different ids are different players even when the names match. Equality for this purpose is explicit id comparison in the service.
- **Rationale**: The specification and the README assumptions define identity as player id, not object reference. A non-nullable id means a missing id cannot be constructed. A null `Player` is the missing-player case.
- **Alternatives considered**: `int?` so a null id can be rejected separately. That represents a state the sample never uses. Reference equality would fail the requirement that a different record with the same id is the same player.

## Validation and exceptions

- **Decision**: Validate before mutation, in a fixed order. Null player throws `ArgumentNullException`. A null, empty, or whitespace-only name throws `ArgumentException`. A null or unknown position throws `ArgumentException`. A negative depth throws `ArgumentOutOfRangeException`. Remove and players-below throw `ArgumentException` when that player id is not ranked at that position. The first failing check is the one reported.
- **Rationale**: The README assumptions name the three exception types for null player, invalid position, and negative depth. The specification requires rejection with an unchanged chart. A fixed order makes a call that has two faults deterministic. Whitespace-only names were the one unspecified blank-name case; treating them as blank gives tests a single outcome.
- **Alternatives considered**: A success/failure result type would avoid exceptions. The planning assumptions already name the exception types. Returning an empty list for a missing player was proposed and rejected by the specification. A no-op remove was proposed and rejected for the same reason.

## Natural position

- **Decision**: `NaturalPosition` is an optional string. The service does not compare it to the chart position or to the sport's position list.
- **Rationale**: The specification says natural position does not restrict membership. The published example sometimes omits it.
- **Alternatives considered**: Requiring the natural position to be a code of the same sport would contradict the rule that a player may be charted away from that position, including across the sports of two separate charts.

## Test and CI stack

- **Decision**: xUnit is the only test framework. The class library takes no package references. GitHub Actions runs `dotnet test` on the solution with warnings treated as errors.
- **Rationale**: The constitution requires xUnit and GitHub Actions, and it forbids unused dependencies. An extra assertion package does not add behaviour the specification needs.
- **Alternatives considered**: FluentAssertions or Shouldly would shorten asserts and would be a third-party dependency the library tests do not need. A hosted test server is infrastructure the specification does not require.

## Complexity ceiling

- **Decision**: Keep O(n) scans and list shifts. Do not add caches, trees, or span-based special cases in the first implementation.
- **Rationale**: The constitution forbids optimisation before a measured need is written into the specification. The volume check is 20 players at a position.
- **Alternatives considered**: A balanced tree ordered by depth would complicate shift semantics for no specified gain.

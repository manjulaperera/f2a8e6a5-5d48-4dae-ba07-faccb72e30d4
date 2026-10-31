# Library Contract: Depth Chart

This is the public surface of the class library. It is an in-process contract. There is no HTTP route, message schema, or database mapping.

Field rules are in [../data-model.md](../data-model.md). Behaviour comes from [../spec.md](../spec.md).

## Types

```text
Player
  PlayerId: int
  Name: string
  NaturalPosition: string or null

SportDefinition
  Name: string
  Positions: ordered position codes
  Contains(position) -> bool
  Nfl: SportDefinition
  Mlb: SportDefinition

DepthChartRow
  Position: string
  PlayerIds: read-only list of int

IDepthChartService
  AddPlayer(player, position, depth?)
  RemovePlayer(player, position)
  GetFullDepthChart() -> read-only list of DepthChartRow
  GetPlayersBelow(player, position) -> read-only list of int

DepthChartService
  constructor(SportDefinition)
  implements IDepthChartService
```

`depth` on add is optional. Omitting it is the no-depth case from the specification.

`DepthChartService` is constructed with one sport definition. Callers who want NFL and MLB at the same time construct two services. The interface does not expose the dictionary.

## AddPlayer

**Preconditions**: `player` is non-null, `Name` is not blank, `position` is an exact code from the service's sport, and `depth` is either omitted or greater than or equal to 0.

**Postconditions when the player id is new at that position**:

- Omitted depth, or a depth greater than or equal to the current count, places the player after everyone already there.
- A depth from 0 through the last occupied depth places the player there. Everyone from that depth downward moves down one. Their relative order stays the same.
- Other positions are unchanged.

**Postconditions when the player id is already at that position**:

- The player remains a single entry.
- A depth from 0 through one less than the current count becomes that player's depth.
- An omitted depth, or a depth greater than or equal to the current count, places the player last.
- Every other player at that position keeps relative order.

**Failure**: The matching exception from the validation table in [../data-model.md](../data-model.md). The chart is unchanged.

## RemovePlayer

**Postcondition**: The matching player id is gone from that position. Later players move up so depths stay contiguous from 0. Other positions are unchanged. The last player at a position removes that position from later full-chart results.

**Failure**: `ArgumentException` when that id is not ranked at that position, including when the id is ranked only at a different position. Null and invalid arguments use the same validation table. The chart is unchanged.

## GetFullDepthChart

**Postcondition**: Rows follow the sport's published order. A position appears only when it has at least one player. Each row's ids run from depth 0 to the bottom. An empty chart returns an empty list. The call does not change the chart.

The published sample, after Bob (id 1) at WR depth 0, Alice (id 2) at WR depth 0, Charlie (id 3) at WR depth 2, and Bob at KR with no depth, returns:

```text
WR: 2, 1, 3
KR: 1
```

## GetPlayersBelow

**Postcondition**: Ids strictly below the matched player at that position, in rank order. The named player is excluded. Other positions are excluded. The last player at the position returns an empty list. The call does not change the chart.

For the published WR ranking, players below Alice are `1, 3`.

**Failure**: `ArgumentException` when that id is not ranked at that position. The result is not an empty list in that case.

## Copying

Every returned list is a copy. Changing the list, or changing a later read, does not change a list already returned. The service does not hand out the internal `List<Player>`.

## Out of scope for this contract

No save, load, screen, or network method. No method that ranks by skill, statistics, or status. No method that accepts a position outside the sport definition supplied at construction.

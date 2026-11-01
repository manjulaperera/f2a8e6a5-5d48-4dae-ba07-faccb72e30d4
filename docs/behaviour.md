# Behaviour

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

## Assumptions and edge cases

The challenge fixes the sample, the NFL and MLB codes, multi-position ranking, and "add at the end when depth is omitted." The behaviours below are decisions recorded in [`specs/001-manage-depth-chart/spec.md`](../specs/001-manage-depth-chart/spec.md) where the challenge is silent. The specification is the authority for those cases.

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

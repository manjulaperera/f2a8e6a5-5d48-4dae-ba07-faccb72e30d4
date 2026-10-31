# Data Model: Manage Depth Chart

The library keeps these types in memory. There is no stored schema and no serializer.

## Player

A person who can be ranked. Instances are immutable.

| Field | Type | Required | Rule |
| --- | --- | --- | --- |
| `PlayerId` | `int` | Yes | Identity. Compared by value. `0` is allowed. Two instances with the same id are the same player. |
| `Name` | `string` | Yes | Not null, empty, or whitespace. The service rejects a blank name before it changes a chart. |
| `NaturalPosition` | `string` or null | No | Descriptive. It does not grant or deny a chart position. It may be a code from another sport. |

**Relationships**: A player does not own a chart. The same id may appear at several positions on one chart and on charts of different sports. Those charts are separate service instances.

**Identity**: Removal and players-below find the ranked entry with the same `PlayerId`. The instance used to add the player and the instance used later do not have to be the same object. A different id with the same name is a different player.

## SportDefinition

Immutable description of one sport. This is the extension point.

| Field | Type | Required | Rule |
| --- | --- | --- | --- |
| `Name` | `string` | Yes | Display and diagnostics. It is not used as a ranking key. |
| `Positions` | ordered list of `string` | Yes | Published order. Codes match exactly, including capitalisation. Duplicates inside one definition are invalid configuration. |

**Supplied definitions**:

| Name | Published positions |
| --- | --- |
| NFL | QB, WR, RB, TE, K, P, KR, PR |
| MLB | SP, RP, C, 1B, 2B, 3B, SS, LF, RF, CF, DH |

A further sport is another instance. Its position list is its published order. It is not added to the NFL or MLB lists.

**Validation owned by this type**: `Contains(position)` is true only for an exact code in `Positions`. Null and any other string are not contained. The service turns a failed check into `ArgumentException`.

## Depth chart storage

Owned only by `DepthChartService`. Not part of the public contract.

| Structure | Role |
| --- | --- |
| `Dictionary<string, List<Player>>` | Key: position code. Value: ranking from depth 0 at index 0 through the last player. |

**State rules**:

- A new service starts with an empty dictionary.
- The first accepted add at a position creates the list.
- Index `i` is depth `i`. There are no holes.
- The player at a position appears once. A later add of the same id moves that entry.
- Removing the last player removes the dictionary key.
- Keys that are not in the sport definition are never inserted, because validation rejects them first.
- The dictionary's enumeration order is not the full-chart order.

**Relationships**: One service holds one sport definition and one dictionary. Two sports are two services. Nothing in the model links those dictionaries.

## DepthChartRow

Immutable output. Not stored.

| Field | Type | Rule |
| --- | --- | --- |
| `Position` | `string` | A sport position that currently has at least one player. |
| `PlayerIds` | read-only list of `int` | Depth 0 first. Copied from the stored players. |

`GetFullDepthChart` returns a read-only list of rows. Row order follows `SportDefinition.Positions`, skipping positions with no stored list. An empty chart returns an empty list, not one row per sport position.

## State transitions

### Add a player who is not yet at that position

1. Reject a null player, blank name, invalid position, or negative depth. The dictionary stays as it was.
2. If the position has no list, create an empty list.
3. If `depth` is null, or `depth` is greater than or equal to the list count, append.
4. If `depth` is from 0 through the current count minus one, insert there and shift the tail down by one.

A depth greater than 0 on an empty position uses step 3 and leaves that player at depth 0.

### Add a player who is already at that position

1. Run the same rejection checks. A negative depth does not move the player.
2. Remember the destination. A null depth, or a depth greater than or equal to the current count, means the last depth after removal. Otherwise the destination is the supplied depth.
3. Remove the current entry and close the gap.
4. Insert that player at the destination.

The list length is unchanged. Other player ids keep their relative order.

### Remove

1. Reject a null player, blank name, invalid position, or an id that is not ranked at that position.
2. Remove the matching id and close the gap.
3. If the list is empty, remove the dictionary key.

Other positions and other service instances stay as they were.

### Read

Full chart and players-below do not change the dictionary. Players-below copies the ids strictly after the matched index. The last player yields an empty list. A missing id is a rejection, not an empty list.

## Validation summary

| Input | Outcome | Chart |
| --- | --- | --- |
| Null player | `ArgumentNullException` | Unchanged |
| Blank name, including whitespace only | `ArgumentException` | Unchanged |
| Null, unknown, wrong-sport, or differently capitalised position | `ArgumentException` | Unchanged |
| Negative depth | `ArgumentOutOfRangeException` | Unchanged |
| Remove or players-below for an id not at that position | `ArgumentException` | Unchanged |
| Empty chart, full read | Empty row list | Unchanged |
| Last player, players-below | Empty id list | Unchanged |

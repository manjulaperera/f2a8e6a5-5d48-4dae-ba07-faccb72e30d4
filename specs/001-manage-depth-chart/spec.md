# Feature Specification: Manage Depth Chart

**Feature Branch**: `001-manage-depth-chart`

**Created**: 2026-10-30

**Status**: Draft

**Input**: User description: "Create a detailed functional specification for a sports depth chart management library. Cover add player, remove player, retrieve the full depth chart, and retrieve players ranked below a given player. Support NFL positions QB, WR, RB, TE, K, P, KR, PR and MLB positions SP, RP, C, 1B, 2B, 3B, SS, LF, RF, CF, DH. Depth positions are zero-based. Inserting at an occupied position shifts existing players down. Omitting the depth appends the player. Players may appear in multiple positions. A player's natural position does not restrict depth chart membership. Preserve ranking order. Support additional sports without changing the core ranking rules. Include Given/When/Then acceptance criteria. Identify ambiguous behaviours rather than silently inventing requirements. Include normal scenarios, edge cases, and validation."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Add a player to a depth chart (Priority: P1)

A manager ranks a player at a position on one sport's depth chart. The manager
either chooses a zero-based depth or leaves the depth unset and places the
player at the end of that position. If the chosen depth is already occupied, the
new player takes that depth and the players at that depth and below move down
one place, keeping their relative order. If that player is already ranked at
that position, the existing entry moves and the player still appears once.

**Why this priority**: No chart exists until a manager can place players. Every
other outcome depends on the ranking this action creates.

**Independent Test**: Place players at one position with a stated depth, with an
omitted depth, and into an occupied depth. Read that position's ranking back.
The order matches the shift and append rules, and the published sample ranking
for WR and KR is reproduced.

**Acceptance Scenarios**:

1. **Given** an empty NFL chart, **When** the manager adds Bob (player id 1) at WR depth 0, **Then** WR is ranked Bob only, at depth 0.
2. **Given** an empty NFL chart, **When** the manager adds Bob at WR and omits the depth, **Then** Bob is ranked at WR depth 0.
3. **Given** WR is ranked Alice then Bob, **When** the manager adds Charlie at WR and omits the depth, **Then** WR is ranked Alice, Bob, Charlie.
4. **Given** an empty NFL chart, **When** the manager adds Bob at WR depth 0 and then adds Alice (player id 2) at WR depth 0, **Then** WR is ranked Alice at depth 0 and Bob at depth 1.
5. **Given** WR is ranked Alice, Bob, Charlie, **When** the manager adds Dana at WR depth 1, **Then** WR is ranked Alice, Dana, Bob, Charlie.
6. **Given** WR is ranked Alice then Bob, **When** the manager adds Charlie (player id 3) at WR depth 2, **Then** WR is ranked Alice, Bob, Charlie.
7. **Given** an empty NFL chart and players Bob (id 1), Alice (id 2), and Charlie (id 3), **When** the manager adds Bob at WR depth 0, Alice at WR depth 0, Charlie at WR depth 2, and Bob at KR with no depth, **Then** WR is ranked as player ids 2, 1, 3 and KR is ranked as player id 1.
8. **Given** the ranking in scenario 7, **When** the full chart is read, **Then** the WR ranking is unchanged by Bob's placement at KR.
9. **Given** a player whose natural position is WR, **When** the manager adds that player at KR on an NFL chart, **Then** the player is ranked at KR.
10. **Given** an empty MLB chart, **When** the manager adds a first player at SP depth 0 and a second player at SP depth 0, **Then** SP is ranked second player then first player.
11. **Given** an empty NFL chart and one player, **When** the manager adds that player once at each of QB, WR, RB, TE, K, P, KR, and PR, **Then** each of those positions ranks that player at depth 0.
12. **Given** an empty MLB chart and one player, **When** the manager adds that player once at each of SP, RP, C, 1B, 2B, 3B, SS, LF, RF, CF, and DH, **Then** each of those positions ranks that player at depth 0.
13. **Given** Bob (player id 1, name Bob) is ranked at WR, **When** the manager adds a record with player id 1 and a different name at KR, **Then** KR ranks player id 1 and the chart still treats that id as one player.
14. **Given** an empty NFL chart, **When** the manager adds a player at a depth below 0, **Then** the request is rejected and the chart stays empty.
15. **Given** an empty NFL chart, **When** the manager adds a player at SP, **Then** the request is rejected and the chart stays empty.
16. **Given** an empty NFL chart, **When** the manager adds a player at a position code that is not an NFL position, including a different capitalisation of a valid code, **Then** the request is rejected and the chart stays empty.
17. **Given** any chart, **When** the manager adds a player who is missing, has no player id, or has a blank name, **Then** the request is rejected and every ranking stays unchanged.
18. **Given** WR is ranked Alice, Bob, Charlie, **When** the manager adds Bob at WR depth 0, **Then** WR is ranked Bob, Alice, Charlie, and Bob appears once.
19. **Given** WR is ranked Alice, Bob, Charlie, Dana, **When** the manager adds Dana at WR depth 1, **Then** WR is ranked Alice, Dana, Bob, Charlie, and Dana appears once.
20. **Given** WR is ranked Alice, Bob, Charlie, **When** the manager adds Alice at WR and omits the depth, **Then** WR is ranked Bob, Charlie, Alice, and Alice appears once.
21. **Given** WR is ranked Alice, Bob, Charlie, **When** the manager adds Dana at WR depth 10, **Then** WR is ranked Alice, Bob, Charlie, Dana.
22. **Given** WR is ranked Alice, Bob, Charlie, **When** the manager adds Alice at WR depth 10, **Then** WR is ranked Bob, Charlie, Alice, and Alice appears once.

---

### User Story 2 - Remove a player from a depth chart (Priority: P2)

A manager takes a player off one position's ranking. Players who remain at that
position keep the order they had before the removal, and their depths are
renumbered from 0 with no empty depths between them. Rankings at other positions
stay as they were.

**Why this priority**: A chart that only grows cannot reflect a backup moving up
or a player leaving a position.

**Independent Test**: Start from a known ranking of three or more players, remove
the top, middle, or last player, and read that position back. Also remove a
player from one of two positions where that player is listed and confirm the
other position is unchanged.

**Acceptance Scenarios**:

1. **Given** WR is ranked Alice, Dana, Bob, Charlie, **When** the manager removes Bob from WR, **Then** WR is ranked Alice, Dana, Charlie at depths 0, 1, and 2.
2. **Given** WR is ranked Alice, Bob, Charlie, **When** the manager removes Alice from WR, **Then** WR is ranked Bob, Charlie.
3. **Given** WR is ranked Alice, Bob, Charlie, **When** the manager removes Charlie from WR, **Then** WR is ranked Alice, Bob.
4. **Given** Bob is ranked at both WR and KR, **When** the manager removes Bob from WR, **Then** Bob is no longer at WR and Bob remains at KR in the same place.
5. **Given** KR is ranked Bob only, **When** the manager removes Bob from KR, **Then** KR has no players.
6. **Given** WR is ranked Alice, **When** the manager removes a different player from WR, **Then** the request is rejected and WR remains Alice.
7. **Given** Bob is ranked at KR and is not ranked at WR, **When** the manager removes Bob from WR, **Then** the request is rejected, WR is unchanged, and Bob remains at KR.
8. **Given** an empty NFL chart, **When** the manager removes a player from WR, **Then** the request is rejected and the chart stays empty.
9. **Given** an NFL chart, **When** the manager removes a player from SP or from a position code that is not an NFL position, **Then** the request is rejected and every ranking stays unchanged.

---

### User Story 3 - Retrieve the full depth chart (Priority: P3)

A manager reads the whole chart. For every position that is included, the
manager sees its players from depth 0 downward, each identified by player id.
The published sample is the required result for that sequence of adds.

**Why this priority**: The manager needs the complete ranking, not only the last
edit, to compare starters and backups across positions.

**Independent Test**: Reproduce the published sample adds, then read the full
chart. The WR and KR rankings match the sample. A separate MLB chart returns
only MLB positions that the inclusion rule keeps.

**Acceptance Scenarios**:

1. **Given** Bob (id 1), Alice (id 2), and Charlie (id 3) were added as Bob at WR depth 0, Alice at WR depth 0, Charlie at WR depth 2, and Bob at KR with no depth, **When** the manager retrieves the full chart, **Then** WR is player ids 2, 1, 3 in that order and KR is player id 1.
2. **Given** a position ranking, **When** the manager retrieves the full chart, **Then** players at that position appear from the top of the ranking to the bottom, identified by player id.
3. **Given** an MLB chart with one player at C and no NFL chart involved, **When** the manager retrieves the full chart, **Then** the result does not contain an NFL position.
4. **Given** a chart with no players, **When** the manager retrieves the full chart, **Then** the result is an empty collection.
5. **Given** an NFL chart with players only at KR and WR, **When** the manager retrieves the full chart, **Then** the result contains WR then KR, and it does not contain QB, RB, TE, K, P, or PR.
6. **Given** an NFL chart, **When** the manager adds a player at KR and then a player at WR, **Then** the full chart lists WR before KR.
7. **Given** an MLB chart with players only at CF and C, **When** the manager retrieves the full chart, **Then** the result contains C then CF, and it does not contain SP, RP, 1B, 2B, 3B, SS, LF, RF, or DH.

---

### User Story 4 - Retrieve players ranked below a player (Priority: P4)

A manager names a player and a position and receives every player ranked below
that player at that position. The named player is not included. Results are
player ids in rank order. Players above the named player are not included, and
other positions are not included.

**Why this priority**: The manager needs the backup chain under a starter or
under a selected backup without reading players who outrank them.

**Independent Test**: Build the published WR ranking of Alice, Bob, Charlie.
Ask for the players below Alice, below Bob, and below Charlie. The results are
Bob then Charlie, Charlie only, and no players.

**Acceptance Scenarios**:

1. **Given** WR is ranked Alice (id 2), Bob (id 1), Charlie (id 3), **When** the manager retrieves the players below Alice at WR, **Then** the result is player ids 1, 3 in that order.
2. **Given** WR is ranked Alice, Bob, Charlie, **When** the manager retrieves the players below Bob at WR, **Then** the result is Charlie only.
3. **Given** WR is ranked Alice, Bob, Charlie, **When** the manager retrieves the players below Charlie at WR, **Then** the result contains no players.
4. **Given** Bob is ranked at WR and at KR, and another player is ranked below Bob only at KR, **When** the manager retrieves the players below Bob at WR, **Then** the result includes only players below Bob at WR.
5. **Given** Alice is not ranked at WR, **When** the manager retrieves the players below Alice at WR, **Then** the request is rejected and every ranking stays unchanged.
6. **Given** an NFL chart, **When** the manager retrieves the players below a player at SP or at a position code that is not an NFL position, **Then** the request is rejected and every ranking stays unchanged.

---

### User Story 5 - Apply the same ranking rules to another sport (Priority: P5)

NFL and MLB are both supported now. A further sport is defined by its own
position names. Add, remove, full-chart retrieval, and players-below retrieval
follow the same ranking rules there as they do for NFL and MLB. The sport
changes which position names are valid. It does not change shift, append,
multi-position membership, or preserved order.

**Why this priority**: Both listed sports are required, and the challenge states
that more sports may be added later. The ranking rules have to stay one set of
rules so a new sport is a new list of positions.

**Independent Test**: Create a chart for a sport whose positions are neither the
NFL list nor the MLB list. Run an equivalent add, shift, append, remove, and
players-below sequence. The ranking matches an NFL chart that received the same
depth choices. An NFL position name is rejected on that chart.

**Acceptance Scenarios**:

1. **Given** a chart for a sport whose positions are GK and ST, **When** the manager adds the first player at GK depth 0 and the second player at GK depth 0, **Then** GK is ranked second player then first player.
2. **Given** that GK ranking, **When** the manager omits the depth while adding a third player at GK, **Then** the third player is last.
3. **Given** a player ranked at GK and at ST, **When** the manager removes that player from GK, **Then** the ST ranking is unchanged.
4. **Given** that sport's chart, **When** the manager adds a player at WR, **Then** the request is rejected and the chart stays unchanged.
5. **Given** equivalent sequences on an NFL chart and on this other sport's chart, **When** the manager compares the resulting rankings, **Then** the order of players matches and no extra sport-specific ranking rule has applied.
6. **Given** a separate NFL chart and a separate MLB chart, **When** the manager adds the same player id at WR on the NFL chart and at CF on the MLB chart, **Then** both charts rank that player id.

---

### Edge Cases

- First player at a position, with depth 0 or with the depth omitted: that player is at depth 0.
- Depth omitted when the player is not already ranked at that position: the player is appended. When the player is already ranked there, that entry moves to the last depth.
- Depth equal to the current number of players, for a player not already ranked there: the player is appended. The published sample does this when Charlie is added at WR depth 2 while two players are already ranked there.
- Insert at an occupied depth, for a player not already ranked there, including the top and the middle: the new player takes the requested depth and every player from that depth downward moves down one, relative order preserved.
- Same player listed at two positions: each position is independent for add, remove, and players-below.
- Player natural position differs from the chart position: membership is allowed when the chart position belongs to the chart's sport.
- Remove the top, middle, or last player: remaining players at that position keep relative order and are renumbered from depth 0.
- Remove a player from one position while that player remains at another position: only the named position changes.
- Remove the last player at a position: that position has no players and no longer appears on the full chart. Positions that still have players remain, in the sport's published order.
- Players below the top player, a middle player, and the last player: everyone strictly below, only those below, and no players.
- Empty chart: full-chart retrieval returns an empty collection.
- Same player already ranked at that position: the entry moves, the player appears once, and the other players keep their relative order. An omitted depth, or a depth at or past the current count, places that player last.
- Depth greater than the number of players at that position, for a player not already ranked there: the player is appended. No empty depth is created.
- Negative depth: rejected, chart unchanged.
- Position from the other sport, an unknown code, or a different capitalisation of a valid code: rejected, chart unchanged.
- Missing player, missing player id, or blank name: rejected, chart unchanged.
- Remove, or ask for players below, a player who is not ranked at that position: rejected, chart unchanged.
- Same player id on a separate chart for another sport: both charts may rank that player.
- Non-empty full chart: only positions with at least one player appear, in the sport's published order. NFL order is QB, WR, RB, TE, K, P, KR, PR. MLB order is SP, RP, C, 1B, 2B, 3B, SS, LF, RF, CF, DH. This inclusion rule is an assumption.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: A manager MUST be able to add a player to one position on a depth chart at a zero-based depth.
- **FR-002**: When the manager omits the depth and the player is not already ranked at that position, the player MUST be placed after every player already ranked at that position. When the position has no players, that player MUST be placed at depth 0.
- **FR-003**: When the requested depth is equal to the number of players already ranked at that position, and the player is not already ranked there, the player MUST be placed after those players.
- **FR-004**: When the requested depth is occupied and the player is not already ranked at that position, the new player MUST occupy that depth, and every player already ranked at that depth and below MUST move down one depth while keeping their relative order.
- **FR-005**: A player MUST be able to hold a ranking at more than one position on the same chart. An add or removal at one position MUST leave every other position's ranking unchanged.
- **FR-006**: A player's natural position MUST NOT decide eligibility. The manager MUST be able to rank a player at any position that belongs to the chart's sport.
- **FR-007**: A manager MUST be able to remove a player from one named position. Players who remain at that position MUST keep their relative order and MUST occupy contiguous depths starting at 0.
- **FR-008**: A manager MUST be able to retrieve the players ranked at a position from depth 0 downward, each identified by player id.
- **FR-009**: A manager MUST be able to retrieve the players ranked strictly below a named player at a named position, identified by player id, in rank order, excluding the named player and excluding every other position.
- **FR-010**: When the named player is last at that position, the players-below result MUST contain no players.
- **FR-011**: An NFL depth chart MUST accept only the positions QB, WR, RB, TE, K, P, KR, and PR. An MLB depth chart MUST accept only the positions SP, RP, C, 1B, 2B, 3B, SS, LF, RF, CF, and DH.
- **FR-012**: A depth chart MUST belong to one sport. The ranking rules for add, remove, full-chart content within a position, and players-below MUST be the same for every sport. A further sport MUST be supported by its own position names alone, without a sport-specific exception to those ranking rules.
- **FR-013**: When a player who is already ranked at a position is added to that same position again, that player MUST remain a single entry and MUST be moved. Every other player at that position MUST keep their relative order. When the supplied depth is from 0 through one less than the current count, the player MUST occupy that depth. When the depth is omitted, or the supplied depth is greater than or equal to the current count, the player MUST occupy the last depth.
- **FR-014**: When the player is not already ranked at that position and the requested depth is greater than the number of players currently ranked there, the player MUST be appended after those players. The ranking MUST NOT gain an empty depth.
- **FR-015**: When the depth chart has no players, retrieval of the full chart MUST return an empty collection. When the chart has one or more players, the full chart MUST include only positions that have at least one player, ordered as the sport publishes them: NFL QB, WR, RB, TE, K, P, KR, PR; MLB SP, RP, C, 1B, 2B, 3B, SS, LF, RF, CF, DH. A position with no players MUST be omitted. The order of entry MUST NOT change that position order.
- **FR-016**: A depth below 0 MUST be rejected.
- **FR-017**: A position that is not one of the chart's sport positions MUST be rejected, including a position from another sport and a code that differs in capitalisation from the sport's position.
- **FR-018**: A missing player, a player without a player id, or a player with a blank name MUST be rejected.
- **FR-019**: Removing a player who is not ranked at the named position MUST be rejected. This includes a player who is ranked only at a different position.
- **FR-020**: Retrieving players below a player who is not ranked at the named position MUST be rejected.
- **FR-021**: Every rejected request MUST leave every position's ranking unchanged.
- **FR-022**: Two player records with the same player id MUST be treated as the same player. Name and natural position MUST NOT create a second player.

### Key Entities *(include if feature involves data)*

- **Player**: A person who can be ranked. Attributes are player id, name, and natural position. Player id is the identity used in rankings and in retrieved results. Natural position is descriptive and does not grant or deny chart membership.
- **Sport**: A named set of position codes. NFL and MLB are required, with the codes in FR-011. Another sport is a different named set of position codes and uses the same ranking rules.
- **Depth chart**: The rankings for one team in one sport. It holds at most one ranking per position of that sport.
- **Position ranking**: The ordered players at one position. Depth 0 is the top of the ranking. Accepted rankings use contiguous depths from 0 through one less than the number of players.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: For the published sample sequence, the full chart shows WR as player ids 2, 1, 3 and KR as player id 1, and the players below Alice at WR are player ids 1 then 3.
- **SC-002**: A manager can place a ranking on all 8 NFL positions on an NFL chart and all 11 MLB positions on an MLB chart.
- **SC-003**: Across the acceptance scenarios for accepted adds and removals, 100% of players who remain on a position keep the relative order they had before the change.
- **SC-004**: Across the acceptance scenarios for rejected requests, 100% of those requests leave every position ranking unchanged.
- **SC-005**: An equivalent sequence of adds and removals on NFL, MLB, and one further sport produces the same player order. The further sport accepts none of the NFL or MLB position codes.
- **SC-006**: A position ranking of 20 players still returns those players from depth 0 downward after an insertion at depth 0 and after removal of the player at depth 0, with relative order of the remaining players preserved.
- **SC-007**: In one retrieval, a manager sees every player ranked behind a chosen player at a chosen position, in that rank order, and sees none of the players ahead of the chosen player and none from another position.

## Assumptions

- The manager is a coach or front-office user maintaining one team's chart for a single sport. A caller may keep more than one chart. Each chart is still one sport.
- The published sample is a required observable outcome. Bob is player id 1, Alice is player id 2, and Charlie is player id 3. The sample's displayed lines describe the information the manager receives. They do not require a screen, a printed page, or a console transcript.
- Retrieved rankings identify players by player id. The sample does not include names in the ranking result, so names are not part of that result.
- Depth 0 is the top of the position. A larger depth is further down the ranking.
- The source challenge shows Charlie added at a depth equal to the current count, so that value appends. A depth greater than the current count also appends when the player is not already ranked there (FR-014). That greater-depth rule is an assumption, not a challenge requirement.
- When the player is already ranked at that position, another add moves the existing entry (FR-013). An omitted depth moves that player to the last depth. These move rules are assumptions, not challenge requirements.
- The source challenge does not show removal. Remaining players are renumbered from depth 0 with no gap because an accepted ranking is a contiguous zero-based order and because relative order must be preserved. An empty depth left behind after removal would contradict that rule.
- A player who is not on the named position cannot be removed from it, and has no players below them there. Both requests are rejected and the chart stays unchanged (FR-019, FR-020). A no-op removal and an empty players-below result were proposed for these cases. They are not used, because this specification already rejects both requests.
- A chart with no players returns an empty collection from full-chart retrieval. That empty-chart result is an assumption.
- The same player id may be ranked on separate charts for different sports. Each chart still accepts only its own sport's positions.
- Player identity is the player id. Two records with the same player id are the same player even when they are distinct records. Name and natural position do not create a second player.
- Position codes match the sport's codes exactly. `wr` is not the NFL position `WR`.
- A player name is required and must not be blank. Player id is required. A missing player is rejected. The source model includes both name and player id.
- The chart does not order players by skill, statistics, health, or status. Order changes only when a manager adds or removes a player.
- Out of scope: saving charts in a database, a user interface, a remote service, multi-team transactions, injury or inactive lists, and positions that neither NFL, MLB, nor a defined further sport has named.
- A non-empty full chart includes only positions that have at least one player, in the sport's published order. NFL order is QB, WR, RB, TE, K, P, KR, PR. MLB order is SP, RP, C, 1B, 2B, 3B, SS, LF, RF, CF, DH. Adding KR before WR still returns WR before KR. Positions with no players are omitted. The challenge sample shows WR then KR and does not by itself require this order, so the rule is an assumption (FR-015).

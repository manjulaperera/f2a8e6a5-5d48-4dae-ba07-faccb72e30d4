# Depth Chart Management

A C#/.NET implementation of an extensible sports depth chart management system.

## Supported Sports

- NFL
- MLB

## Engineering Practices

- Specification-Driven Development
- Test-Driven Development
- SOLID principles
- AI-assisted development with Cursor
- Automated testing with xUnit
- GitHub Actions CI

## Status

Development in progress.

## Assumptions

The challenge does not define the behaviours in this section. They are assumptions
for this solution, not requirements stated by the challenge. The functional
specification in `specs/001-manage-depth-chart/spec.md` is the authority when it
already decides a case.

| Scenario | Assumption |
| --- | --- |
| Add a player who is already ranked at that position | Move the existing player to the requested depth. The player remains a single entry, and the other players keep their relative order. |
| Add a player who is already ranked at that position, with no depth | Move that player to the end of the position. |
| Negative depth | Reject the add and leave the chart unchanged. The assumed failure signal is `ArgumentOutOfRangeException`. |
| Depth greater than the number of players already ranked at that position | Append the player to the end. No empty depth is created. If that player is already ranked there, the existing entry moves to the end. |
| Remove a player who is not ranked at that position | Reject the removal and leave the chart unchanged. A no-op was proposed. It is not used, because the specification already requires rejection (FR-019). |
| Players ranked below a player who is not at that position | Reject the request and leave the chart unchanged. An empty collection was proposed. It is not used, because the specification already requires rejection (FR-020). |
| Position that is not valid for the chart's sport | Reject the request and leave the chart unchanged. The assumed failure signal is `ArgumentException`. |
| Same player in multiple positions | Allowed. This is also required by the challenge and by the specification. |
| Same player in multiple sports | Allowed. The same player id may be ranked on separate charts for different sports. Each chart still accepts only its own positions. |
| Empty depth chart | Full-chart retrieval returns an empty collection. |
| Full chart when at least one player is ranked | Include only positions that have at least one player, in the sport's published order. NFL order is QB, WR, RB, TE, K, P, KR, PR. MLB order is SP, RP, C, 1B, 2B, 3B, SS, LF, RF, CF, DH. Adding KR before WR still returns WR before KR. Positions with no players are omitted. |
| Player identity | Identity is `PlayerId`, not object reference. Two records with the same player id are the same player. |
| Null player | Reject the request and leave the chart unchanged. The assumed failure signal is `ArgumentNullException`. |

A depth equal to the current number of players still appends, as the challenge
sample does when Charlie is added at depth 2. The exception types above describe
how a rejection required by the specification is signalled. They are not
challenge requirements.

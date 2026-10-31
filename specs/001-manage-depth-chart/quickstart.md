# Quickstart: Manage Depth Chart

Validation guide for the library after implementation. This file does not contain the implementation.

## Prerequisites

- .NET 10 SDK
- The solution described in [plan.md](./plan.md): `src/DepthChart` and `tests/DepthChart.Tests`
- No database, web host, or extra package feed

Project settings for both projects:

- Target framework `net10.0`
- Nullable reference types enabled
- Implicit usings enabled
- Warnings treated as errors

The class library has no package references. The test project references xUnit, Microsoft.NET.Test.Sdk, and the xUnit runner.

## Build and test

From the repository root:

```text
dotnet test
```

A successful run builds the library and the test project, then passes the xUnit suite. A compiler warning fails the build. A failed assertion fails the run.

Run one test by its display name while practising Red-Green-Refactor:

```text
dotnet test --filter FullyQualifiedName~DepthChartServiceTests
```

Replace the filter with the test that was just written. Confirm that test fails for the expected reason before adding the production code that should make it pass.

## What the suite must prove

Use [contracts/depth-chart.md](./contracts/depth-chart.md) for the operations and [data-model.md](./data-model.md) for the fields. The tests call `IDepthChartService`. They compare player ids and exception types.

Minimum end-to-end check, the published sample:

1. Create a service with `SportDefinition.Nfl`.
2. Add Bob (`PlayerId` 1) at `WR` depth 0.
3. Add Alice (`PlayerId` 2) at `WR` depth 0.
4. Add Charlie (`PlayerId` 3) at `WR` depth 2.
5. Add Bob at `KR` with no depth.
6. The full chart is WR `2, 1, 3`, then KR `1`.
7. Players below Alice at WR are `1, 3`.

Also confirm:

- An empty service returns an empty full chart.
- MLB uses `SportDefinition.Mlb` and the same shift rules.
- A service built with a custom `SportDefinition` accepts only that definition's positions.
- A rejected add, remove, or lookup leaves the previous ids in place.

## Continuous integration

GitHub Actions runs on the branch workflow. The job uses the .NET 10 SDK and executes `dotnet test`. The job fails when compilation warns or when a test fails. The workflow does not deploy and does not start a database or a web server.

## Out of scope for a local check

No UI, HTTP call, or connection string. There is nothing to migrate. Passing `dotnet test` is the validation for this feature.

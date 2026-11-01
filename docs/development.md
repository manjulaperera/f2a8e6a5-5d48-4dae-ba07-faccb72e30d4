# Development

GitHub Spec Kit holds the agreed behaviour under [`specs/001-manage-depth-chart`](../specs/001-manage-depth-chart). The constitution is [`.specify/memory/constitution.md`](../.specify/memory/constitution.md).

The artifacts were produced in this order:

1. **Constitution.** Engineering rules: C#/.NET 10, xUnit, specification first, Red-Green-Refactor, no database, API, or UI, and no AI co-author on commits.
2. **Specification** (`spec.md`). The challenge rewritten as user scenarios and functional requirements, including NFL, MLB, and a further sport supplied only as position names.
3. **Clarification.** Open cases were decided and written into the specification and the [assumptions](behaviour.md), instead of being left for the implementation to invent.
4. **Plan** (`plan.md`, `data-model.md`, `research.md`, `contracts/depth-chart.md`). The types, the dictionary of lists, the validation order, and the complexity in [architecture](architecture.md).
5. **Tasks** (`tasks.md`). One Red, Green, or Refactor step per behaviour.

Production code was added only for behaviour those artifacts already described.

## Red-Green-Refactor

Each behaviour followed the same cycle:

1. Write a failing xUnit test for the observable outcome.
2. Run it and confirm it fails for the expected reason.
3. Write the minimum production code that makes it pass.
4. Run the suite.
5. Refactor only when the structure is clearer and the observable behaviour stays the same.
6. Run the suite again.

Tests assert returned ids, row order, and exception type. They do not read the private dictionary. Existing assertions were left in place when later code was added.

Tests are xUnit, named Given/When/Then, and assert with FluentAssertions 7.2.2. `ThrowExactly` is used where a derived exception must not satisfy the test. Moq is referenced by the unit project for a test that needs to isolate a dependency. The current tests construct `DepthChartService` and players directly and do not use a mock.

Neither test project references NHibernate. The chart is in memory and has no database.

## Human-reviewed AI-assisted development

Cursor was the editor used to draft the Spec Kit artifacts and the implementation. Spec Kit supplied the constitution, specification, plan, and task workflow. The developer chose each slice, reviewed the proposed change before it was treated as accepted work, and controlled every commit.

The practice recorded in the constitution is:

- One task at a time, with the assumption and the trade-off stated before the edit.
- A human review at each Red-Green-Refactor checkpoint.
- Commits only when the developer asks for them.
- No `Co-authored-by` trailer for Cursor or any other assistant.

A review of the library against the challenge, constitution, specification, and plan is in [`code-review.md`](code-review.md).

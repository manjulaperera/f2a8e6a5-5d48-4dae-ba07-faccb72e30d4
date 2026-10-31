<!--
Sync Impact Report
- Version change: unratified template → 1.0.0
- Modified principles:
  - [PRINCIPLE_1_NAME] → I. Specification-First Development
  - [PRINCIPLE_2_NAME] → II. Strict Test-Driven Development (NON-NEGOTIABLE)
  - [PRINCIPLE_3_NAME] → III. SOLID Without Unnecessary Abstraction
  - [PRINCIPLE_4_NAME] → IV. Clean Domain Modelling
  - [PRINCIPLE_5_NAME] → V. Sport Extensibility
- Added principles:
  - VI. Deterministic Behaviour
  - VII. Explicit Validation and Documented Assumptions
  - VIII. No Unnecessary Infrastructure
  - IX. Automated Tests and CI Validation
  - X. Human Review of AI-Generated Code
  - XI. Incremental Git Commits
  - XII. No AI Co-Author Attribution
- Added sections:
  - Technology and Quality Constraints (replaces [SECTION_2_NAME])
  - Development Workflow (replaces [SECTION_3_NAME])
- Removed sections: none
- Follow-up TODOs: none
-->

# Depth Chart Management Constitution

## Core Principles

### I. Specification-First Development

Every change MUST start from an approved specification and technical plan produced
with GitHub Spec Kit. Implementation MUST stay inside that approved scope.
Ambiguous requirements MUST be clarified before implementation begins. Each
implementation task MUST be small enough to verify on its own.

**Rationale:** Behaviour is agreed and reviewed before production code exists, so
later sports and rules extend a known contract.

### II. Strict Test-Driven Development (NON-NEGOTIABLE)

Production behaviour MUST be built with strict Red-Green-Refactor, in this order:

1. Write a failing test that states the required behaviour.
2. Run that test and confirm it fails for the expected reason.
3. Write the minimum production code that makes the test pass.
4. Run the test suite.
5. Refactor while observable behaviour stays the same.
6. Run the complete test suite again.

Tests MUST be written before the production implementation they describe. A
feature and its tests MUST be produced in separate steps. Tests MUST cover
normal behaviour, edge cases, and invalid inputs. Tests MUST assert observable
behaviour, stay independent of one another, and remain unchanged when the goal
is to make an incorrect implementation pass.

**Rationale:** A failing test is the specification of the next increment. Production
code written before that test has no prior evidence of the behaviour it claims.

### III. SOLID Without Unnecessary Abstraction

Design MUST follow the Single Responsibility, Open-Closed, Liskov Substitution,
Interface Segregation, and Dependency Inversion principles. A new abstraction
MUST earn its place by isolating a real variation, such as sport-specific rules.
Dependency injection MUST be used only where it separates a genuine dependency.
The domain model MUST stay direct and free of abstractions that have a single
implementation and no expected variation.

**Rationale:** SOLID keeps NFL and MLB rules replaceable. Abstractions with no
variation hide the depth chart and increase review cost.

### IV. Clean Domain Modelling

Depth chart concepts MUST be expressed as small, cohesive types with meaningful
names. Methods MUST do one job. Callers MUST receive immutable output collections,
or defensive copies, so a returned chart cannot change internal state. Domain
rules MUST live in the model that owns them.

**Rationale:** A small model is readable in review, testable one rule at a time,
and safe for a second sport to reuse.

### V. Sport Extensibility

Sport-specific rules MUST sit behind an extension point that the sport-agnostic
depth chart operations already depend on. Adding a sport MUST be possible by
supplying that sport's rules through the existing extension point. The approved
scope is NFL and MLB until a specification adds another sport. An extension
point MUST represent a variation the specification already identifies.

**Rationale:** The challenge requires further sports later. The core chart
operations stay stable when only the sport rules change.

### VI. Deterministic Behaviour

The same inputs and the same prior state MUST produce the same outputs.
Ordering, ranking, and removal results MUST follow explicit domain rules.
Tests MUST control any dependency on time, randomness, enumeration order, or
the environment. Incidental collection order MUST NOT decide depth chart order.

**Rationale:** Deterministic results make failures reproducible and make chart
order meaningful across sports.

### VII. Explicit Validation and Documented Assumptions

Every public operation MUST validate its inputs and reject invalid input with
an explicit, deterministic outcome. Assumptions about positions, player
identity, ordering, duplicates, removal, and error signalling MUST be written
in the specification before implementation encodes them. Implementation MUST
follow those documented assumptions.

**Rationale:** Undocumented assumptions collide when a new sport uses the same
operations with different rules.

### VIII. No Unnecessary Infrastructure

The solution MUST remain a domain library and its tests. It MUST NOT add a
database, web API, user interface, hosted service, or other infrastructure the
specification does not require. It MUST NOT add a dependency that the domain
and its tests do not need. Premature optimisation is prohibited. A performance
change MUST wait for a measured need recorded in the specification.

**Rationale:** Extra infrastructure widens the review surface and obscures the
depth chart behaviour this challenge is meant to demonstrate.

### IX. Automated Tests and CI Validation

xUnit MUST verify the behaviour required by the specification. GitHub Actions
MUST run both test projects on the supported branch workflow and MUST fail the
build when either project fails or the compiler reports warnings. A task is
locally complete only after the relevant test run passes under the same warning
settings CI enforces.

Unit tests and integration tests MUST live in separate projects:

- `tests/DepthCharts.Unit.Tests`, namespace `DepthCharts.Unit.Tests`. A unit
  test covers one type or one operation. `Player`, `SportDefinition`, and each
  chart operation belong here.
- `tests/DepthCharts.Integration.Tests`, namespace
  `DepthCharts.Integration.Tests`. An integration or acceptance test runs the
  public operations together. The published challenge sample, a chart that uses
  more than one position, and two charts or sports belong here.

A new test MUST go in the project that matches that boundary. Moq MAY be
referenced by the unit project only, and only to isolate a dependency. Neither
project may reference NHibernate or a database package.

**Rationale:** Local Red-Green-Refactor and human review both rely on a shared,
automated gate. Separating unit and acceptance tests keeps a single-operation
failure distinct from a scenario that crosses the public contract.

### X. Human Review of AI-Generated Code

A human MUST review every AI-generated change to specifications, plans, tests,
and production code before that change is accepted. Review MUST be requested
at each TDD checkpoint: after the failing test is confirmed, and again after
the implementation is green. AI assistance MUST explain the proposed change,
the assumptions it uses, and any architectural trade-off before editing code,
and MUST work on one task at a time.

**Rationale:** The developer remains accountable for the solution. The tool that
drafted a change does not approve it.

### XI. Incremental Git Commits

Accepted work MUST be committed in small increments that match a single
verifiable task. The developer controls every commit. A commit MUST be created
only when the developer explicitly requests it. History MUST be amended,
squashed, or rewritten only with the developer's explicit approval.

**Rationale:** Small commits keep review and rollback aligned with the
Red-Green-Refactor steps.

### XII. No AI Co-Author Attribution

Commit messages MUST name the human author who accepted the change. Messages
MUST NOT include a Co-authored-by trailer for an AI tool. Commits MUST NOT
attribute authorship to Copilot, Cursor, Claude, or any other AI assistant.

**Rationale:** Authorship records the person who reviewed the change and took
responsibility for it.

## Technology and Quality Constraints

The Depth Chart Management solution MUST use:

- C# on .NET 10 for production code and tests.
- xUnit for automated tests.
- GitHub Spec Kit for specifications, plans, and tasks.
- Cursor for AI-assisted editing, subject to Principle X.
- GitHub Actions for continuous integration.

Project settings MUST:

- Enable nullable reference types.
- Enable implicit usings.
- Treat compiler warnings as errors.

Code MUST:

- Keep classes small and cohesive.
- Return immutable output collections or defensive copies.
- Prefer deterministic logic, as required by Principle VI.
- Avoid dependencies that Principles III and VIII do not justify.

The repository MUST NOT contain:

- A database or persistence layer.
- A web API or other network endpoint.
- A user interface.
- Optimisations adopted before a measured need is specified.

Both test projects MUST reference xUnit and the production assembly. They MUST
follow the same nullable, warning, and determinism rules as production code.
Their folders and namespaces are fixed by Principle IX.

## Development Workflow

Work MUST proceed in this order:

1. Specify the behaviour and record assumptions (Principle I and Principle VII).
2. Plan the design inside the approved scope, including any extension point a
   sport requires (Principle V).
3. Write a failing test for the next behaviour (Principle II).
4. Run the test and confirm the failure reason.
5. Implement the minimum production code that satisfies the test.
6. Refactor under a green suite.
7. Request human review of the tests and the production change (Principle X).
8. Commit only when the developer requests it, as one increment for that task
   (Principle XI), with human authorship only (Principle XII).

Production implementation of a behaviour MUST NOT begin before its test exists
and has been seen to fail for the expected reason. A task MUST NOT be treated
as accepted before a human has reviewed it and the automated tests pass.

## Governance

This constitution is the authority for engineering practice on the Depth Chart
Management solution. Where another note conflicts with this document, this
document applies.

Amendments MUST be made in this file and MUST include:

- A semantic version bump.
- An updated last-amended date in `YYYY-MM-DD` form.
- A short rationale for the change.

Version bumps follow these rules:

- MAJOR: a principle is removed or redefined so existing governed practice no
  longer complies.
- MINOR: a principle or section is added, or existing guidance is materially
  expanded.
- PATCH: wording, clarification, or other non-semantic refinement.

The developer is the approving authority for an amendment. Compliance MUST be
reviewed when a specification, plan, or task list is produced, and again before
a change is accepted. GitHub Actions MUST enforce the automated portion of
these rules, including tests and warnings-as-errors. Any complexity beyond
these principles MUST be justified in the approved specification or plan.

**Version**: 1.1.0 | **Ratified**: 2026-10-30 | **Last Amended**: 2026-11-01

Amendment 1.1.0 separates unit tests from integration and acceptance tests, as
stated in Principle IX. The ranking rules are unchanged.

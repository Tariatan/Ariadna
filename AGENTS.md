# Ariadna agent instructions

## Start here

Read [MEMORY.md](MEMORY.md), then
[docs/Technical_Design_Description.md](docs/Technical_Design_Description.md)
for product behavior and technical decisions, and [docs/PLAN.md](docs/PLAN.md)
for remaining/upcoming tasks. Load other context only as needed.

Apply the global instructions supplied with the session; this file adds
repository-specific rules.

The user's current instructions take precedence. The Technical Design
Description owns product behavior and technical decisions; memory is a compact
handoff, not a second specification. Proposals are not user-confirmed requirements.
Resolve contradictions explicitly instead of silently choosing an old memory entry.

## Product boundaries

Ariadna is a personal Windows media catalog with four independently launched
modes: movies, documentaries, games, and library. The current application uses
C#/.NET 9 WinForms and direct SQLite storage in Ariadna.Storage. The separate
migration utility alone uses a SQL client for legacy export. Preserve collection-specific
behavior and the ability to run all four modes simultaneously.

Keep existing catalog IDs, metadata, relationships, ignore entries, and external
images intact. Poster filenames use extensionless IDs; game previews use the
configured suffix plus numbers 1 through 4. Treat database and image recovery
as one matched dataset. Use synthetic data and disposable directories for write
tests; keep API keys and personal catalog contents out of examples and diagnostics.

## Work and verification

Implement one usable slice at a time from docs/PLAN.md. Before changing code,
identify the affected workflow in the Technical Design Description. Preserve
existing behavior through characterization tests before broader refactoring.
Migration work must follow the recovery and comparison gates in
[docs/SQLITE-MIGRATION-PLAN.md](docs/SQLITE-MIGRATION-PLAN.md).

Use README.md for restore/build/test/run guidance. The solution uses SDK-style
projects with checked-in dependency locks; do not assume another project's SDK
or commands apply here. Close running Ariadna instances
before rebuilding. UI changes need native Windows mouse/keyboard checks in the
affected modes. In-memory query tests do not prove database-provider behavior.
Do not claim planned commands, unavailable checks, or documentation as verified
implementation.

## Versioning

Bump versions only for projects whose implementation changed. Do not bump an
unchanged project merely because it references a changed project or ships with it.
Record notable implementation changes in [CHANGELOG.md](CHANGELOG.md) under the
affected project's version, newest first, with a date and the appropriate Keep
a Changelog category. Use Major for breaking behavior, Minor for compatible
features, and Patch for compatible bug fixes. Keep version metadata and changelog
entries aligned; planned rework belongs in PLAN.md until implemented.

## Editing Rules

- Never add an empty line at the end of a file.
- If a file ends with an empty line, remove that empty line.
- Keep documentation links repository-relative. Do not publish machine-specific absolute paths or references to local agent libraries.

## Unit Tests

- Prefer `new Mock<T>()` and `.Object` over `Mock.Of<T>()` and `Mock.Get()`.
- Name mocks `[mockedObjectName]Mock`.
- Name unit test classes `[ClassUnderTest]Tests`.
- Name unit test methods `[MethodUnderTest]_[Precondition]_[ExpectedOutcome]`.
- Structure unit tests as `Arrange - Act - Assert` and include `// Arrange`, `// Act`, and `// Assert` comments.
- Prefer `[TestInitialize]` for complicated test setup. Use the test class constructor only for simple setup.
- Before broader refactoring, prefer characterization tests that cover existing workflow behavior over low-value tests for trivial methods.

## Code Reviews

- Check whether the concept is clear in the changes, naming matches the concept, responsibilities are assigned correctly, and any pattern used fits the problem.
- Check whether related behavior is kept together and properly cleaned up, including disposing, unsubscribing, and similar lifecycle handling.
- Check whether expected functionality is missing and whether the result is convenient to use without unnecessary repeated actions.
- Check whether code blocks are easy to read and avoid unnecessary complexity or fancy constructs.
- Prefer existing functionality and established language or framework features over custom reimplementation.
- Check whether logic inside loops is limited to work that must happen inside the loop.
- Treat locally suppressed warnings as a review concern unless there is a strong reason.
- If feedback is not explicitly covered by an agreed guideline or decision, discuss it as feedback or best practice rather than presenting it as a strict rule.
- If the concept appears fundamentally mismatched and would require major rework, recommend a discussion with the author instead of only leaving isolated comments.

## Maintain context

Keep this file operational. Update docs/Technical_Design_Description.md when
behavior or architecture changes, then update docs/PLAN.md's task list. Keep
detailed investigations in their focused documents and link them from the design.
Update local MEMORY.md with durable facts, evidence dates, limitations, and the
next concrete step when project context changes. Do not accumulate transcripts,
secrets, personal catalog values, or unverified claims. This concerns repository
memory only, not global agent memory.

At handoff, distinguish implemented, verified, planned, and blocked work. Record
checks actually run. Do not mark a milestone complete merely because documents
describe it. No automatic delegation, remote publication, or recurring automation
is established by this file.

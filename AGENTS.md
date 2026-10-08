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

## Default project

Focus on Ariadna.Wpf by default for features, fixes, UI work, builds, and runs.
Use Ariadna.Wpf.Tests for frontend tests and change shared projects only when
the task requires it. The legacy WinForms Ariadna project is not the default
target; modify it only when explicitly requested.

## Product boundaries

Ariadna is a personal Windows media catalog with one application instance and
four permanent tabs: movies, documentaries, games, and library. The current application uses
C#/.NET 10 WPF in Ariadna.Wpf and direct SQLite storage in Ariadna.Storage.
The legacy WinForms frontend remains in the repository. The separate
migration utility alone uses a SQL client for legacy export. Preserve collection-specific
behavior and each tab's filters, selection, scroll position, and palette.

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
projects in Ariadna.slnx with checked-in dependency locks and an SDK pin in
global.json; do not assume another project's SDK or commands apply here.
Close running Ariadna instances
before rebuilding. UI changes need native Windows mouse/keyboard checks in the
affected modes. In-memory query tests do not prove database-provider behavior.
Do not claim planned commands, unavailable checks, or documentation as verified
implementation.

## Versioning

Bump versions for projects whose implementation changed. Whenever any project's
version changes, also bump the main project Ariadna.Wpf, even if its implementation
is unchanged. Do not bump other unchanged projects merely because they reference
or ship with a changed project.
Record notable implementation changes in [CHANGELOG.md](CHANGELOG.md) under the
affected project's version, newest first, with a date and the appropriate Keep
a Changelog category. Use Major for breaking behavior, Minor for compatible
features, and Patch for compatible bug fixes. Keep version metadata and changelog
entries aligned; planned rework belongs in PLAN.md until implemented.

## Editing Rules

- docs/PLAN.md is maintained exclusively by the user. Read it for context, but never modify it, including task status, completion notes, or formatting.
- Never add an empty line at the end of a file.
- If a file ends with an empty line, remove that empty line.
- Keep documentation links repository-relative. Do not publish machine-specific absolute paths or references to local agent libraries.

## Maintain context

Keep this file short and operational. Update
[docs/Technical_Design_Description.md](docs/Technical_Design_Description.md)
only when a significant application design decision changes, such as architecture,
component responsibilities or boundaries, or a core workflow contract. Keep the
TDD concise and focused on durable design rationale. Routine features, bug fixes,
UI adjustments, implementation details, test results, and progress notes belong
in CHANGELOG.md, focused documentation, or repository memory when requested;
they do not require a TDD update unless they materially change application design.
Leave docs/PLAN.md unchanged. Link focused documents from the TDD only when they
support an important design decision.

If repository MEMORY.md exists, update it only when requested by the user, with
durable facts, evidence dates, limitations, and the next concrete step. Do not
accumulate transcripts, secrets, private telemetry, or unverified claims.
This concerns repository memory only, not global agent memory.

At handoff, distinguish implemented, verified, planned, and blocked work. Record
checks actually run. Do not mark a milestone complete merely because documents
describe it. No automatic delegation, remote publication, or recurring automation
is established by this file.

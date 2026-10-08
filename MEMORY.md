# Ariadna project memory

Last tidied: 2026-10-08. Repository context only; evidence below is dated.

## Read the authoritative sources

- [AGENTS.md](AGENTS.md): project rules and ownership.
- [Technical Design Description](docs/Technical_Design_Description.md): product
  behavior and technical decisions.
- [PLAN.md](docs/PLAN.md): user-maintained upcoming work.
- [CHANGELOG.md](CHANGELOG.md): implementation history and project versions.
- [WPF migration](docs/WPF-MIGRATION.md): compatibility and acceptance sequence.
- [SQLite operations](docs/SQLITE-OPERATIONS.md): matched recovery and migration
  evidence; [migration investigation](docs/SQLITE-MIGRATION-PLAN.md): original gates.

## Decisions and acceptance boundaries

- **2026-10-04 — WPF is the default development target.** The user's designation
  is recorded in AGENTS.md and the TDD's WPF section. It does not establish
  production cutover or WinForms retirement; WinForms remains the fallback.
- **2026-10-03 — copied-catalog acceptance.** The user reported successful
  browse, details, edit and restart checks across all four collections with
  version 2.0.1 on an isolated full-data copy, including posters/game previews.
  See SQLite operations, Verification scope. This is dated acceptance of that
  copy, not evidence of later WPF or live-data checks.
- **As recorded through 2026-10-06 — WPF acceptance remains incomplete.** Native
  checks used disposable synthetic catalogs. Personal matched-dataset use,
  changed monitor/DPI, physical touchpad behavior, live TMDb, configured external
  tools and clean-machine installation remain outside the recorded verification.
  Native destructive removal and exhaustive image/clipboard/discovery checks
  were not established by the initial WPF implementation.
- WPF initially copied default configuration without importing legacy per-user
  settings. Check configured paths before acceptance; see WPF migration.

## Latest recorded handoff

- **2026-10-06 — WPF 1.9.0/1.9.1.** Details genre-picker mouse/keyboard checks
  passed with synthetic Movies; the recorded Release WPF suite passed 113 tests.
  The subsequent Paste (`↓`) before Add (`+`) change passed six focused people
  tests, a Release build and whitespace checks. Personal-catalog/DPI acceptance
  remains open. Evidence is historical; no application tests were rerun for this
  memory cleanup.
- Current next work is in PLAN.md: remaining Details redesign and edit fields.
  Acceptance should follow the WPF migration sequence on a disposable matched
  copy before a daily-use switch. The earlier genre text-picker, deferred-scroll
  and splash descriptions in history are superseded by the TDD's WPF section.

## Incidents worth retaining

- **Storage 2.0.1, 2026-10-03:** a read-only empty recovery directory blocked
  cleanup. The fix clears that directory's read-only flag while preserving
  recovery decisions and other access errors; four filesystem regressions and
  a synthetic published startup check passed.
- **Storage 2.0.2, 2026-10-04:** unchanged relationship saves could replace legacy
  duplicates/IDs. Four regressions failed before the fix; unchanged sequences
  now preserve rows, while explicit edits retain replacement behavior.
- **WPF 1.0.1, 2026-10-04:** saving geometry at Closed encountered empty
  RestoreBounds and invalid-infinity JSON. Saving after cancellation handlers at
  Closing fixed it; five regressions and isolated native close/reopen passed.
  An initial harness accidentally started production startup; it was stopped
  before editing and corrected. No production recovery outcome was verified.
- **WPF 1.4.3, 2026-10-05:** simply disabling deferred scrolling caused backward
  thumb jumps. The user rejected waiting until release; retaining fractional
  thumb position separately from row offsets enabled live dragging. Four drag
  cases and synthetic native dragging passed.
- **WinForms 4.0.1/4.1.5, 2026-10-03/04:** application-model DPI defaults shrank
  pixel layouts; explicit DpiUnaware restored scaling. A later designer save
  duplicated tab pages and omitted identity tags, causing startup failure;
  named designer-page references replaced tag identity. Regressions and synthetic
  native checks passed; a new designer save/reopen round trip was not verified.

## Historical evidence on demand

The previous entries are preserved in these dated archives. Their test counts,
benchmarks, limitations and incident details belong to their recorded versions;
stale “next” steps do not override PLAN.md.

- [WPF Details and previous context](docs/memory/WPF-DETAILS-HISTORY.md).
- [WPF browsing and frontend introduction](docs/memory/WPF-BROWSING-HISTORY.md).
- [Legacy frontend and storage incidents](docs/memory/LEGACY-STORAGE-HISTORY.md).

Keep this handoff focused on non-obvious decisions, incidents and unresolved
acceptance. Release narratives belong in CHANGELOG.md; edit memory only when
requested, as required by AGENTS.md.

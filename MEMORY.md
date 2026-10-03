# Ariadna project memory

Last updated: 2026-10-03. Scope: this repository only.

## Durable context

- Windows personal media catalog: movies/series, documentaries, games, and books.
  Each launch selects one mode; independent windows can run simultaneously.
- Current stack: .NET 9 WinForms, EF6 6.5.1, SQL Server LocalDB. DbProvider is a
  legacy .NET Framework 4.8 project; its generated sources are also linked into
  the desktop project. SQLite remains a proposed migration.
- Catalog IDs connect database entries to extensionless poster filenames.
  Game previews append the configured suffix and numbers 1 through 4. Preserve
  identifiers and keep database/assets together for recovery.
- Language conventions and reusable-agent routing are supplied by global agent
  instructions. Repository editing/test/review rules remain in [AGENTS.md](AGENTS.md).

## Current evidence

- 2026-10-03: removed machine-specific instruction paths and local-library links
  from public documentation. The migration investigation now identifies catalog
  locations through configuration names and filenames instead of absolute paths.
  Global instructions remain supplied by the agent environment.
- 2026-10-03: added [CHANGELOG.md](CHANGELOG.md) following Balancia's per-project
  Keep a Changelog/SemVer format. Baselines are Ariadna 1.0.0 (evaluated MSBuild
  Version) and DbProvider 1.0.0 (assembly/file version 1.0.0.0). These dates record
  changelog initialization, not earlier releases; version metadata is unchanged.
  AGENTS.md now requires notable implementation changes to be recorded there.
- 2026-10-03: compared Balancia's current AGENTS, MEMORY, README, and docs with
  this repository. Aligned their document roles and reading order, renamed the
  architecture document to [docs/Technical_Design_Description.md](docs/Technical_Design_Description.md),
  and added [docs/PLAN.md](docs/PLAN.md). Source/project inspection confirmed the
  current stack, duplicate model compilation, direct EF access in forms/main
  window, and PNG saves to extensionless image paths.
- [docs/SQLITE-MIGRATION-PLAN.md](docs/SQLITE-MIGRATION-PLAN.md) reports a
  2026-10-03 read-only investigation of the live database and image inventory.
  That evidence was not rechecked against live data during harness alignment.
  Backup/restore rehearsal, export, conversion, content-hash comparison,
  performance benchmarking, and migrated UI acceptance remain unperformed.
- The migration investigation was already present as an untracked file at the
  start of alignment and was preserved during initial alignment; the later
  public-documentation cleanup replaces its local path references. Documentation
  work changes no application implementation, dependency, database, or image asset.
- This checkout has no SDK pin or NuGet dependency lock files. Existing CLI
  commands are documented in README; build/test/native UI checks were not run
  for this documentation-only change. Documentation links, obsolete filename
  references, trailing blank lines, and diff whitespace were checked.

## Resume here

Use [docs/PLAN.md](docs/PLAN.md) for the next task. Establish a repeatable clean
restore/build/test baseline and native four-mode acceptance on disposable data.
Before any SQLite implementation, resolve the proposal's open choices and
demonstrate matched database/image recovery. Do not infer migration approval
or completed work from the existence of planning documents.

## Context maintenance

Product behavior and technical reasoning belong in the Technical Design
Description; remaining/upcoming tasks belong in PLAN.md. Keep this handoff
compact, dated, and explicit about evidence limits. This is repository memory,
not the global agent memory folder.

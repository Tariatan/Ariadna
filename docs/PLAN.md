# Implementation plan and checkpoint

Last updated: 2026-10-03.

The current application remains on WinForms, EF6, and SQL Server LocalDB.
The [Technical Design Description](Technical_Design_Description.md) owns current
behavior and technical decisions. The
[SQLite migration investigation](SQLITE-MIGRATION-PLAN.md) is a detailed proposal;
its presence does not approve implementation or mark any migration gate complete.

## ToDO list

### Development and acceptance baseline

- [ ] Verify and document a clean Windows restore/build/test recipe for the
  SDK-style desktop/tests and legacy .NET Framework DbProvider project, including
  restoration of its packages.config dependencies.
- [ ] Decide the supported SDK/tooling baseline and whether to add an SDK pin
  and dependency locks. Do not claim reproducible locked restore until configured
  and verified for the actual project mix.
- [ ] Run the existing characterization tests and native four-mode acceptance
  scenarios in the Technical Design Description using disposable data/images.
  Record actual results and limitations in repository MEMORY.md.

### SQLite migration proposal — conditional work

Use the investigation's detailed sequence and gates if migration is approved.
These are proposed tasks, not changes authorized by harness alignment.

- [ ] Resolve storage organization, Unicode comparison policy, provider/native
  engine selection, image recovery behavior, and practical performance baseline.
- [ ] Establish matched SQL Server/image/configuration recovery and demonstrate
  a restore to a separate database before changing the production application.
- [ ] Build a separate, lossless export/conversion rehearsal against the restored
  source. Preserve IDs, identity high-water marks, NULLs, duplicate relationships,
  photos, and external image filenames. Compare all values and content hashes;
  require structural and foreign-key integrity checks to pass.
- [ ] Implement focused SQLite storage operations and migrate queries/detail
  workflows collection by collection. Preserve filters, discovery, ignore rules,
  launch behavior, and simultaneous windows; test transactions and failure paths.
- [ ] Verify all four modes with the real provider and copied data, including
  Unicode searches, empty relationships, image failures, concurrent writes, and
  reopen behavior. Benchmark against the same baseline dataset.
- [ ] Repeat conversion from a fresh matched backup for final cutover; retain
  recovery artifacts and the legacy executable. Explain rollback limits after
  accepting new SQLite edits. Remove EF/DbProvider dependencies only when all
  migration gates pass and verify clean deployment without LocalDB.
- [ ] Establish and demonstrate matched SQLite/database-image backup and restore
  for ongoing use and future schema upgrades.

## Completion evidence

For each completed slice, record the source revision/context, checks actually
run, and any remaining native/provider/recovery limitations. Documentation
alignment is complete when reading order, document ownership, renamed-file
references, local links, and editing rules are consistent; it does not verify
application behavior or complete the migration proposal.

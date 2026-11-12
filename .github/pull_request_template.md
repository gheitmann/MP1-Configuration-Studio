## Summary

<!-- One paragraph: what changed and why. -->

## Ticket

<!-- M1-07, #123, or a short description. -->

## Tests run

- [ ] Fixture tests x64
- [ ] Native smoke on isolated runtime
- [ ] Fault-injection attach test
- [ ] Manual check on a real installation

## Evidence

<!-- CI run, test log, screenshots, sanitized report. -->

## Data safety

- [ ] No writes outside `appearance.xml`, logs, backups, and Studio files.
- [ ] Native controls, values, and save handlers preserved.
- [ ] Classic fallback verified if attach or theming changed.

## Checklist

- [ ] One logical change.
- [ ] New or updated test for the behavior.
- [ ] Documentation updated if user-visible.
- [ ] No secrets, personal configuration, or `dist/` output committed.
- [ ] GPL-3.0-compatible contribution.

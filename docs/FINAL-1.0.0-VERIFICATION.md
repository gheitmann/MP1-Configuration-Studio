# 1.0.0 — Final Release Verification

**Release:** [MP1 Configuration Studio 1.0.0](https://github.com/gheitmann/MP1-Configuration-Studio/releases/tag/v1.0.0)
**Tag:** `v1.0.0`
**Commit:** `705ed5d7075c61baa623771453f0dbb980733087`
**Published:** 2026-11-13
**Signing:** Unsigned, documented waiver (`ALLOW_UNSIGNED_FINAL=true`); no code-signing certificate exists in the project environment.

## Published assets

| Asset | Size | SHA256 |
| --- | --- | --- |
| `MP1-Configuration-Studio-1.0.0-Portable.zip` | 87,501 | `3FA0D4EACF29D3866937F34CA9DA2AAACF0B0278287B18FDC153100CF529FB77` |
| `MP1-Configuration-Studio-1.0.0-Setup.exe` | 2,181,422 | `EE8BB04C37B23C26097F3DAFD2A90AB6EDC538040ACAC5DFA867A9532889B7E2` |

Both SHA256 sidecars match the downloaded assets. The portable ZIP reports version 1.0.0.0 and contains no `ShellTests`, audit, review, or persistence symbols.

## Validation

| Check | Result |
| --- | --- |
| Fixture checks x64 | 174 / 174 |
| Native smoke, isolated MP 1.39 x64 | 27 / 27 |
| Full 175% review | 61 / 61 pages |
| Value preservation | 61 / 61 |
| Overflow candidates | 0 |
| Plugin dialogs | 5 / 5 |
| Attach time | 303–320 ms |
| Classic CLI | Original configurator started |
| Original `Configuration.exe` hash | Unchanged |
| Production `MediaPortal.xml` hash | Unchanged |

## Release workflow

The tag workflow completed successfully:

- Build and fixture tests passed.
- Portable ZIP, Inno Setup executable, and SHA256 sidecars were built.
- The draft release was published after asset download and hash verification.
- The setup was built in CI but not executed on the development account because it requires elevation.

## Known limitations

- The release is unsigned. Verify SHA256; SmartScreen may warn.
- A clean Windows 11 VM installation/start was not executed; validation used the development Windows 11 installation and an isolated MediaPortal runtime.
- OS DPI 150% and 200% were not validated in a fresh session.
- The Studio shell is German; native page labels follow MediaPortal. English shell localization is planned for 1.1.
- High Contrast uses system colors; native owner-drawn tabs/lists/sliders still need an audit.
- The fixture suite uses the deterministic project harness; NUnit migration is planned.

## Waiver

The maintainer approved publication without code signing and without a clean-VM run on 2026-11-13. The waiver is documented here and in the release notes; it does not claim that the missing checks passed.

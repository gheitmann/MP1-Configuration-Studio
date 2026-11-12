# M1 Status

**Updated:** 2026-11-12
**Branch:** `m1-foundation`
**Baseline:** `7a00043` (`main`)
**Last verification:** `build.ps1 -TestX86` → 142/142 fixture checks on x64 and x86; native smoke 27/27 on isolated MediaPortal 1.39 x64.

## Environment

| Item | State |
| --- | --- |
| MediaPortal | 1.39 x64 installed at `C:\Program Files\Team MediaPortal\MediaPortal` |
| .NET SDK | 8.0.428, portable at `work\dotnet`; used by `build.ps1` |
| Legacy compiler | No longer used by the build; kept only in git history |
| Git | System Git and portable MinGit available |
| GitHub | Authenticated as `gheitmann` through Windows Credential Manager; `m1-foundation` and the archive tag are pushed |
| Testing | x64 only; no external testers required; optional 0–5 pilot |

## Decisions

- MediaPortal 1.39 **x64 only** for 1.0. No x86 binary is shipped.
- Validation-first: automated fixture, visual, native, and fault-injection checks plus a seven-day self-use period. External pilot is optional (0–5 users).
- Windows 11 23H2+ is supported. Windows 10 22H2 is best effort if a VM is available.

## Progress

| Ticket | Status | Evidence / note |
| --- | --- | --- |
| M1-01 Archive alpha | Done | Tag `v0.1.0-alpha.1-archive` pushed |
| M1-02 Split solution | Done (logical) | `src/Core`, `src/Native`, `src/App`; single deployable assembly retained |
| M1-03 MSBuild/Roslyn | Done | SDK-style `net48` project; `build.ps1` uses the .NET 8 SDK |
| M1-04 CI | Done | Hosted runs 1–8 completed successfully |
| M1-05 Diagnostics separation | Done | `diagnostics\Studio.Diagnostics.csproj`; shipped app contains no test symbols |
| M1-06 `NativeContract` | Done | Commit `6a218a0`; contract validation before attach |
| M1-07 Transactional attach | Done | Commit `6a218a0`; snapshot rollback and eight fault-injection stages |
| M1-08 Logging/exceptions | Done | Structured rolling log, redaction, crash dialog log path |
| M1-09 Versioning/About | Done | Product/file version 1.0.0; About entry in the tools menu |
| M1-10 Governance/docs | Done | Commit `c2d6c43` |
| M1-11 Test runner | Deferred | Custom harness remains; NUnit migration scheduled for M2 |
| M1-12 Compatibility manifest | Done | 61 section types, embedded default, override file, four checks |
| M1-13 Appearance schema v2 | Done | Commit `44e8066`; 7 new checks |
| M1-14 Performance baseline | Done | Lazy page preparation reduced attach to 303–320 ms; full 175% review 61/61 with 0 overflow candidates |
| M1-15 Support bundle | Done | Sanitized ZIP with metadata, policy, sections, appearance, and logs |
| M1-16 Security/threat model | Done | `SECURITY.md` documents trust boundaries, elevation, plugin code, and data handling |
| M1-17 First artifact | Partial | CI artifact downloaded, version 1.0.0.0, no test symbols, started successfully on the isolated runtime; clean-VM start pending |

## Verification

- Baseline `0.1.0-alpha.1`: 95/95 x64 and x86 checks passed before changes.
- Current branch: 162/162 x64 fixture checks passed from the diagnostics host.
- The shipped application binary contains no `ShellTests`, audit, review, or persistence symbols.
- Native smoke: 27/27 checks passed on an isolated MediaPortal 1.39 x64 runtime after the source split.
- Attach baseline: 303–320 ms after lazy page preparation, below the 2000 ms budget.
- Full 175% review after the change: 61/61 pages, 0 value failures, 0 overflow candidates, 5 plugin dialogs.
- Page switches above 300 ms and searches above 150 ms are logged as warnings.
- The downloaded CI artifact reports version 1.0.0.0, contains no test symbols, and starts with the expected Studio window on the isolated runtime.
- Logs are written to `%LOCALAPPDATA%\MP1 Configuration Studio\logs\studio.log` and rotate at 2 MB with three files.
- Fault injection at eight attach stages restores the native control set, form state, and page presentation.
- The native `Configuration.exe` and production `MediaPortal.xml` hashes are unchanged after the isolated run.

## Next actions

1. Download the CI artifact and verify it on a clean Windows 11 VM (M1-17).
2. Optimize the 2716 ms attach path (M2).
3. Move fixture tests to NUnit in M2 (M1-11 deferred).
4. Continue with M2 hardening: plugin tiers, classic mode, DPI matrix.

## Remaining external dependencies

- A clean Windows 11 VM for installer and clean-install validation. VirtualBox is available locally.
- Code signing for the 1.0 release, not for M1.
- Optional pilot users; not a release gate.

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
| M1-02 Split solution | Partial | Diagnostics project separated; Core/Native/App split remains |
| M1-03 MSBuild/Roslyn | Done | SDK-style `net48` project; `build.ps1` uses the .NET 8 SDK |
| M1-04 CI | Done | Hosted runs 1 and 2 completed successfully |
| M1-05 Diagnostics separation | Done | `diagnostics\Studio.Diagnostics.csproj`; shipped app contains no test symbols |
| M1-06 `NativeContract` | Done | Commit `6a218a0`; contract validation before attach |
| M1-07 Transactional attach | Done | Commit `6a218a0`; snapshot rollback and eight fault-injection stages |
| M1-08 Logging/exceptions | Done | Structured rolling log, redaction, crash dialog log path |
| M1-09 Versioning/About | Not started | |
| M1-10 Governance/docs | Done | Commit `c2d6c43` |
| M1-11 Test runner | Unblocked | Test framework can be restored with the SDK |
| M1-12 Compatibility manifest | Not started | Needs the 61-section inventory |
| M1-13 Appearance schema v2 | Done | Commit `44e8066`; 7 new checks |
| M1-14 Performance baseline | Not started | |
| M1-15 Support bundle | Not started | |
| M1-16 Security/threat model | Partial | `SECURITY.md` written; detailed threat model still open |
| M1-17 First artifact | Not started | Depends on M1-03/04 |

## Verification

- Baseline `0.1.0-alpha.1`: 95/95 x64 and x86 checks passed before changes.
- Current branch: 148/148 x64 fixture checks passed from the diagnostics host.
- The shipped application binary contains no `ShellTests`, audit, review, or persistence symbols.
- Native smoke: 27/27 checks passed on an isolated MediaPortal 1.39 x64 runtime.
- Logs are written to `%LOCALAPPDATA%\MP1 Configuration Studio\logs\studio.log` and rotate at 2 MB with three files.
- Fault injection at eight attach stages restores the native control set, form state, and page presentation.
- The native `Configuration.exe` and production `MediaPortal.xml` hashes are unchanged after the isolated run.

## Next actions

1. Split the remaining application into Core/Native/App (M1-02).
2. Add version identity and the compatibility manifest (M1-09, M1-12).
3. Add the support bundle and performance baseline (M1-14, M1-15).
4. Produce and verify the first CI-built x64 artifact (M1-17).

## Remaining external dependencies

- A clean Windows 11 VM for installer and clean-install validation. VirtualBox is available locally.
- Code signing for the 1.0 release, not for M1.
- Optional pilot users; not a release gate.

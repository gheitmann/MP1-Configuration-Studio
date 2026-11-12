# M1 Status

**Updated:** 2026-11-12
**Branch:** `m1-foundation`
**Baseline:** `7a00043` (`main`)
**Last verification:** `build.ps1 -TestX86` → 102/102 fixture checks on x64 and x86.

## Environment

| Item | State |
| --- | --- |
| MediaPortal | 1.39 x64 installed at `C:\Program Files\Team MediaPortal\MediaPortal` |
| Compiler | Legacy .NET Framework 4.0 `csc.exe` (C# 5) |
| .NET SDK / VS Build Tools | Not installed |
| Git | Not on `PATH`; portable MinGit available in the local work folder |
| GitHub credentials | Not available; push and issue creation pending |

## Progress

| Ticket | Status | Evidence / note |
| --- | --- | --- |
| M1-01 Archive alpha | Done locally | Tag `v0.1.0-alpha.1-archive` at `7a00043`; push pending |
| M1-02 Split solution | Not started | Requires refactoring the `StudioShell` partials |
| M1-03 MSBuild/Roslyn | Blocked | No .NET SDK or VS Build Tools; legacy `csc` build still used |
| M1-04 CI | Blocked | Workflow can be written; observing it needs GitHub access |
| M1-05 Diagnostics separation | Not started | Depends on M1-02 |
| M1-06 `NativeContract` | Next | Critical path |
| M1-07 Transactional attach | Next | Depends on M1-06 |
| M1-08 Logging/exceptions | Not started | |
| M1-09 Versioning/About | Not started | |
| M1-10 Governance/docs | Done | Commit `c2d6c43` |
| M1-11 Test runner | Blocked | Needs a test framework via NuGet; current harness runs 102 checks |
| M1-12 Compatibility manifest | Not started | Needs the 61-section inventory |
| M1-13 Appearance schema v2 | Done | Commit `44e8066`; 7 new checks |
| M1-14 Performance baseline | Not started | |
| M1-15 Support bundle | Not started | |
| M1-16 Security/threat model | Partial | `SECURITY.md` written; detailed threat model still open |
| M1-17 First artifact | Not started | Depends on M1-03/04 |

## Verification

- Baseline `0.1.0-alpha.1`: 95/95 x64 and x86 checks passed before changes.
- Current branch: 102/102 x64 and x86 checks passed.
- No native smoke run yet on this branch.
- No files were written to the live MediaPortal configuration during build or fixture tests.

## Next actions

1. Implement `NativeContract` and replace scattered reflection lookups.
2. Make attach transactional with fault injection.
3. Split the solution and move diagnostics out of the app.
4. Add logging, version identity, and the compatibility manifest.
5. Modernize the build and add CI once the SDK and repository access are available.

## Blockers needing a decision

- Approve a local portable .NET SDK install for M1-03, or keep the legacy build through M1.
- Provide GitHub credentials or push the branch manually for M1-04 and the archive tag.
- Provide an x86 MediaPortal runtime if x86 support is to remain a 1.0 target.

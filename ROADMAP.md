# Roadmap to 1.0

**Project:** MP1 Configuration Studio
**Target:** Version 1.0.0 for MediaPortal 1.39
**Status:** Planning
**Last updated:** 2026-11-12
**Companion documents:** [Release checklist](docs/RELEASE-CHECKLIST.md), [M1 tickets](docs/M1-TICKETS.md)

---

## 1. Product decision

MP1 Configuration Studio is a **safe reading and navigation layer** over the last MediaPortal 1 configurator. It makes the native configuration pages usable on large displays: grouped navigation, search, readable typography, light/dark presentation, and validated scaling. It does not reimplement settings, save handlers, or plugin logic.

MediaPortal 1.39 is frozen. That is an advantage: the integration target does not move. Version 1.0.0 can be a genuinely finished product with a defined compatibility window, followed by maintenance rather than an endless alpha.

> **Success statement:** A user with a fresh Windows 11 installation, a 4K display at 150% scaling, and MediaPortal 1.39 x64 can install, configure, save, and uninstall the Studio without a command line, manual file copying, or configuration loss.

### What "final" means

- 1.0.0 is supported and documented, not a snapshot.
- Every shipped claim has a test or a documented limitation.
- The product can say "not supported" and fall back safely.
- Validation comes from automation and documented self-use, not from tester count.
- After 1.0.0 the project enters maintenance mode.

---

## 2. Goals and non-goals

### Goals

- Make the 61 native configuration pages readable and navigable at 100–175% UI scaling.
- Preserve every native page, control, value, validation rule, and save handler.
- Offer search across pages, setting labels, and option items.
- Offer light/dark presentation and a collapsible help column.
- Never leave the user without a usable native configurator.
- Ship as a signed, installable, bilingual product with a clear support matrix.

### Non-goals

- Rebuilding MediaPortal settings or save logic.
- Supporting MediaPortal 2 or MediaPortal versions other than 1.39.
- Theming arbitrary third-party plugin dialogs.
- Cloud features, telemetry, auto-update pipelines, or a plugin marketplace.
- Changing MediaPortal itself, its skin, or its binaries.

---

## 3. Principles

1. **Native remains the source of truth.** No setting, page lifecycle, or save handler is reimplemented.
2. **The worst case is the native UI.** If the shell cannot attach, the original configurator must remain usable.
3. **Write only to our own files.** `MediaPortal.xml` is written exclusively by native handlers.
4. **Claim only what is validated.** No scale option, architecture, or plugin promise without a passing gate.
5. **German and English.** Hardcoded UI strings are a release blocker.
6. **Offline by default.** No telemetry, no mandatory network access.
7. **Small scope, deep verification.** A finished product is allowed to do less.

---

## 4. Supported matrix (1.0 target)

| Dimension | 1.0 target | Gate |
| --- | --- | --- |
| MediaPortal | 1.39, x64 | Supported |
| MediaPortal | 1.39, x86 | Not shipped in 1.0; revisit only with an x86 test runtime |
| Windows | Windows 11 23H2+ | Supported |
| Windows | Windows 10 22H2 | Best effort, not a 1.0 gate |
| Runtime | .NET Framework 4.8 | Required |
| UI scaling | 100, 115, 125, 150% | Supported; 175% is conditional on M2 |
| OS DPI | 100–200%, single monitor | Supported; mixed multi-monitor best effort |
| Languages | German, English | Auto-detect from MediaPortal, manual override |
| Themes | Light, dark | Supported; Windows High Contrast uses system colors |
| Plugins | Tier A fully tested, Tier B native look, Tier C blocked | Tier B is the default |
| Other MP versions | None | Refuse with a clear message and classic fallback |

---

## 5. Scope of 1.0.0

### Included

- Grouped navigation with icons, navigation history, keyboard control.
- Search across page titles, setting labels, check/radio/combo/list options, with ranking and result highlighting.
- Light/dark shell, validated UI scaling, collapsible help column.
- German and English UI, language detection from MediaPortal.
- Transactional attach with classic fallback.
- Compatibility manifest and plugin tiers.
- Backup and restore of `MediaPortal.xml` (and optionally the full config folder, explicitly opted in).
- Structured logging and a sanitized support bundle.
- Signed installer, uninstaller, portable ZIP, SHA256 checksums.
- Documentation with screenshots, support matrix, known limitations.
- Optional manual update check that opens the GitHub release page. No auto-download.

### Explicitly not included

- Reimplementation of any setting or save handler.
- MP2, MP 1.38, or other MediaPortal versions.
- Theming of third-party plugin dialogs beyond a base palette.
- Remote configuration, cloud sync, telemetry, or a plugin marketplace.
- Automatic updates.
- Changes to the MediaPortal installation or skin.

### Deferred to post-1.0

- MPE1 packaging through the MediaPortal Extension Installer.
- Additional languages beyond German and English.
- Theme import/export.
- Deeper plugin-specific theming.
- Per-page help content beyond links to the official documentation.

---

## 6. Target architecture

Source layout, compiled into one deployable application assembly:

```
src/App        WinExe, ships as MP1ConfigurationStudio.exe
src/Core       presentation state, compatibility policy, logging
src/Native     native contract, attach transaction, zoom, theme
diagnostics/   Studio.Diagnostics.csproj, test and review host
```

| Source | Responsibility |
| --- | --- |
| `src/Core` | Appearance schema and migration, compatibility policy, structured logging |
| `src/Native` | `NativeContract`, AppDomain host, transactional attach, page zoom, theme and control chrome |
| `src/App` | Shell layout, navigation, search, support bundle, About, optional update check, CLI (`--classic`) |
| `diagnostics/` | Fixture tests, native smoke checks, audit/persistence/save-cycle harnesses, performance harness, screenshot renderer |

### Deployment decision

The application compiles Core and Native sources into one assembly. This is deliberate: the app copies itself beside `Configuration.exe`, and a single-file deployment avoids missing-DLL failures for non-technical users. The folders keep the boundaries reviewable. A future split into separate DLLs requires the installer and updater to copy and verify every Studio assembly first.

### Deployment model

- **Installed mode (primary):** setup copies only `MP1ConfigurationStudio*` files beside `Configuration.exe`, registers an uninstaller, and leaves all native files untouched.
- **Portable mode (secondary):** ZIP with the same files; the user selects the MediaPortal folder on first launch.
- **Appearance and logs:** `%LOCALAPPDATA%\MP1 Configuration Studio\`.
- **No services, drivers, background processes, or scheduled tasks.**

### Runtime data

| File | Location | Written by |
| --- | --- | --- |
| `appearance.xml` | LocalAppData | Studio |
| `studio.log` | LocalAppData | Studio, rotated |
| `compatibility.xml` | Embedded default, optional override | Studio reads only |
| `backups\*.xml` | LocalAppData | Studio, on explicit user action |
| `MediaPortal.xml` | ProgramData | Native handlers only |

---

## 7. Compatibility strategy

### 7.1 Native contract

All reflected field, property, and type names live in one `NativeContract` class. On startup it validates every required member and writes a human-readable compatibility report before any UI is replaced. A missing member is a controlled abort, not an exception in the middle of the layout rebuild.

### 7.2 Attach transaction

The shell is built off-screen and swapped in only after it is complete. The native form is not cleared before the replacement is ready. Fault injection tests must prove that a failure at any stage leaves the native configurator usable.

### 7.3 Compatibility manifest

`compatibility.xml` maps each native section type to:

- `zoom`: allowed or native size only
- `theme`: full, base palette, or native
- `search`: enabled or page-level only
- `tier`: A, B, or C

Unknown section types default to Tier B. This is the escape hatch that makes a final release possible without testing every plugin.

### 7.4 Plugin tiers

| Tier | Meaning | Behavior |
| --- | --- | --- |
| A | Fully tested | Zoom, theme, search enabled |
| B | Opens and saves safely | Native look, shell navigation, limited theme |
| C | Known incompatible | Blocked with explanation and classic fallback |

The 1.0 release documents the Tier A list. Everything else is Tier B or C.

### 7.5 Classic mode

`--classic` and a visible menu item start the original configurator without the shell. This is both a support tool and the permanent escape hatch.

### 7.6 Unknown MediaPortal builds

Version 1.39 is checked via file version and Core.dll architecture. Unknown builds produce a warning and default to classic mode unless the user explicitly opts in. The compatibility report is attached to the support bundle.

---

## 8. Quality strategy

### 8.1 Test pyramid

| Layer | Scope | Frequency | Gate |
| --- | --- | --- | --- |
| Unit | Core logic, appearance, navigation map, search index, localization completeness | Every push | Required |
| Fixture UI | Shell behavior, theming, scaling, change tracking, paint probes | Every push | Required |
| Native smoke | Attach, 61 sections, save round trips, value preservation, fault injection | Every release candidate, self-hosted VM | Required |
| Manual matrix | OS, architecture, DPI, language, plugins, installer | Every release | Required |
| Visual regression | Shell rendered to PNG at each scale and theme | Every push | Required for shell |

Hosted CI cannot run MediaPortal. Native smoke runs on a self-hosted Windows runner with an isolated MP 1.39 runtime or is recorded as a manual gate. This limitation is stated openly; it is not replaced by fixture numbers.

### 8.2 CI

- `build`: restore, build all projects, run unit and fixture tests, produce a ZIP artifact.
- `screenshots`: render the shell at 100/125/150/175%, light and dark; upload and diff against baselines.
- `native-smoke`: manual trigger and release tags on a self-hosted runner; publishes an XML report.
- `package`: produce installer and portable ZIP from tagged commits.

### 8.3 Performance budgets

| Metric | Budget |
| --- | --- |
| Added startup time vs. native | ≤ 2 s |
| Page switch | ≤ 300 ms added |
| First search result | ≤ 150 ms |
| Studio memory overhead | ≤ 150 MB |
| GDI handles after 20 theme/scale cycles | stable, no growth |
| UI freeze | no block > 1 s |

### 8.4 Accessibility

- Complete keyboard path for navigation, search, theme, scale, and help.
- Visible focus on all custom-drawn controls.
- Accessible names and roles for custom buttons and tabs.
- Windows High Contrast disables the custom palette and owner-draw presentation.
- 200% OS scaling must not clip the shell.
- Narrator or NVDA smoke test before 1.0.

### 8.5 Data safety

- No writes to native configuration outside native handlers.
- Backup before the first restore and on explicit user request.
- Native tests run only in a marked isolated runtime.
- Production `Configuration.exe` and `MediaPortal.xml` hashes are monitored during verification.

---

## 9. Milestones

| Milestone | Duration (part-time) | Output | Exit gate |
| --- | --- | --- | --- |
| **M1 Foundation** | 3–4 weeks | Buildable, testable, observable, safe-to-attach foundation | CI green; no test code in the app; fault injection passes |
| **M2 Hardening** | 5–6 weeks | Compatibility policy, fallback, backups, matrix decisions | All pages and saves pass; 175% decided; x64-only matrix confirmed |
| **M3 Productization** | 5–6 weeks | Bilingual UI, search v2, installer, signing, docs | Clean install/uninstall on fresh Windows |
| **M4 Internal validation** | 1–2 weeks | Release-candidate evidence and optional small pilot | Automated and maintainer HTPC checks pass; no P1/P2 |
| **M5 RC → 1.0.0** | 2–3 weeks | Frozen, independently verified final release | All 1.0 exit criteria |

### M1 Foundation

**Goal:** make the project maintainable and safe before adding product features.

Workstreams:

1. Split the monolithic source into Core, Native, App, and Diagnostics.
2. Replace the legacy `csc.exe` build with MSBuild/Roslyn and deterministic output.
3. Add CI, artifact upload, and repository governance.
4. Introduce `NativeContract` and a transactional attach.
5. Move tests and review harnesses out of the shipped executable.
6. Add structured logging, global exception handling, and version identity.
7. Inventory all 61 section types for the compatibility manifest.
8. Add a performance and support-bundle skeleton.

**Exit gate:**

- A clean clone builds on a clean Windows VM.
- CI is green and uploads an artifact.
- The release executable contains no test or review entry points.
- Fault injection at every attach stage leaves the native form usable.
- `NativeContract` produces a compatibility report.
- No user-visible feature changes beyond version/About metadata.

Detailed tickets: [docs/M1-TICKETS.md](docs/M1-TICKETS.md).

### M2 Hardening

**Progress (2026-11-13):** classic mode, Tier C blocking, and configuration backup/restore are implemented. Remaining: Tier A validation, High Contrast, 175% decision, attach performance, and the DPI/theme matrix.

- Fill `compatibility.xml` for all 61 section types.
- Implement Tier A/B/C behavior and per-page opt-out.
- Implement classic mode and unknown-build handling.
- Add backup/restore and the sanitized support bundle.
- Implement High Contrast fallback.
- Validate 175% or remove it.
- Confirm the x64-only matrix and stop building or shipping x86 binaries.
- Expand native smoke tests and the DPI/theme matrix.
- Fix all P1/P2 findings.

**Exit gate:**

- 61/61 pages activate; 57/57 writable sections save; 68/68 field round trips.
- Unknown plugin pages degrade to Tier B without blocking.
- Fault injection and data-safety checks pass.
- The 175% decision is recorded; the x64-only matrix is reflected in the shipped product.
- No open P1/P2 issues.

### M3 Productization

- Full German and English localization; no hardcoded UI strings.
- Search v2: pages, labels, check/radio/combo/list options, ranking, highlighting, debounce.
- Installer, uninstaller, upgrade path, portable ZIP.
- Code signing and SmartScreen preparation.
- Backup/restore UI and support bundle UI.
- Screenshots, README, support matrix, troubleshooting and classic-mode docs.
- Optional update check and About dialog.

**Exit gate:**

- Fresh Windows 10 and 11: install, configure, save, uninstall, rollback.
- Setup and binaries are signed and timestamped.
- Localization check passes; no truncated labels at supported scales.
- Support bundle contains no setting values or credentials.
- Search coverage matrix is documented.

### M4 Internal validation

- Run the full automated matrix: fixture, visual regression, native smoke, fault injection, and save/reload cycles.
- Use the release candidate for all of the maintainer's own configuration changes for at least seven days.
- Install, upgrade, and uninstall on a clean Windows 11 VM and on the real HTPC.
- Record known limitations and support-matrix results.
- Optional: invite zero to five pilot users. Their feedback is informative, not a release gate.
- No feature work during the validation window.

**Exit gate:**

- No open P1/P2 issues.
- At most three P3 issues with documented workarounds.
- Automated, clean-VM, and real-HTPC checks pass.
- Self-use period completed without data loss or blocked workflows.
- Pilot feedback, if any, is triaged and recorded.

### M5 RC → 1.0.0

- Feature freeze; only release blockers.
- Independent verification on fresh VMs and at least one real HTPC.
- Complete the release checklist with linked evidence.
- Sign, package, publish, and verify the downloaded artifacts.
- Publish release notes, support policy, and known limitations.

**Exit gate:** all criteria in section 12.

---

## 10. Risk register

| Risk | Likelihood | Impact | Mitigation | Trigger |
| --- | --- | --- | --- | --- |
| Reflection contract breaks | Low (1.39 frozen) | Total failure | `NativeContract`, hash/version check, classic fallback | Startup report shows missing member |
| Plugin page breaks under zoom/theme | High | Support load | Compatibility manifest, Tier B default, per-page opt-out | Tester report or native smoke failure |
| SmartScreen/AV blocks download | High | No adoption | Authenticode, timestamping, Microsoft submission, no obfuscation, source availability | Fresh-VM download warning |
| x86 requested after 1.0 | Low | 32-bit MP users cannot use Studio | Documented x64-only support; revisit only with an x86 test runtime | x86 request |
| Data loss through user error | Low | Catastrophic | Own files only, backup/restore, native handlers, isolated tests | Any write outside allowed paths |
| Support burden exceeds capacity | High | Project stalls | Support matrix, tiers, diagnostics bundle, documented non-goals | Issue volume or unresolved P3s |
| No independent testers | Medium | Blind spots | Automated matrix, seven-day self-use, optional 0–5 pilot, conservative support matrix | First user report |
| MP1 audience shrinks | High long-term | Reduced relevance | Small scope, maintenance mode, no over-investment | Download/issue trend |
| Localization drifts | Medium | Mixed UI | Strings in data files, CI check, translator guide | CI hardcoded-string check fails |
| Windows update regression | Medium | Visual/DPI breakage | Annual regression on current Windows builds | New Windows release |
| Licensing/attribution issue | Low | Legal | GPL-3.0 compatibility, SPDX headers, NOTICE, no MP binaries | Review finding |

---

## 11. Versioning and release policy

- Semantic versioning: `1.0.0`, `1.0.1`, `1.1.0`.
- Pre-releases use `v1.0.0-beta.1` and `v1.0.0-rc.1`. The word "alpha" is retired.
- Assembly version `1.0.0.0`; product display `1.0.0 for MediaPortal 1.39`.
- Every release has a changelog entry and signed artifacts.
- Artifacts: setup EXE, portable ZIP, `SHA256SUMS.txt`, release notes, archived PDBs.
- Support window: 24 months of security and compatibility fixes after 1.0.0, reviewed annually.
- 1.1 is reserved for environment-forced changes (Windows or MediaPortal), not feature creep.
- Old releases remain available; assets are never replaced in place.

---

## 12. Definition of Done for 1.0.0

1. 61/61 sections activate; 57/57 writable sections save; 68/68 field round trips; two full save/reload cycles on a fresh profile.
2. Fresh install, upgrade, uninstall, and rollback pass on Windows 11 x64; Windows 10 is best effort if a VM is available.
3. No open P1/P2 issues for four weeks; all P3 issues have documented workarounds.
4. No hardcoded UI strings; German and English are complete.
5. Fault injection at every attach stage leaves the native configurator usable.
6. Startup, page switch, search, memory, and GDI budgets pass.
7. Setup and binaries are signed and timestamped; SHA256 published; fresh-VM SmartScreen check recorded.
8. A seven-day self-use period is complete; optional pilot feedback (zero to five users) is triaged.
9. README, support matrix, known limitations, troubleshooting, and release notes are complete.
10. The release checklist is fully checked or has written waivers.

---

## 13. After 1.0.0

- Maintenance mode: security fixes, Windows regressions, plugin compatibility updates.
- Annual regression run on current Windows 10/11 builds and a current MP 1.39 installation.
- No new features without a written 1.1 decision tied to an environment change.
- Issue triage targets: acknowledge within 7 days, fix or document within 30 days.
- Archive build artifacts and PDBs for every release.

---

## 14. Open decisions

| Decision | Options | Deadline | Owner |
| --- | --- | --- | --- |
| x86 support | Decided: x64 only for 1.0; no x86 binary | Done | Maintainer |
| 175% scaling | Keep if all pages pass / remove from selector | End of M2 | Maintainer |
| Code signing | SignPath OSS / OV certificate / Azure Trusted Signing | Start of M3 | Maintainer |
| Installer technology | Inno Setup / WiX / custom setup | Start of M3 | Maintainer |
| MPE1 packaging | Include / defer to post-1.0 | Start of M3 | Maintainer |
| Update check | Manual link only / none | Start of M3 | Maintainer |
| Theme "System" option | Include / keep light-dark only | M2 | Maintainer |
| Support channel | GitHub Issues / forum thread / both | M4 | Maintainer |

---

## 15. Timeline

Assumes 10–15 focused hours per week.

| Phase | Weeks | Cumulative |
| --- | --- | --- |
| M1 Foundation | 3–4 | 4 |
| M2 Hardening | 5–6 | 10 |
| M3 Productization | 5–6 | 16 |
| M4 Internal validation | 1–2 | 18 |
| M5 RC → 1.0.0 | 2–3 | 21 |

The critical path is native verification and the DPI/plugin matrix, not feature development. External testers are optional; the release relies on automation and a documented self-use period.

# Release Checklist — MP1 Configuration Studio 1.0.0

Copy this file to `docs/releases/1.0.0-CHECKLIST.md` when the release process starts and fill in the identity block. Every unchecked item is a release blocker unless a written waiver with rationale is linked.

---

## Release identity

| Field | Value |
| --- | --- |
| Version | `1.0.0` |
| Git tag | `v1.0.0` |
| Commit | `<sha>` |
| Build date (UTC) | `<date>` |
| Built by | `<name / CI run>` |
| Signing identity | `<certificate subject>` |
| Supported MediaPortal | `1.39 x64` |
| Setup SHA256 | `<hash>` |
| Portable ZIP SHA256 | `<hash>` |
| Release manager | `<name>` |

---

## 1. Scope and freeze

- [ ] Scope matches the 1.0 section of `ROADMAP.md`.
- [ ] Feature freeze is in effect; only release blockers are being merged.
- [ ] All deferred items are recorded in the roadmap.
- [ ] Every known limitation is written in the release notes.
- [ ] No dependency upgrades after the release candidate without a re-run of this checklist.

**Evidence:** `<link to milestone / freeze commit>`

---

## 2. Repository and code

- [ ] `main` is protected and required checks are green.
- [ ] No test, audit, review, or persistence harness is present in the shipped executable.
- [ ] No hardcoded user-visible strings in source; localization check passes.
- [ ] SPDX headers and `NOTICE.md` are current.
- [ ] Production code has no third-party runtime dependencies beyond the .NET Framework.
- [ ] No secrets, personal paths, user configuration, or large binaries in the repository.
- [ ] Version is consistent across project files, installer, About dialog, and docs.
- [ ] The `0.1.0-alpha.1` release is marked as superseded.

**Evidence:** `<CI run / commit>`

---

## 3. Build and reproducibility

- [ ] A clean clone builds on a clean Windows VM with the documented toolchain.
- [ ] The build produces x64 binaries; no x86 binary is shipped.
- [ ] Deterministic build verified, or non-determinism is documented and accepted.
- [ ] Build log and warnings are archived.
- [ ] PDBs are archived for the release.
- [ ] The build script does not depend on a developer machine, local MP installation, or NuGet feed at release time.
- [ ] The artifact contains no test runtime marker or local evidence files.

**Evidence:** `<build log / artifact listing>`

---

## 4. Automated verification

- [ ] Unit tests pass.
- [ ] Fixture UI tests pass (record count: `__ / __`).
- [ ] Visual regression screenshots pass at every shipped scale (100/115/125/150%, plus 175% if shipped) in light and dark.
- [ ] Fault-injection attach test passes.
- [ ] Native smoke x64 passes (record count: `__ / __`).
- [ ] Native smoke x86 is not required: 1.0 ships x64 only. If an x86 binary is ever added, this item becomes mandatory.
- [ ] Localization completeness test passes.
- [ ] No test writes outside the isolated runtime.
- [ ] Test reports are attached to the release evidence.

**Evidence:** `<CI run / XML reports>`

---

## 5. Native integration and data safety

- [ ] All 61 native sections activate.
- [ ] All 57 writable sections save successfully.
- [ ] 68/68 field changes save, reload, and restore.
- [ ] Two complete OK-save / fresh-process reload cycles pass.
- [ ] Cancel discards changes and does not write configuration.
- [ ] Theme and scale changes preserve all captured values.
- [ ] Page navigation preserves unsaved values.
- [ ] Plugin dialogs open, theme, and cancel without changing their configuration files.
- [ ] Original `Configuration.exe` and native DLL hashes are unchanged after verification.
- [ ] `MediaPortal.xml` hash is unchanged during isolated verification.
- [ ] Studio writes only `appearance.xml`, `studio.log`, backups, and its own files.
- [ ] Backup and restore work on a copy of a real configuration.
- [ ] The isolated runtime marker is required and enforced.
- [ ] Classic mode starts the unmodified configurator.

**Evidence:** `<native smoke report / hash comparison>`

---

## 6. Functional acceptance

- [ ] First launch detects MediaPortal 1.39 and selects the correct architecture.
- [ ] Installer and portable mode both start correctly.
- [ ] Grouped navigation shows all sections and correct icons.
- [ ] Navigation history back/forward works across groups.
- [ ] Keyboard shortcuts work: Ctrl+F, Ctrl+Plus/Minus, Alt+Left/Right, F1, Escape.
- [ ] Search finds pages, setting labels, and check/radio/combo/list options.
- [ ] Search ranks exact page matches above option matches.
- [ ] Selecting a result opens the page and highlights the target.
- [ ] Light and dark themes apply to shell and supported native controls.
- [ ] High Contrast mode uses system colors and remains readable.
- [ ] Every shipped scale step renders without clipping or overlap.
- [ ] Help column expands, collapses, and persists.
- [ ] Change tracking marks edited fields and clears when the original value returns.
- [ ] Tier A plugins work; Tier B plugins open and save; Tier C plugins are blocked with an explanation.
- [ ] An unknown plugin does not break the shell.
- [ ] Classic mode and the classic menu item work.
- [ ] Error dialog offers the support bundle and the classic fallback.
- [ ] About dialog shows version, supported MP version, license, and project link.
- [ ] Optional update check opens the release page and sends no data.

**Evidence:** `<manual test record / screenshots>`

---

## 7. Compatibility matrix

Test every row and record the result. If the 175% option is removed at the M2 gate, drop that scale from the rows.

| # | Windows | MP architecture | OS DPI | Studio scale | Language | Configuration | Result |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Windows 11 23H2+ | x64 | 100% | 100/125/150/175% | DE | Fresh | `<pass/fail>` |
| 2 | Windows 11 23H2+ | x64 | 150% | 100/125/150/175% | EN | Migrated | `<pass/fail>` |
| 3 | Windows 11 23H2+ | x64 | 200% | 100/125/150/175% | DE | Fresh | `<pass/fail>` |
| 4 | Windows 11 23H2+ | x64 | 100/150% | 125% | DE | Multi-monitor, mixed DPI | `<pass/fail>` |
| 5 | Windows 11 | x64 | 100% | 150% | EN | Plugin Tier A | `<pass/fail>` |
| 6 | Windows 11 | x64 | 150% | 125% | DE | Plugin Tier B | `<pass/fail>` |
| 7 | Windows 11 | x64 | 150% | 125% | DE | Plugin Tier C | `<pass/fail>` |
| 8 | Windows 11 | x64 | 150% | 125% | DE | No TV tuner / no TV Server | `<pass/fail>` |
| 9 | Windows 10 22H2 | x64 | 150% | 125% | DE | Best effort if a VM is available | `<pass/fail>` |
| 10 | Clean Windows 11 VM | x64 | 100% | 125% | EN | Fresh install | `<pass/fail>` |

- [ ] At least one real HTPC with a TV display was tested.
- [ ] At least one test used a non-administrator Windows account for normal use.
- [ ] Results are linked in the release evidence.

### Self-use and optional pilot

- [ ] The release candidate was used for all maintainer configuration changes for at least seven days.
- [ ] The self-use log records pages used, save cycles, and any workarounds.
- [ ] If pilot users were invited, their number (0–5) and feedback are recorded.
- [ ] If no pilot was used, the release evidence states that the release relies on automated and maintainer validation.

---

## 8. Installer, upgrade, uninstall, and rollback

- [ ] Fresh install on a clean Windows VM succeeds with the expected UAC prompt.
- [ ] Installer detects the MediaPortal folder correctly, including a non-default path.
- [ ] Installer refuses or warns when MediaPortal 1.39 is missing.
- [ ] Upgrade from the previous public release or release candidate preserves appearance settings.
- [ ] Uninstall removes only `MP1ConfigurationStudio*` files and its uninstall entry.
- [ ] Uninstall leaves `Configuration.exe`, native DLLs, and `MediaPortal.xml` untouched.
- [ ] After uninstall, the original configurator still starts.
- [ ] Rollback to the previous Studio version works.
- [ ] Portable ZIP runs without installation and writes only to LocalAppData.
- [ ] Silent install/uninstall is either supported and tested or explicitly not offered.
- [ ] No leftover temporary files after install, update, or uninstall.

**Evidence:** `<installer logs / before-after file listing>`

---

## 9. Localization and accessibility

- [ ] German UI is complete and reviewed.
- [ ] Native page labels follow the MediaPortal language.
- [ ] English shell localization is documented as deferred to 1.1.
- [ ] No shell language override is offered in 1.0.
- [ ] No text is truncated at any shipped scale.
- [ ] Shell is fully usable with the keyboard only.
- [ ] Focus is visible on all custom controls.
- [ ] Screen reader smoke test with Narrator or NVDA passes.
- [ ] High Contrast mode is readable and does not use custom-drawn colors.
- [ ] 200% OS scaling does not clip the shell.

**Evidence:** `<screenshots / accessibility notes>`

---

## 10. Performance

- [ ] Added startup time is ≤ 2 s versus the native configurator.
- [ ] Page switch adds ≤ 300 ms.
- [ ] First search result appears within ≤ 150 ms.
- [ ] Studio memory overhead is ≤ 150 MB.
- [ ] GDI handles remain stable after 20 theme/scale cycles.
- [ ] No UI freeze longer than 1 s during navigation, search, theme, or scale changes.
- [ ] Measurements are recorded with hardware and OS details.

**Evidence:** `<performance report>`

---

## 11. Security, signing, and antivirus

- [ ] Setup executable is Authenticode-signed and timestamped.
- [ ] Shipped executables are Authenticode-signed and timestamped.
- [ ] Signature verification passes on a clean VM.
- [ ] SmartScreen behavior on a fresh VM is recorded.
- [ ] Microsoft Defender scan of the installer and installed files is clean.
- [ ] Samples were submitted to Microsoft if SmartScreen reputation is insufficient.
- [ ] SHA256 checksums are published.
- [ ] No unexpected network connections at startup or during normal use.
- [ ] Elevation is requested only for installation or the initial file copy.
- [ ] No telemetry, analytics, or hidden update requests.
- [ ] The support bundle contains no setting values, credentials, or personal paths.
- [ ] The privacy statement is current and accurate.

**Evidence:** `<signature output / scan report / network capture>`

---

## 12. Documentation and support

- [ ] README explains what the product is, who it is for, and what it does not do.
- [ ] Screenshots show navigation, search, themes, and scaling.
- [ ] Install, upgrade, uninstall, and rollback are documented.
- [ ] Supported matrix is documented and matches the shipped product.
- [ ] Known limitations are documented.
- [ ] Troubleshooting covers classic mode and support bundles.
- [ ] Issue templates for bugs and compatibility reports exist.
- [ ] `SECURITY.md` explains how to report vulnerabilities.
- [ ] Changelog and release notes are complete.
- [ ] Validation evidence is linked from the README.
- [ ] GPL-3.0 license and `NOTICE.md` are present.
- [ ] The project is clearly marked as unofficial and not affiliated with Team MediaPortal.

**Evidence:** `<docs links>`

---

## 13. Artifacts and distribution

- [ ] `MP1-Configuration-Studio-1.0.0-Setup.exe` is signed and has a SHA256 sidecar.
- [ ] `MP1-Configuration-Studio-1.0.0-Portable.zip` is built from the same commit and has a SHA256 sidecar.
- [ ] `SHA256SUMS.txt` covers every distributed file.
- [ ] Release notes are final and match the artifacts.
- [ ] GitHub release is a draft until all checks pass.
- [ ] Asset names contain the version and no `alpha`/`beta` suffix for the final release.
- [ ] Downloading from the public release and verifying hashes works on a clean machine.
- [ ] No old release asset was overwritten.

**Evidence:** `<release draft link / hash verification>`

---

## 14. Post-release verification

- [ ] Install from the public release asset on a clean VM.
- [ ] Run the full smoke path: start, navigate, search, theme, scale, save, cancel, exit.
- [ ] Confirm the downloaded installer signature and hash.
- [ ] Confirm the native configurator is unchanged.
- [ ] Monitor issues for 72 hours.
- [ ] Publish the support note and known limitations.
- [ ] Archive build logs, PDBs, and test reports.
- [ ] Tag is immutable; no force-push after release.

---

## 15. Sign-off

| Role | Name | Decision | Date |
| --- | --- | --- | --- |
| Release manager | `<name>` | Approve / Reject | `<date>` |
| Native integration reviewer | `<name>` | Approve / Reject | `<date>` |
| Installer/security reviewer | `<name>` | Approve / Reject | `<date>` |
| Maintainer | `<name>` | Approve / Reject | `<date>` |

**Waivers:** `<link or none>`

**Final decision:** `<release / hold>`

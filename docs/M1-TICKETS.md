# M1 — Foundation: Tickets

**Milestone:** M1 Foundation
**Goal:** Make the project buildable, testable, observable, and safe to attach before any product feature work.
**Duration:** 3–4 weeks part-time (~22 person-days)
**Exit gate:** CI green, no diagnostics in the shipped app, fault-injected attach leaves the native UI usable, `NativeContract` reports compatibility, internal M1 tag created.

Each ticket below is written so it can be pasted into GitHub Issues as-is. Recommended labels: `m1`, `foundation`, plus the track label.

---

## Summary

| ID | Title | Track | Estimate | Depends on |
| --- | --- | --- | --- | --- |
| M1-01 | Archive the 0.1.0-alpha.1 state | Governance | 0.5 d | – |
| M1-02 | Split the solution into four projects | Architecture | 2 d | M1-01 |
| M1-03 | Replace the legacy csc build with MSBuild/Roslyn | Build | 1 d | M1-02 |
| M1-04 | Add CI build, tests, and artifact upload | Build | 1 d | M1-03 |
| M1-05 | Remove tests and review harnesses from the app | Diagnostics | 1.5 d | M1-02 |
| M1-06 | Introduce `NativeContract` with startup validation | Native safety | 2 d | M1-02 |
| M1-07 | Make attach transactional with fault injection | Native safety | 2.5 d | M1-06 |
| M1-08 | Add structured logging and global exception handling | Observability | 1 d | M1-02 |
| M1-09 | Centralize versioning and add an About dialog | Product | 0.5 d | M1-02 |
| M1-10 | Repository governance, templates, and disclaimers | Governance | 1.5 d | M1-01 |
| M1-11 | Migrate fixture tests to a real test runner | Diagnostics | 2 d | M1-05 |
| M1-12 | Add the compatibility manifest skeleton | Native safety | 1.5 d | M1-06 |
| M1-13 | Appearance schema v2 with migration and recovery | Core | 1 d | M1-02 |
| M1-14 | Add performance baseline measurements | Diagnostics | 1 d | M1-05 |
| M1-15 | Add the support bundle skeleton | Observability | 1.5 d | M1-08 |
| M1-16 | Write the security and threat model note | Governance | 1 d | M1-08 |
| M1-17 | Produce the first CI-built portable artifact | Build | 0.5 d | M1-03, M1-04, M1-05 |

---

## Track A — Build and repository

### M1-01 — Archive the 0.1.0-alpha.1 state

**Goal:** Preserve the current public alpha as a historical reference and make the new direction unambiguous.

**Work:**
- Tag the current commit as `v0.1.0-alpha.1-archive`.
- Add a short note to the existing release that it is superseded by the 1.0 roadmap.
- Pin the old release in the README's history section.
- Do not delete any existing assets.

**Acceptance criteria:**
- The archive tag exists and points at the published alpha commit.
- The release notes link to `ROADMAP.md`.
- README clearly states that `0.1.0-alpha.1` is not the supported line.

**Estimate:** 0.5 d

---

### M1-02 — Split the solution into four projects

**Goal:** Replace the monolithic compile set with clear boundaries.

**Work:**
- Create `Studio.Core`, `Studio.Native`, `Studio.App`, and `Studio.Diagnostics`.
- Move types to their target project without changing behavior.
- Keep namespaces stable or add minimal forwarding where necessary.
- Add project references; no circular dependencies.
- Update the solution file and build docs.

**Acceptance criteria:**
- All four projects build from a clean clone.
- `Studio.App` references Core and Native; Diagnostics references Core and Native.
- Core has no WinForms dependency where avoidable.
- No behavior change in the fixture tests.
- The dependency graph is documented in the README or architecture doc.

**Estimate:** 2 d

---

### M1-03 — Replace the legacy csc build with MSBuild/Roslyn

**Goal:** Use a maintainable, modern compiler toolchain while staying on .NET Framework 4.8.

**Work:**
- Convert to SDK-style projects targeting `net48`.
- Enable deterministic builds and portable PDBs where possible.
- Keep production dependency-free; test framework packages are allowed in Diagnostics only.
- Replace `build.ps1` with a single documented entry point.
- Remove the `Framework64\v4.0.30319\csc.exe` dependency.
- Produce an x64 application output; 1.0 does not ship x86.

**Acceptance criteria:**
- A clean Windows VM with the documented toolchain builds without Visual Studio.
- The application output is x64. The x86 build target is removed from the release script.
- Build is deterministic or the remaining non-determinism is documented.
- No production NuGet dependency.
- Build log is readable and warnings are not suppressed globally.

**Estimate:** 1 d

---

### M1-04 — Add CI build, tests, and artifact upload

**Goal:** Every push is built and tested automatically.

**Work:**
- Add a GitHub Actions workflow for `windows-latest`.
- Restore, build all projects, run unit and fixture tests.
- Upload a portable ZIP artifact and test reports.
- Add a status badge to the README.
- Ensure no secrets or personal paths are required.

**Acceptance criteria:**
- Pull requests run build and tests.
- A failed test fails the workflow.
- The artifact is downloadable from the run.
- The workflow does not attempt to run MediaPortal; native smoke remains a separate gate.

**Estimate:** 1 d

---

## Track B — Architecture and native safety

### M1-05 — Remove tests and review harnesses from the app

**Goal:** The shipped executable contains only product code.

**Work:**
- Move `ShellTests`, `StudioReview`, `StudioPersistenceReview`, `StudioSaveCycle`, and `StudioPluginReview` into `Studio.Diagnostics`.
- Remove `--self-test`, `--audit`, `--review`, `--persistence`, `--save-test`, and `--verify` from the app entry point.
- Keep `--classic` as the only public diagnostic switch.
- Add an internal `--diagnostics` only if needed for support, and document it.
- Verify the app has no reference to the test namespace.

**Acceptance criteria:**
- The release executable has no test or review entry points.
- `Studio.Diagnostics.exe` runs the fixture suite and native checks.
- The app binary does not reference Diagnostics.
- A CI check fails if test types reappear in the app assembly.

**Estimate:** 1.5 d

---

### M1-06 — Introduce `NativeContract` with startup validation

**Goal:** One explicit, testable definition of every native integration point.

**Work:**
- Centralize field, property, and type names used by reflection: form fields, section dictionary, TreeNode section property, debug settings, config folders, settings API.
- Validate required and optional members separately.
- Produce a human-readable report (version, architecture, missing members).
- Add unit tests with fake forms and missing members.
- Document how to update the contract for a new MP build.

**Acceptance criteria:**
- A missing required member produces a structured report and a controlled abort.
- The report is written to the log before any UI replacement.
- No native UI is modified when validation fails.
- Tests cover present, missing, and wrong-type members.

**Estimate:** 2 d

---

### M1-07 — Make attach transactional with fault injection

**Goal:** A failure at any stage leaves the native configurator usable.

**Work:**
- Build the shell off-screen.
- Do not clear or reparent native controls until the replacement is complete.
- Add explicit fault-injection hooks at each attach stage (validation, shell build, theme, layout, first refresh).
- Add tests that inject a failure at every hook.
- Ensure cleanup releases resources and restores focus/visibility.
- Show one clear error message with the classic fallback.

**Acceptance criteria:**
- Every injected failure leaves the native form with all original controls and handlers.
- The native form remains closable and does not save accidentally.
- The error message names the stage and points to the log.
- No partially themed or partially reparented state remains.
- Tests run in CI with the fixture form.

**Estimate:** 2.5 d

---

### M1-12 — Add the compatibility manifest skeleton

**Goal:** Make per-page behavior configurable before plugin hardening begins.

**Work:**
- Inventory all 61 native section types.
- Define `compatibility.xml`: section type, tier, zoom, theme, search.
- Default unknown types to Tier B.
- Load and validate the manifest in Core.
- Add a test that every known section type has an entry.
- Document the schema and how to add an override.

**Acceptance criteria:**
- 61/61 known section types are present.
- Unknown types resolve to Tier B.
- Invalid entries fail validation with a clear message.
- The manifest is embedded and can be overridden by a local file.
- No runtime behavior change beyond the resolved policy object.

**Estimate:** 1.5 d

---

## Track C — Diagnostics and observability

### M1-08 — Add structured logging and global exception handling

**Goal:** Support can diagnose failures without asking users for screen recordings.

**Work:**
- Add a rolling log in `%LOCALAPPDATA%\MP1 Configuration Studio\logs`.
- Log levels: error, warning, info, debug.
- Never log setting values, credentials, or full user paths where avoidable.
- Add global handlers for UI-thread and AppDomain exceptions.
- Add a crash dialog with log path and support-bundle action.
- Include a correlation ID per session.

**Acceptance criteria:**
- Startup, attach, navigation, theme, and scale events are logged.
- An injected exception produces a log entry and a controlled dialog.
- Log rotation is bounded (for example 2 MB × 3 files).
- No setting values or secrets appear in logs.
- Tests cover the logger and the sanitizer.

**Estimate:** 1 d

---

### M1-11 — Migrate fixture tests to a real test runner

**Goal:** Per-test reporting and isolation instead of one sequential method.

**Work:**
- Choose NUnit or xUnit.
- Convert the 95 fixture checks into individual tests with meaningful names.
- Add categories: unit, fixture, paint, scaling, theming, navigation.
- Produce TRX or XML reports in CI.
- Keep the existing coverage; do not silently drop checks.

**Acceptance criteria:**
- 95/95 checks pass as individual tests or a documented equivalent.
- A single failure does not stop unrelated tests.
- CI publishes a readable report.
- Test names map to the original check names in a migration table.

**Estimate:** 2 d

---

### M1-14 — Add performance baseline measurements

**Goal:** Make performance regressions visible before they become user complaints.

**Work:**
- Add a harness in Diagnostics for startup, page switch, first search, memory, and GDI handles.
- Record results as JSON with hardware and OS details.
- Encode the roadmap budgets as assertions where stable enough.
- Document how to run the harness on a reference machine.

**Acceptance criteria:**
- Baseline file is committed with the reference machine description.
- Repeated runs are stable enough to detect a 20% regression.
- GDI handle count is sampled before and after 20 theme/scale cycles.
- Budgets from the roadmap are linked.

**Estimate:** 1 d

---

### M1-15 — Add the support bundle skeleton

**Goal:** One click produces a safe, useful diagnostic archive.

**Work:**
- Collect app version, MP version, architecture, OS build, DPI, scale, theme, language.
- Include logs and the compatibility report.
- Include the list of section types and tiers, without values.
- Exclude `MediaPortal.xml`, credentials, plugin data, and personal paths.
- Write a ZIP to a user-selected location and show the path.

**Acceptance criteria:**
- Bundle contains no setting values or credentials.
- Bundle opens with standard tools and includes a `README.txt`.
- Missing optional files do not break creation.
- Sanitization is covered by a test with planted secrets.

**Estimate:** 1.5 d

---

## Track D — Product and governance

### M1-09 — Centralize versioning and add an About dialog

**Goal:** One version identity for app, installer, docs, and support.

**Work:**
- Set product version to `1.0.0` and assembly version to `1.0.0.0`.
- Remove the mirrored MediaPortal `1.39.x` product version.
- Show version, supported MP version, architecture, license, and project link in About.
- Include the build commit when available.

**Acceptance criteria:**
- Version is consistent in file metadata, About, and docs.
- "For MediaPortal 1.39" is presented as compatibility, not product version.
- About is reachable from the help menu.

**Estimate:** 0.5 d

---

### M1-10 — Repository governance, templates, and disclaimers

**Goal:** A public repository that looks and behaves like a maintained project.

**Work:**
- Rewrite README: what, why, install, support matrix, screenshots placeholder, limitations.
- Add `ROADMAP.md`, `docs/RELEASE-CHECKLIST.md`, `docs/M1-TICKETS.md`.
- Add `SECURITY.md`, `CONTRIBUTING.md`, `SUPPORT.md`.
- Add issue templates for bug reports and compatibility reports.
- Add a pull-request template with test and evidence fields.
- Add SPDX headers and update `NOTICE.md`.
- Add a clear "unofficial, not affiliated with Team MediaPortal" statement.
- Add a short decision log.

**Acceptance criteria:**
- All links resolve.
- Issue templates ask for version, OS, DPI, MP architecture, plugin, and log bundle.
- README states the support matrix and the non-goals.
- License and attribution are unambiguous.

**Estimate:** 1.5 d

---

### M1-16 — Write the security and threat model note

**Goal:** Make the unusual integration behavior explicit and reviewable.

**Work:**
- Document self-copy into the MediaPortal folder, elevation, AppDomain loading, plugin code execution, and file writes.
- Document what is never written and what is never sent over the network.
- List mitigations: signing plan, hashes, no obfuscation, source availability, support bundle sanitization.
- Add a vulnerability reporting process.

**Acceptance criteria:**
- A reviewer can understand the trust boundaries in under 15 minutes.
- Every unusual behavior has a rationale and a mitigation.
- `SECURITY.md` links to the threat model.

**Estimate:** 1 d

---

### M1-13 — Appearance schema v2 with migration and recovery

**Goal:** Presentation preferences survive upgrades and corruption.

**Work:**
- Add a schema version to `appearance.xml`.
- Migrate v1 files without data loss.
- Keep atomic write via temp file and replace.
- Fall back to defaults on parse errors and report the fallback.
- Add tests for valid, legacy, missing, and corrupted files.

**Acceptance criteria:**
- Existing `appearance.xml` loads and preserves theme, scale, size, and context state.
- Corrupted files do not crash or block startup.
- The file is never left half-written.
- Migration is documented.

**Estimate:** 1 d

---

## Track E — First artifact

### M1-17 — Produce the first CI-built portable artifact

**Goal:** End M1 with a runnable, reproducible artifact.

**Work:**
- Package the app, config, and required metadata into a ZIP from CI.
- Add a short smoke test on a clean VM: start, navigate one page, close.
- Publish SHA256 with the artifact.
- Document how to run it without an installer.

**Acceptance criteria:**
- Artifact is produced from a tagged commit.
- It contains no tests, review harnesses, or local evidence.
- It starts on a clean Windows 11 x64 VM with MP 1.39 installed.
- SHA256 is published.
- Installer work remains out of scope.

**Estimate:** 0.5 d

---

## Suggested sequence

| Week | Tickets |
| --- | --- |
| 1 | M1-01, M1-02, M1-03, M1-10 |
| 2 | M1-04, M1-05, M1-06, M1-09, M1-13 |
| 3 | M1-07, M1-08, M1-11, M1-12 |
| 4 | M1-14, M1-15, M1-16, M1-17, milestone review |

M1-06 and M1-07 are the critical path. Start them as early as the project split allows.

---

## Explicitly out of scope for M1

- New user-visible features or visual redesign.
- Localization content and language switching.
- Installer, uninstaller, and code signing.
- The 175% scaling decision.
- Plugin tier behavior beyond the manifest skeleton.
- Search v2.
- Backup/restore implementation.

---

## Definition of Done for M1

- All M1 tickets are closed or explicitly deferred with a written reason.
- CI is green on `main` and required for merges.
- The release executable contains no test or review entry points.
- Fault injection proves the native UI survives every attach failure.
- `NativeContract` writes a compatibility report before any UI change.
- A portable artifact is produced from CI and starts on a clean VM.
- No user-visible feature changes beyond version and About metadata.
- Internal tag `v1.0.0-m1` is created.
- A five-minute demo shows: normal start, fault-injected start, classic mode, support bundle.

# SignPath Foundation — free code signing

> **Repository move pending:** The final repository is planned to move toward the official MediaPortal organization. Do not submit the SignPath application until the final repository URL is known. SignPath binds projects, origin verification, and signing policies to the repository URL; applying before the move would require reconfiguring the project afterwards. Tags and releases normally follow a GitHub repository transfer. After the move, update this document, the README policy links, the release URLs, and the SignPath project repository URL, then apply. See [REPOSITORY-MOVE.md](REPOSITORY-MOVE.md).

The only free route that produces a **Windows-trusted** signature is the SignPath Foundation program:

- Website: https://signpath.org/
- Terms: https://signpath.org/terms.html
- Apply: https://signpath.org/apply
- SignPath login: https://app.signpath.io/

SignPath issues the certificate to **SignPath Foundation**, not to the project owner. The foundation therefore appears as the publisher. The private key stays on SignPath's HSM, and every release signing request needs manual approval.

## Current status

The project is prepared for SignPath, but the application has **not been submitted or approved** yet. The published `1.0.0` is unsigned by documented waiver. Once approved, a signed `1.0.1` (or later) can be produced without changing the application code.

## Eligibility checklist

| Requirement | Project status |
| --- | --- |
| OSI-approved license without commercial dual-licensing | GPL-3.0 — yes |
| No proprietary components | Yes |
| Actively maintained | Yes |
| Already released in the form to be signed | 1.0.0 is published |
| Documented functionality | README and release notes |
| Code signing policy on the project home page | README section “Code signing policy” |
| Privacy policy | `PRIVACY.md` |
| Named committers/reviewers/approvers | `@gheitmann` |
| MFA on GitHub and SignPath | Must be enabled by the maintainer |
| Verifiable automated build from source | GitHub Actions |
| Project reputation | **Open risk:** the repository is new; SignPath may decline until downloads/stars/community coverage grow |

SignPath explicitly states that it requires a certain verifiable reputation for executable programs and that it can reject projects.

## Application

1. Create a SignPath account with GitHub and enable MFA.
2. Apply at https://signpath.org/apply with:
   - Repository: https://github.com/gheitmann/MP1-Configuration-Studio
   - Release page: https://github.com/gheitmann/MP1-Configuration-Studio/releases/tag/v1.0.0
   - Description: modern, accessible shell for the MediaPortal 1.39 configurator
   - License: GPL-3.0-or-later
   - Reputation: releases, download counts, stars, forum/community references once available
3. Wait for manual review. SignPath may ask for additional information.
4. After approval, create a project:
   - Slug: `MP1_Configuration_Studio`
   - Trusted build system: GitHub.com
   - Artifact configurations: `app` and `setup` (see `.signpath/artifact-configurations/`)
   - Signing policies: `release-signing` restricted to `main` and `v*` tags; optional `test-signing`
5. Create an API token with submitter permission.
6. Add GitHub configuration:
   - Secret: `SIGNPATH_API_TOKEN`
   - Variables: `SIGNPATH_ORGANIZATION_ID`, `SIGNPATH_PROJECT_SLUG`, `SIGNPATH_SIGNING_POLICY_SLUG`, `SIGNPATH_APP_CONFIG_SLUG`, `SIGNPATH_SETUP_CONFIG_SLUG`
7. Run a test tag (for example `v1.0.1-rc.1`) and approve the signing request in the SignPath dashboard.

## Workflow behavior

`.github/workflows/release.yml`:

- Builds and tests the application.
- If `SIGNPATH_ORGANIZATION_ID` is configured, uploads the unsigned application, submits it to SignPath, downloads the signed result, and uses it for the portable ZIP and setup.
- The setup is then submitted as a second signing request so the downloaded installer is also signed.
- If no SignPath configuration and no PFX certificate exist, the workflow requires the documented `ALLOW_UNSIGNED_FINAL` waiver for a final release.
- If `SIGNING_PFX_BASE64` exists, the local PFX path takes precedence.

SignPath requires one manual approval per signing request in the dashboard.

## Artifact configurations

- `.signpath/artifact-configurations/app.xml` signs `MP1ConfigurationStudio.exe`.
- `.signpath/artifact-configurations/setup.xml` signs `MP1-Configuration-Studio-*-Setup.exe`.

Both expect a ZIP artifact with the executable at the archive root, as staged by the release workflow.

## Alternatives

| Route | Cost | Trusted by Windows | Verdict |
| --- | --- | --- | --- |
| SignPath Foundation | Free | Yes, SignPath Foundation publisher | Best free option; approval and reputation required |
| Certum Open Source | From ~€49/year (2026) | Yes | No longer free |
| Azure Artifact Signing / Trusted Signing | ~$10/month | Yes | Not free; fallback for an organisation |
| Microsoft Store MSIX | Free individual account | Yes, Store-signed | Not suitable: admin tool writes beside MediaPortal |
| Sigstore / cosign | Free | No SmartScreen trust | Integrity/transparency only |
| Self-signed certificate | Free | No | Does not remove “Unknown Publisher” |

## If SignPath declines

Options:

1. Improve project reputation (downloads, stars, community posts, release history) and reapply later.
2. Use Certum's paid Open Source certificate if the project remains non-commercial.
3. Use Azure Artifact Signing if a small monthly fee is acceptable.
4. Keep publishing unsigned with SHA256 sidecars and the documented waiver.

Do not claim that self-signed or Sigstore signatures remove Windows SmartScreen warnings; they do not.

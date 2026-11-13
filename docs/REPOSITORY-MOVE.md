# Repository move checklist

The final repository is planned to move toward the official MediaPortal organization. Use this checklist when the destination is known.

## Before the transfer

- [ ] All release-candidate work is merged into `main`.
- [ ] `v1.0.0` release and assets are verified.
- [ ] A local mirror or bundle exists as a backup.
- [ ] The destination organization and repository name are confirmed.
- [ ] License and attribution review is complete (GPL-3.0, no MediaPortal binaries redistributed).
- [ ] Open issues and pull requests are listed; close or transfer them deliberately.

## Transfer

- [ ] Transfer the repository in GitHub settings.
- [ ] Verify that tags, releases, assets, and old URLs redirect.
- [ ] Update the local remote:

  ```powershell
  git remote set-url origin https://github.com/<new-org>/MP1-Configuration-Studio.git
  git fetch origin
  ```

- [ ] Re-check branch protection and required status checks.
- [ ] Re-create repository secrets and variables if they did not transfer:
  - `SIGNING_PFX_BASE64`, `SIGNING_PFX_PASSWORD` (if used)
  - `ALLOW_UNSIGNED_FINAL`
  - `SIGNPATH_API_TOKEN`
  - `SIGNPATH_*` variables

## After the transfer

- [ ] Update absolute repository URLs in README, release notes, issue templates, and docs.
- [ ] Install the SignPath GitHub App on the new organization and grant repository access.
- [ ] Apply to SignPath Foundation with the final repository URL and release page.
- [ ] Create the SignPath project and artifact configurations after approval.
- [ ] Update `docs/SIGNPATH.md` with organization/project/policy slugs.
- [ ] Re-run the build workflow on `main`.
- [ ] Tag a signed re-release (`v1.0.1` or later) after SignPath approval.
- [ ] Verify the signed app and setup with `Get-AuthenticodeSignature`.
- [ ] Publish SHA256 sidecars and update the support/download links.
- [ ] Announce the new repository location and the signed release.

## Do not

- Do not submit the SignPath application before the final repository URL is known.
- Do not re-upload assets under the same names with different hashes.
- Do not claim a signed release until the signature verifies as `Valid`.

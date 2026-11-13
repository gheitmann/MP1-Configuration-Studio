# Privacy Policy

MP1 Configuration Studio is an offline desktop application for the MediaPortal 1.39 configurator.

## Network access

This program will not transfer any information to other networked systems unless specifically requested by the user or the person installing or operating it.

- No telemetry, analytics, crash upload, or usage reporting.
- No background update check.
- No cloud synchronization.
- The application starts the locally installed `Configuration.exe`; it does not contact a server.

## Local data

The Studio writes only its own files:

- `%LOCALAPPDATA%\MP1 Configuration Studio\appearance.xml` — theme, window size, scale.
- `%LOCALAPPDATA%\MP1 Configuration Studio\logs\studio.log` — diagnostic log, rotated, with token/password patterns redacted.
- `%LOCALAPPDATA%\MP1 Configuration Studio\backups\` — user-requested and first-run backups of `MediaPortal.xml`.
- Studio files beside the MediaPortal installation.

`MediaPortal.xml` and plugin configuration are written only by the original MediaPortal handlers. Backups contain `MediaPortal.xml` and may therefore contain credentials or service tokens; they are local-only and are deliberately excluded from support bundles.

## Support bundles

A support bundle is created only when the user requests it. It contains version, environment, compatibility policy, section types, appearance settings, and logs. It does not contain `MediaPortal.xml`, plugin configuration, setting values, credentials, or media paths.

## Third-party components

MediaPortal, its plugins, and their data remain subject to their own privacy policies. The Studio does not bundle or redistribute them.

## Contact

Use the repository issue tracker for privacy questions. Do not attach `MediaPortal.xml` or backups to public issues.

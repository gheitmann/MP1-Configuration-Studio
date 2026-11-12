# Security Policy

## Supported versions

| Version | Supported |
| --- | --- |
| `1.0.x` | Yes, once released |
| `0.1.0-alpha.1` | No; historical preview only |

## Reporting a vulnerability

Please use GitHub's private vulnerability reporting via the **Security** tab of this repository. Do not open a public issue for a suspected vulnerability.

Include:

- Affected version and architecture.
- Windows version.
- MediaPortal version and architecture.
- Steps to reproduce.
- Impact and any proof of concept.
- Whether user configuration or credentials could be exposed.

We aim to acknowledge reports within 7 days and to provide an assessment within 30 days. These are targets, not a commercial support guarantee.

## Trust boundaries

The Studio is unusual by design. This section makes the behavior explicit.

- **Self-copy into the MediaPortal folder.** The app copies only its own `MP1ConfigurationStudio*` files beside `Configuration.exe` so that `Application.StartupPath` and plugin probing paths stay correct. It never replaces native binaries.
- **Elevation.** Administrator rights may be requested to write those files into `Program Files`. The app does not elevate for normal configuration work.
- **AppDomain loading.** The unmodified native `Configuration.exe` runs in a child AppDomain inside the Studio process. Native code therefore runs with the same user rights as the Studio.
- **Plugin code.** MediaPortal plugins execute in-process with the same rights. The Studio does not sandbox them and cannot certify third-party plugin behavior.
- **File writes.** The Studio writes `appearance.xml`, logs, backups, and its own files. `MediaPortal.xml` and plugin configuration are written only by native handlers.
- **Network.** Normal operation is offline. The optional update check opens the GitHub release page in the default browser and sends no telemetry.
- **Support bundle.** The bundle contains versions, logs, DPI, and a sanitized page list. It must not contain setting values, credentials, or personal paths. This is covered by tests.

## Explicit non-goals

- Sandboxing MediaPortal or third-party plugins.
- Protecting against a compromised local administrator account.
- Providing a hosted service or collecting telemetry.
- Certifying hardware, TV tuners, remotes, or plugin behavior.

## Disclosure

Once a fix is available, we will publish a GitHub security advisory and credit the reporter unless anonymity is requested.

# Contributing

Thanks for considering a contribution. This project integrates deeply with a frozen MediaPortal 1.39 installation; correctness and data safety matter more than feature speed.

## Before you start

- Read [ROADMAP.md](ROADMAP.md) and the current milestone tickets.
- For native integration changes, read the architecture notes and `SECURITY.md`.
- Open an issue before starting a large change so the approach can be agreed first.

## Requirements

- Windows 10 22H2 or Windows 11.
- .NET Framework 4.8.
- PowerShell 5.1.
- MediaPortal 1.39 for native integration tests. Native tests must use an isolated runtime, never a live user profile.

## Build and test

Until the M1 build modernization lands, use the current script:

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1 -TestX86
```

The build writes outputs and test logs to `dist/`. All 95 fixture checks must pass for x64 and x86.

Native integration checks require a separately staged MediaPortal runtime with the isolation marker. Follow the existing tooling and never point persistence tests at a live profile.

## Branching and pull requests

- `main` is protected; use short-lived branches.
- One logical change per pull request.
- Reference the ticket ID in the title, for example `M1-07: make attach transactional`.
- Fill in the pull request template, including the tests you ran and the evidence.
- Do not commit `dist/`, logs, personal configuration, credentials, or generated test receipts.

## Code rules

- Preserve native control identity, event handlers, and save behavior. Do not replace native pages or settings.
- New code uses one statement per line. Existing compact code may be reformatted only in the area you touch.
- No new production dependency without prior agreement. Test-only dependencies are acceptable in the diagnostics project.
- Never write to `MediaPortal.xml` or plugin configuration directly.
- Any change to attach, theme, zoom, or navigation requires a fault-injection check and a fixture test.
- Add or update a test for every behavior change. A fix without a regression test is incomplete.

## Native integration checklist

- [ ] Control identity and native handlers preserved.
- [ ] All affected sections still activate.
- [ ] Writable sections still save and reload.
- [ ] Theme and scale changes preserve values.
- [ ] Attach failure leaves the native UI usable.
- [ ] No writes outside the allowed files.
- [ ] Evidence attached to the pull request.

## License

By contributing, you agree that your contribution is licensed under GPL-3.0-or-later, matching the project license. Do not add code or assets with incompatible terms.

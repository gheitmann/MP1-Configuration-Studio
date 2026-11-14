# MP1 Configuration Studio plugin 2.0.0-beta.33

**Status:** Preview aligned with desktop Studio beta.33. Unsigned preview.

## Fixed

- Own focus model for Up/Down/Left/Right/OK/Back; footer actions are reachable.
- Row labels refresh immediately after a value change.
- Direct atomic `MediaPortal.xml` writes with ZIP backup instead of the shared `MPSettings` cache; unrelated settings are no longer rewritten on MediaPortal shutdown.
- Save status states that a MediaPortal restart is required.

## Verified in MediaPortal

- Navigation, value toggle, save, backup, back to home, and persistence after MediaPortal exit.
- Plugin builds with 0 warnings/errors; `ISetupForm` smoke test passes (window 8917).

## Limits

- Unsigned.
- Changes require a MediaPortal restart.

# MP1 Configuration Studio 2.0.0-beta.23

**Status:** Twenty-third preview. DSP test playback now applies DynAmp and Compressor through BASS FX. Unsigned preview. Not installed locally (no UAC run).

## New in beta.23

### DSP test playback with full effects

- Loads BASS_FX through Bass.Net (`BassFx.LoadMe`) before opening the stream.
- Applies **Gain** as stream volume.
- Applies **DynAmp** with the selected Soft/Medium/Hard preset through `BASS_FX_BFX_DAMP`.
- Applies **Compressor** with the configured threshold through `BASS_FX_BFX_COMPRESSOR`.
- Uses the same preset values as Bass.Net:
  - Soft: target 0.92, quiet 0.02, rate 0.01, gain 1, delay 0.5
  - Medium: target 0.94, quiet 0.03, rate 0.01, gain 1, delay 0.35
  - Hard: target 0.98, quiet 0.04, rate 0.02, gain 2, delay 0.2
- Compressor threshold is converted from the stored dB×10 value to a linear level.
- Test dialog status reports which effects were applied or the BASS error code.

### Keymap windows

- Window add/delete was verified end-to-end:
  - Added window `9999 / Test Window` and saved it to `keymap.xml`.
  - Deleted it through the UI and saved; the resulting file has 22 windows and no `9999`.
- Global remains protected.

## Validation

- Fixture checks: **224/224** on x64.
- DSP test playback with Gain + DynAmp + Compressor reported:
  **„Wiedergabe mit Gain, DynAmp, Compressor …“**
- Keymap round-trip: window 9999 added, then removed; XML checked after both saves.
- The installed beta.14 remains active because installing still requires elevation.

## Install

The app is **not installed** by this build; the previously installed version remains active until an elevated copy is approved. The portable ZIP/setup is available from the release.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.

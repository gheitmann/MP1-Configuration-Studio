# MP1 Configuration Studio 2.0.0-beta.27

**Status:** Twenty-seventh preview. Adds native Daemon Tools detection/path checking and applies VST plugins in DSP test playback. Unsigned preview. Not installed locally (no UAC run).

## New in beta.27

### Virtual drive panel

The **BD / DVD → Virtuelles Laufwerk** group is now a native panel instead of plain text fields:

- **Detect Daemon Tools** through the 32/64-bit registry views (`DAEMON Tools Lite`, `DAEMON Tools Pro`, `DiscSoft`) and the default install folders.
- **Browse …** for the program if detection fails.
- **Pfad prüfen** verifies that the configured file exists and shows its size.
- **Erkannten Pfad übernehmen** applies a detected installation path.
- Edits enable, extensions, drive number, and ask-before-playing directly in the `daemon` section.
- Unknown values remain preserved; the section is excluded from the automatic “Weitere Einstellungen” group so nothing appears twice.

### VST in DSP test playback

- VST entries from `musicdsp.xml` are resolved relative to `musicplayer\plugins\dsp` and applied through `bass_vst` (`BASS_VST_ChannelSetDSP`).
- `bass_vst` is loaded through Bass.Net (`BassVst.LoadMe`).
- The status reports applied plugins or errors, e.g. `Wiedergabe mit Gain, DynAmp, Compressor, VST …`.
- **Safety guard:** files that are not PE DLLs (or are suspiciously small) are skipped with a log entry instead of being passed to the VST host, which previously could block on an invalid placeholder file.
- Winamp DSP entries remain editable in `musicdsp.xml`; applying Winamp DSP needs a Winamp host and is not part of this test dialog.

## Validation

- Fixture checks: **224/224** on x64.
- Virtual drive round-trip: path set through the panel and saved to `daemon/path`; drive spinner changed and saved to `daemon/driveNo`.
- Path check reported **„Pfad ist gültig · 606.720 Bytes“**; detection correctly reported that Daemon Tools is not installed on the test system.
- DSP test log:
  - `bass_fx loaded=True`
  - `bass_vst loaded=True plugins=1`
  - `VST skipped, not a plugin DLL: …\TestVST.dll`
- Playback status: **„Wiedergabe mit Gain, DynAmp, Compressor, VST skipped TestVST …“** — no hang, all other effects active.
- Production configuration untouched.

## Install

The app is **not installed** by this build; the previously installed version remains active until an elevated copy is approved. The portable ZIP/setup is available from the release.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.

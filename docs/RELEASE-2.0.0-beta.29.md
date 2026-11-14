# MP1 Configuration Studio 2.0.0-beta.29

**Status:** Twenty-ninth preview. Adds real keymap conflict resolution and modifier guidance. Unsigned preview. Not installed locally (no UAC run).

## New in beta.29

### Key conflict resolution

When a captured key is already used by another action **in the same context**, the editor now asks before changing anything:

```
Die Taste 2 ist bereits belegt durch:

• Remote Button 2 (#60)

Taste bei diesen Aktionen entfernen und hier zuweisen?
```

- **Ja** removes the key from the listed actions and assigns it to the edited action.
- **Nein** leaves both actions unchanged.
- The same check runs when adding a new action.
- Conflicts are only searched within the selected context (Global or one window), matching MediaPortal's scoped key handling.
- After resolution the grid is rebuilt and the status reports `Konflikt aufgelöst`.

### Modifier combinations

MediaPortal's native `keymap.xml` stores single key names only. This was confirmed against MediaPortal 1.39:

- `Core/guilib/key.cs` exposes only a `KeyCode`, no modifier flags.
- The shipped base/keymap files contain no Ctrl/Alt/Shift combinations.

Pressing **Ctrl** or **Alt** during capture therefore shows an inline explanation instead of silently dropping the modifier or writing an unsupported value:

> keymap.xml speichert nur einzelne Tasten. Strg-/Alt-Kombinationen sind nicht Teil der nativen MediaPortal-Tastenbelegung; bitte die Taste ohne Strg oder Alt drücken.

Users who need modifier chords need the separate InputPlugin/remote mapping, not `keymap.xml`.

## Validation

- Fixture checks: **230/230** on x64.
- New keymap checks:
  - duplicate detection within a context
  - conflict clearing
  - window scoping (a key in another window is not treated as a conflict)
- UI round-trip:
  - captured `2` on `Remote Button 1` (id 59), which was already used by `Remote Button 2` (id 60)
  - conflict dialog listed `Remote Button 2 (#60)`
  - chose **Ja**, saved, verified `keymap.xml`: id 59 = `2`, id 60 empty
  - new ZIP backup created
- Modifier warning screenshot verified with a real Ctrl+A key press.
- Production `keymap.xml` untouched; all edits used the test configuration.

## Install

The app is **not installed** by this build; the previously installed version remains active until an elevated copy is approved. The portable ZIP/setup is available from the release.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.

# MP1 Configuration Studio 2.0.0-beta.47

**Status:** Forty-seventh preview. Adds per-entry defaults to the raw XML editor and highlights values that differ from the MediaPortal default. Unsigned preview.

## Raw XML: reset and highlight

- Every entry now has **two** actions:
  - `×` removes the entry after confirmation (unchanged).
  - `↺` resets it to the **MediaPortal 1.39 default** when a default is known, otherwise to the value that was last loaded/saved in this session.
- The reset button tooltip names the target value and whether it is a MediaPortal default or the last saved value.
- Entries whose current value differs from the known MediaPortal default are drawn in the accent colour (labels and values, including boolean rows).
- The hint line above the list explains `×`, `↺` and the highlight.

## Default source

- Defaults were extracted from the classic MediaPortal 1.39 configuration sources (`GetValueAsString/Int/Bool` call sites).
- Only literal string, number and boolean defaults are used; empty defaults and symbolic expressions are ignored so that codecs, paths and identifiers are not falsely flagged.
- **351** defaults are bundled. Entries without a known default are not highlighted; reset falls back to the last saved value.

## Validation

- Fixture checks: **261/261** on x64 (defaults catalogue, saved-value fallback, themed About dialog).
- Verified in the running app: `watchdogEnabled = yes` (default `no`) is highlighted, the `↺` column is aligned for text and boolean rows, reset tooltips resolve.
- Release workflow remains green.

## Install

The combined setup installs application and optional plugin together. No configuration format changes.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.

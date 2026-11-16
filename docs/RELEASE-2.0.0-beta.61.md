# MP1 Configuration Studio 2.0.0-beta.61

**Status:** Sixty-first preview. Tooltips everywhere and plugin overview fine-tuning. Unsigned preview.

## Tooltips

- Every settings field now shows a tooltip on hover: the field's help text (English or German) or, when no help text exists, the MediaPortal default value from the classic configuration sources ("Default: …" / "Standard: …").
- Tooltips are attached to checkboxes, labels and inputs including composite controls (filter combos, folder pickers), so they appear wherever the pointer rests.
- Sidebar entries show their section subtitle as a tooltip.

## Plugin overview

- Tiles now carry rich tooltips: the plugin's own description and author, plus the quick-actions hint.
- Plugins that cannot be disabled (for example the built-in Home plugin) now report `CanEnable` from the plugin assembly; the "Enable / disable" action and the context-menu entry are disabled for them and the status line notes "cannot be disabled" ("nicht deaktivierbar").
- The overview writes only deltas against the original MediaPortal.xml state: enabling a plugin again no longer leaves unexpected pending entries for keys that were absent originally (for example `pluginswindows`). Reverting an action clears its pending entries cleanly.
- New filter box at the top of the overview: type to narrow the grid by plugin name, description or category; group headers hide when empty.

## Validation

- Fixture checks: **276/276** on x64.
- Verified at runtime in the light theme: filter box, tile tooltips, `CanEnable` gating (Home plugin), the disable → enable round trip ending in "Ready" without pending entries, field tooltips with help text and with the default-value fallback.
- Production `MediaPortal.xml` was not modified by the Studio.

## Install

The combined setup installs application and optional plugin together.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. MediaPortal artwork is used for identification only.

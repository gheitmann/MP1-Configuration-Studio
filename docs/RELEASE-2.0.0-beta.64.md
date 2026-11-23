# MP1 Configuration Studio 2.0.0-beta.64

**Status:** Sixty-fourth preview. x86 support: the Studio now ships for both MediaPortal architectures. Unsigned preview.

## Architectures

- The Studio now ships as **x64 and x86** builds from the same source.
- The **setup detects the installed MediaPortal architecture** (registry view of the MediaPortal uninstall entry) and installs the matching build under the usual `MP1ConfigurationStudio.exe` name. One setup for both.
- The **portable ZIP contains both** executables: `MP1ConfigurationStudio.exe` (x64) and `MP1ConfigurationStudio.x86.exe` (x86). Pick the one that matches your MediaPortal installation (x64 is the default for modern installs).
- The **MediaPortal plugin** is architecture-neutral (AnyCPU/MSIL) and works with both MediaPortal x64 and x86 without changes.
- Rationale: the Studio loads native MediaPortal components in-process (`bass.dll`/`bass_vst.dll` for DSP test playback, DirectShow filter property pages for codecs). Native libraries only load in a process of the same bitness, so the Studio must match the MediaPortal installation.

## Build

- `build.ps1` now builds both flavors, verifies the PE headers (x64 = 0x8664, x86 = 0x14C) and copies the x86 image next to the x64 build for packaging.
- Setup scripts allow installation on all Windows architectures (`x86compatible`) instead of x64-only.

## Validation

- Fixture checks: **285/285** on x64.
- x86 build verified: correct PE header, starts as a 32-bit process and creates its main window against the isolated test runtime.
- Setup compiled with the new architecture detection; the x64 path is the one used on this machine.
- Production `MediaPortal.xml` was not modified by the Studio.

## Limits

- The x86 build could not be exercised against a real 32-bit MediaPortal installation here (no MP x86 on this machine); codec property pages and DSP test playback on x86 are therefore verified only structurally (same code, matching bitness).
- A future release may move native parts into a helper process (single AnyCPU build with bitness-matched helper); the x86 build remains the interim solution.

## Install

The combined setup installs application and optional plugin together, choosing the Studio architecture automatically.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. MediaPortal artwork is used for identification only.

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**EveProbeFormations** is a Windows Forms desktop application (C# .NET 10.0-windows) that lets EVE Online players manage custom scan probe formations. It reads and edits binary EVE player configuration files (`core_user_*.dat`) stored in `%LocalAppData%\CCP\EVE`.

## Build & Run

```bash
dotnet build EveProbeFormations.csproj
dotnet run --project EveProbeFormations/EveProbeFormations.csproj
```

There is no automated test framework. See the manual testing guidelines in AGENTS.md.

## Architecture

### Three-Form UI Flow
1. **frmProfileSelector** — Scans the EVE installation directory and shows discovered player profiles with aliases
2. **frmProbeFormationSelector** — Lists probe formations for the selected profile
3. **frmFormationEditor** — Edits individual probe coordinates (X/Y/Z) and diameter

### Core Classes
- **Probe** (`Probe.cs`) — 3D position + diameter; stores diameter internally in meters, converts to/from AU via `DiameterAU` property
- **FormationSegment** (`FormationSegment.cs`) — Up to 8 probes; serializes to bytes via `ToBytes()`
- **UserProfileProcessor** (`UserProfileProcessor.cs`) — Byte-level parser/writer for EVE's binary profile files
- **Helper** (`Helper.cs`) — EVE path detection, settings I/O, internet time fetch, AU conversion utilities, import/export encryption
- **CryptoHelper** (`CryptoHelper.cs`) — AES encrypt/decrypt for formation export blobs (intentionally weak, obfuscation only)
- **Settings** / **UserAlias** — JSON-backed alias store mapping EVE user IDs to friendly names

## Key Conventions

**Naming:** Windows Forms use `frm` prefix; classes PascalCase; private fields `_camelCase`.

**AU Conversion:** Always use the `DiameterAU` property accessors — do not perform manual conversions.
```csharp
DiameterAU = DimeterRaw / 149_597_870_700d;   // meters → AU
DimeterRaw = DiameterAU * 149_597_870_700d;   // AU → meters
```

**Probe validity:** `DimeterRaw > 1.0` (strictly greater, not `>=`); X/Y/Z/DimeterRaw must not be NaN.

## Binary File Parsing

EVE profile files use these byte delimiters:
- `0x2e` — field separator
- `0x15` — formation/segment delimiter
- `0x2c` — data boundary marker

**Critical:** `UserProfileProcessor` splits files into `preSegmentBytes + probeData + postSegmentBytes`. Always reconstruct in this order on save to avoid corrupting EVE's file structure.

## Special Modes & Expiration

- **Unlocked mode:** Create an empty file named `I accept all risks of running this tool unlocked` in the app directory to disable safety restrictions.
- **Expiration check:** Hardcoded to **May 16, 2026**. On startup, `Helper.GetApproximateInternetTime()` is called (falls back to EVE ESI API). Never remove this check without an explicit request.

## Common Pitfalls

- Always use `Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)` — never hardcode paths.
- Re-enable parent form on child form close; use the `FormClosing` event for modal cleanup.
- Use `double` (not `float`) for all AU/astronomical calculations.
- Test binary format changes against backup EVE profile files before committing.

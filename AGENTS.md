# AI Agent Guidelines for EveProbeFormations

## Project Overview

**EveProbeFormations** is a Windows Forms application that enables EVE Online players to manage custom scan probe formations. It reads EVE player profiles from the local game installation, parses binary configuration files, and allows users to edit probe positioning (X/Y/Z coordinates and diameter in AU).

- **Technology Stack:** C# .NET 10.0-windows, Windows Forms, Newtonsoft.Json
- **Output Type:** WinExe (standalone desktop application)
- **License:** Apache 2.0

## Architecture at a Glance

### Three-Form UI Flow
1. **frmProfileSelector** — Scans local EVE installation; displays discovered player profiles with user aliases
2. **frmProbeFormationSelector** — Lists available probe formations for the selected profile
3. **frmFormationEditor** — Edits individual probe coordinates and diameter

### Core Data Model
- **Probe** — 3D position (X, Y, Z) + diameter; stores diameter in meters internally, converts to/from AU on access
- **FormationSegment** — Collection of up to 8 probes; represents one named formation
- **UserProfileProcessor** — Parses binary EVE `core_user_*.dat` files using byte-level delimiters
- **Settings** — JSON file mapping EVE user IDs to friendly aliases

---

## Key Conventions & Patterns to Follow

### Naming Conventions
- **Windows Forms:** `frm` prefix (e.g., `frmFormationEditor`, `frmProfileSelector`)
- **Classes:** PascalCase
- **Private fields:** `_camelCase`

### Data Persistence
- **Settings:** JSON (`settings.json`), loaded at app startup via `Helper.LoadSettings()`
- **EVE Profiles:** Binary files in `%LocalAppData%\CCP\EVE` matching pattern `core_user_*.dat`
- **Formation data:** Binary format with custom byte delimiters (see **Binary Format** section below)

### Critical Conversions & Formulas

**Astronomical Unit (AU) Conversion:**
```csharp
// Convert from meters (game internal) to AU
DiameterAU = DimeterRaw / 149_597_870_700d;

// Convert from AU (user input) to meters
DimeterRaw = DiameterAU * 149_597_870_700d;
```
Always use the property accessors (`DiameterAU` getter/setter) for safety; do not perform manual conversions.

**Probe Validity Check:**
A probe is valid only if:
- X, Y, Z, DimeterRaw are not NaN
- DimeterRaw > 1.0 (strictly greater than)

---

## Binary File Format & Parsing

### EVE Game File Structure
EVE stores player profiles in binary `core_user_*.dat` files. The application uses custom byte-level parsing:

**Key Delimiters:**
- `0x2e` (.) — Field separator
- `0x15` — Formation/segment delimiter
- `0x2c` (,) — Data boundary marker

**File Layout:**
- Header bytes (application-specific)
- Repeated **FormationSegment** blocks:
  - Segment name (string, null-terminated or delimiter-based)
  - Probe count (up to 8)
  - Probe data (X, Y, Z, diameter as bytes)
- Footer bytes (preserved on write-back to maintain game file integrity)

### Parsing Pattern
When modifying `UserProfileProcessor`, **always preserve pre- and post-segment byte arrays**:
```csharp
// Example pattern
byte[] preSegmentBytes = ...;
byte[] probeData = ...;
byte[] postSegmentBytes = ...;

// On save, reconstruct: preSegmentBytes + probeData + postSegmentBytes
```
This ensures the game file structure remains intact for EVE's own reader.

---

## Security & Special Modes

### Encryption (AES)
- Hardcoded encryption key and IV (intentionally weak—for obfuscation only, not production security)
- See `CryptoHelper.cs` for implementation
- Comments are intentionally humorous about the weak security

### Unlocked Mode
- Enabled by creating an empty file named `I accept all risks of running this tool unlocked` in the application directory
- Allows editing without safety restrictions
- **Default:** locked mode with safeguards

### Expiration Check (Program.cs)
- Built-in expiration date: **May 16, 2026**
- On startup, checks internet time; if expired, shows message and exits
- Calls `Helper.GetApproximateInternetTime()` for NTP-like check

---

## Development & Build

### Build Command
```bash
dotnet build EveProbeFormations.csproj
```

### Run Command
```bash
dotnet run --project EveProbeFormations/EveProbeFormations.csproj
```

### Project Structure
- **EveProbeFormations/** — Main application code
  - `Program.cs` — Entry point, application initialization, expiration check
  - `frmProfileSelector.cs / .Designer.cs / .resx` — First form (profile discovery)
  - `frmProbeFormationSelector.cs / .Designer.cs / .resx` — Second form (formation list)
  - `frmFormationEditor.cs / .Designer.cs / .resx` — Third form (probe editor)
  - `Probe.cs` — Data model with AU conversion logic
  - `FormationSegment.cs` — Formation container (up to 8 probes)
  - `UserProfileProcessor.cs` — Binary file parser for EVE data
  - `Settings.cs` — User alias management (JSON)
  - `Helper.cs` — Utilities (relative timestamps, internet time, settings loader)
  - `CryptoHelper.cs` — AES encryption/decryption
  - `UserDatFound.cs` — Profile metadata (path, name, user ID, alias, modification time)
  - `UserAlias.cs` — Alias data model

---

## Common Development Tasks

### Adding a New Field to Probe
1. Add property to `Probe.cs` (e.g., `public double NewField { get; set; }`)
2. Update validation in `CheckIsValid()`
3. Update `UserProfileProcessor` to parse/serialize the new field
4. Add corresponding UI control in `frmFormationEditor.Designer.cs`
5. Bind the control in `frmFormationEditor.cs` (update on focus loss)

### Modifying the Binary Format
1. Update parsing logic in `UserProfileProcessor.cs`
2. **Critical:** Preserve pre/post-segment bytes to avoid corrupting EVE's file
3. Test thoroughly with backup EVE profiles
4. Consider forward/backward compatibility if changing the format

### Adding User Settings
1. Add new property to `Settings.cs`
2. Update JSON structure and `Helper.LoadSettings()` deserialization
3. Persist to `settings.json` after changes

---

## Testing & Validation

**No automated test framework currently in place.** Manual testing guidelines:

1. **Profile Discovery:** Test with EVE installation at default path (`C:\Users\<user>\AppData\Local\CCP\EVE`)
2. **Formation Parsing:** Use backup profile files; verify byte sequences match expectations
3. **AU Conversion:** Verify bidirectional conversion: `X AU → meters → X AU`
4. **Form Modal Behavior:** Ensure parent form is disabled while child form is open
5. **Expiration:** Test expiration message (manually advance system clock or modify `Program.cs` check)

---

## Common Pitfalls & Notes

- **Do not hardcode file paths** — Always use `Environment.GetFolderPath()` for `LocalAppData`
- **Diameter validation is strict** — `DimeterRaw > 1.0`, not `>= 1.0`
- **AU conversion precision** — Use double, not float, for astronomical calculations
- **Binary file corruption** — If in doubt about byte structure, preserve the original bytes around edits
- **Modal form closing** — Ensure parent form re-enables on child form close (use `FormClosing` event)
- **Unlocked mode check** — Remember the file-based flag in Program.cs; test both locked and unlocked behavior

---

## When to Ask for Clarification

- **Binary format details** — If the delimiter byte sequence seems unclear, clarify with context (look at actual binary dumps if needed)
- **EVE game file compatibility** — If changing core parsing logic, verify with the repository owner
- **Expiration date updates** — Never remove the expiration check without explicit request; it's intentional
- **UI/UX changes** — Windows Forms has specific patterns; preserve modal behavior and event binding patterns

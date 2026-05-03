You are implementing a feature for HostGuard, an Among Us BepInEx lobby moderator mod at
C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard
(BepInEx plugin, .NET 6, Reactor 2.5.0, plugin ID "com.rareshonour.hostguard").

## Goal
Let the host export the whitelist and blacklist to files and import from files, so lists
can be transferred between machines or edited externally.

## Codebase context
- Blacklist.cs — static class. API: Load(), Save(), Add(string friendCode),
  Remove(string friendCode), Contains(string friendCode), GetAll().
  Persists to <BepInEx Config>/hostguard_blacklist.txt (one friend code per line).
  Add() returns false for duplicates (silently).
- Config.cs — WhitelistedCodes is a ConfigEntry<string> (comma-separated friend codes).
  HostGuardConfig.GetWhitelistedCodes() returns the parsed list.
- UI/HostGuardUI.cs lines 86–97 — existing whitelist display (read-only list with Remove
  buttons). Blacklist management likely nearby. Add new buttons here.
- UIFactory.cs — CreateToggle(), CreateTextRow() for building rows.
- ChatHelper.SendLocalMessage(string) — host-visible chat messages.
- HostGuardPlugin.Logger.LogInfo — logging.
- No localization; strings hardcoded in C#.

## Behaviour spec

**Blacklist export:**
- Button "Open Blacklist Folder" → calls
  System.Diagnostics.Process.Start("explorer.exe", BepInEx.Paths.ConfigPath)
  and ChatHelper.SendLocalMessage("[HostGuard] Blacklist file: hostguard_blacklist.txt")
  (the file already lives in that folder — no copying needed).

**Whitelist export:**
- Button "Export Whitelist" → writes each whitelisted friend code to
  <BepInEx Config>/hostguard_whitelist_export.txt (one code per line), then opens the
  folder via Process.Start and sends "[HostGuard] Whitelist exported to hostguard_whitelist_export.txt".

**Blacklist import:**
- Button "Import Blacklist" → reads <BepInEx Config>/hostguard_blacklist_import.txt
  (one friend code per line). For each line: trim, skip empty/whitespace, skip lines
  containing commas (invalid format). Call Blacklist.Add() for each valid code.
  Send "[HostGuard] Blacklist import: X codes added, Y duplicates skipped."
  If file not found, send "[HostGuard] Place hostguard_blacklist_import.txt in the
  BepInEx config folder, then press Import."

**Whitelist import:**
- Button "Import Whitelist" → reads <BepInEx Config>/hostguard_whitelist_import.txt
  (same one-per-line format). Append new codes (not already in whitelist) to
  HostGuardConfig.WhitelistedCodes.Value as comma-separated, then call
  HostGuardConfig.WhitelistedCodes.ConfigFile.Save() to persist.
  Send "[HostGuard] Whitelist import: X codes added, Y duplicates skipped."
  Same "file not found" message as blacklist if missing.

## Implementation hints
- No new config entries needed.
- Only edit UI/HostGuardUI.cs (add buttons to whitelist and blacklist sections).
- Blacklist.Add() already handles duplicates; just count false returns for the report.
- For whitelist: parse existing WhitelistedCodes.Value by comma-split before appending
  to avoid duplicates.

## Acceptance criteria
- [ ] "Open Blacklist Folder" opens BepInEx config folder and chat-notifies.
- [ ] "Export Whitelist" writes the file and opens the folder.
- [ ] "Import Blacklist" reads the import file, reports added/skipped counts.
- [ ] "Import Whitelist" appends new codes to config, reports counts.
- [ ] Missing import file sends a helpful instruction message instead of crashing.
- [ ] Duplicate codes skipped silently; reported in count.
- [ ] Invalid lines (empty, contains comma) skipped.
- [ ] Build succeeds.

## Out of scope
Native file picker, overwrite/replace mode (always append), removing all entries via import.

Before starting, create and switch to a new git branch named feature/import-export-lists.
Commit all changes to that branch when done.

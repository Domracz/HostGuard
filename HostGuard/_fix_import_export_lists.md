You are fixing the import/export whitelist/blacklist feature in HostGuard at
C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard

Check out the branch feature/import-export-lists and read UI/HostGuardUI.cs fully
before making any changes.

## Changes needed

**1. Button rename and separation (blacklist section)**
- Remove the current "Open Blacklist Folder" button (it opens Explorer automatically).
- Add an "Export Blacklist" button that ONLY writes the export file (no Explorer).
- Add a separate "Open Folder" button (one per section, or one shared) that opens the
  BepInEx config folder via:
  System.Diagnostics.Process.Start("explorer.exe", BepInEx.Paths.ConfigPath)
- The whitelist section should have the same "Open Folder" button if it doesn't already.

**2. File naming — use the host's Among Us nickname**
- To get the local player's nickname: PlayerControl.LocalPlayer?.Data?.PlayerName
  If LocalPlayer is null (not yet in a game/lobby), fall back to "host".
- Sanitize the nickname for use in a filename: replace spaces and invalid filename
  characters with underscores.
- Export filenames:
  - Whitelist: whitelist_[nickname]_export.txt
  - Blacklist: blacklist_[nickname]_export.txt
  Where [nickname] is the actual Among Us nickname of the host at the time of export.
- Chat confirmation messages should include the actual filename so the host knows
  what was created.

**3. Import filename convention**
- Import button scans BepInEx.Paths.ConfigPath for the first file matching:
  - Whitelist import: whitelist_*_import.txt
  - Blacklist import: blacklist_*_import.txt
- Use Directory.GetFiles(BepInEx.Paths.ConfigPath, "whitelist_*_import.txt") and take
  the first result. Same for blacklist.
- If no matching file is found, send:
  "[HostGuard] No import file found. Export a list, rename '_export' to '_import',
  place it in the BepInEx config folder, then press Import."
- Everything else about the import logic (duplicate skipping, reporting counts) stays
  the same as the current implementation.

## Acceptance criteria
- [ ] "Export Blacklist" writes blacklist_[nickname]_export.txt, does NOT open Explorer.
- [ ] "Export Whitelist" writes whitelist_[nickname]_export.txt, does NOT open Explorer.
- [ ] "Open Folder" button opens the BepInEx config folder in Explorer.
- [ ] Import buttons scan for *_import.txt pattern, not a hardcoded filename.
- [ ] Nickname is taken from PlayerControl.LocalPlayer, falls back to "host" if null.
- [ ] Nickname is sanitized for use in filenames.
- [ ] Chat messages include the actual filename used.
- [ ] Build succeeds.

Commit the fix to feature/import-export-lists when done.

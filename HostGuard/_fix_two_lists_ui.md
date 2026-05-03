You are fixing missing UI rows in HostGuard at
C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard

Check out the branch feature/two-banned-word-lists.

## Problem
4 new text rows were added to UI/HostGuardUI.cs but are not visible in-game when the
host opens the HostGuard panel.

## What to do

1. Read UI/HostGuardUI.cs fully. Find:
   - The method that actually builds and renders the settings rows the host sees
     (look for where existing working rows like KickDefaultNames, BanForBannedWords
     etc. are created)
   - Exactly how a working text input row is created and registered so it appears in-game
   - Where the 4 new rows were added by the previous implementation

2. If the rows were added in the wrong method, or not following the exact same pattern
   as existing working rows, fix them to match exactly.

3. The 4 rows that should be visible:
   - BannedWords — text row, label "Exact Match List (comma-separated)" — ChatFilter section
   - BannedWordsContains — text row, label "Contains List (comma-separated)" — ChatFilter section
   - BannedNames (existing field, exact name in Config.cs) — text row,
     label "Exact Match Names (comma-separated)" — NameFilter section
   - BannedNamesContains — text row, label "Contains Match Names (comma-separated)" — NameFilter section

4. Do NOT add rows to HostGuardSettingsPanel.cs.

## Acceptance criteria
- [ ] All 4 rows visible in the in-game HostGuard panel.
- [ ] ChatFilter section shows both banned word list rows.
- [ ] NameFilter section shows both banned name list rows.
- [ ] Build succeeds.

Commit the fix to feature/two-banned-word-lists when done.

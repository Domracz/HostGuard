You are fixing the display of banned word and banned name lists in HostGuard at
C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard

Check out the branch feature/two-banned-word-lists.

## Problem
The 4 new list rows (Exact Match Names, Contains Match Names, Exact Match Words,
Contains Match Words) currently render as a single overflowing text row. The user
wants them displayed like the whitelist/blacklist: each entry on its own line with
an X button to remove it, plus a [Show/Hide] toggle to collapse the list.

## What to do

1. Read UI/HostGuardUI.cs fully. Find how the WHITELIST section renders its items
   (each entry on its own line with an X/remove button). Use that exact same pattern
   as the template for all 4 lists below.

2. For each of the 4 lists, implement this UI pattern:
   - A section label (e.g. "Exact Match Words")
   - A [Show] / [Hide] toggle button that collapses or expands the list
   - When expanded: each word/name on its own line with an [X] button that removes
     it from the ConfigEntry value and refreshes the display
   - When collapsed: show nothing (just the header + Show button)
   - A text input row + [Add] button at the bottom of the list to add new entries

3. The 4 lists:
   - ChatFilter: BannedWords (Exact Match Words), BannedWordsContains (Contains Words)
   - NameFilter: BannedNames/BadNameWords (Exact Match Names), BannedNamesContains (Contains Names)
   (Read Config.cs to confirm the exact field names)

4. Parsing: values are stored comma-separated in ConfigEntry<string>. When removing
   an item, rebuild the comma-separated string without that item and save to
   ConfigEntry.Value. When adding, append to the comma-separated string.

5. Do NOT use a plain text row for these lists. Do NOT touch HostGuardSettingsPanel.cs.

## Acceptance criteria
- [ ] All 4 lists render as expandable item lists, not single text rows.
- [ ] [Show]/[Hide] button collapses/expands each list.
- [ ] Each item has an [X] remove button that works.
- [ ] [Add] button with text input adds new entries.
- [ ] Changes persist immediately to ConfigEntry.Value.
- [ ] Build succeeds.

Commit the fix to feature/two-banned-word-lists when done.

You are fixing a display refresh bug in HostGuard at
C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard

Check out the branch feature/import-export-lists.

## Problem
The Whitelist and Blacklist sections in the panel show "(empty)" even after items
have been added (e.g. via import). The panel is built once and does not refresh
when the underlying lists change.

## What to do

1. Read UI/HostGuardUI.cs fully. Find how the Whitelist and Blacklist sections
   are built — specifically where the item rows are created.

2. Find what triggers a panel rebuild or refresh. Look for any existing refresh
   mechanism (e.g. a Rebuild() or Refresh() method, or the panel being rebuilt
   on open).

3. Make the Whitelist and Blacklist sections rebuild their item lists whenever:
   - The panel is opened
   - An item is added or removed
   - An import completes

4. If no refresh mechanism exists, implement one: clear and rebuild just the
   whitelist/blacklist item rows when needed, without rebuilding the entire panel.

## Acceptance criteria
- [ ] After importing a whitelist/blacklist, the items appear in the panel
      without needing to restart the game.
- [ ] After removing an item via the X button, it disappears immediately.
- [ ] Build succeeds.

Commit the fix to feature/import-export-lists when done.

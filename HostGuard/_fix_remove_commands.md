You are removing redundant chat commands from HostGuard at
C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard

Work on the main branch (git checkout main), then create a new branch:
git checkout -b feature/fix-remove-commands

## Problem
Several chat commands toggle features that are now fully controllable from the
in-game settings panel. They are redundant and clutter the command list.

## What to do

1. Read Patches/CommandPatch.cs fully.

2. Remove these commands entirely (both the handler logic and any help text):
   - !autostart / !as
   - !defaultnames / !dn
   - !badnames / !bn
   - !badchat / !bc
   - !contains / !cm
   - !botnames / !bot
   - !flood / !fp
   - !anticheat / !ac
   - !cosmetic / !cos

3. Keep all commands that do NOT have a panel equivalent, such as:
   - Whitelist add/remove by friend code or name
   - /kick, /ban (if present)
   - /promote, /demote, /cohosts (co-host system)
   - Any command that performs an action rather than toggling a setting

4. Update the help text / command list display (!help or !commands) to only show
   the remaining commands.

## Acceptance criteria
- [ ] All 9 listed commands are removed.
- [ ] Remaining commands still work.
- [ ] Help text no longer shows removed commands.
- [ ] Build succeeds.

Commit to feature/fix-remove-commands when done.

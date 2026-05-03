You are removing redundant commands from HostGuard at
C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard

Check out the branch feature/fix-remove-commands.

## Problem
The following commands still appear in !help AND still respond when typed (even if
they no longer change anything):
!autostart/!as, !defaultnames/!dn, !badnames/!bn, !badchat/!bc, !contains/!cm,
!botnames/!bot, !flood/!fp, !anticheat/!ac, !cosmetic/!cos

## What to do

1. Search the ENTIRE codebase (not just CommandPatch.cs) for every occurrence of
   these command strings: "autostart", "defaultnames", "badnames", "badchat",
   "contains", "botnames", "flood", "anticheat", "cosmetic" — in the context of
   command handling. Use grep to find all files.

2. For each file that contains these strings, read it fully and remove:
   - The command string comparison / if-else branch that matches the command
   - The response message that gets sent
   - Any remaining help text entries for these commands
   - Everything related to these commands end to end

3. After removing, verify nothing is left: grep again for each command string to
   confirm no handler or help text references remain.

4. Build the project: dotnet build
   Fix any errors before finishing.

## Acceptance criteria
- [ ] !help does not show any of the 9 removed commands.
- [ ] Typing any of the 9 commands in chat produces no response at all.
- [ ] All other commands still work normally.
- [ ] Build succeeds.

Commit the fix to feature/fix-remove-commands when done.

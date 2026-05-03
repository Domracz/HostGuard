You are fixing leftover help text in HostGuard at
C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard

Check out the branch feature/fix-remove-commands.

## Problem
The !help command still shows these removed commands in its output:
!autostart/!as, !defaultnames/!dn, !badnames/!bn, !badchat/!bc, !contains/!cm,
!botnames/!bot, !flood/!fp, !anticheat/!ac, !cosmetic/!cos

## What to do
1. Read Patches/CommandPatch.cs fully.
2. Find every place where help text is built or sent (look for SendLocalMessage calls
   that list commands, or any string containing these command names).
3. Remove all references to the 9 commands listed above from the help output.
4. Keep all other commands in the help text.

## Acceptance criteria
- [ ] !help no longer shows any of the 9 removed commands.
- [ ] All remaining commands still appear in !help.
- [ ] Build succeeds.

Commit the fix to feature/fix-remove-commands when done.

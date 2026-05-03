You are fixing missing UI rows in HostGuard at
C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard

Check out the branch feature/auto-start.

## Problem
3 new rows were added to UI/HostGuardUI.cs for the auto-start feature but are not
visible in the Lobby section of the in-game panel.

## What to do

1. Read UI/HostGuardUI.cs fully. Find:
   - The method that builds and renders the settings rows the host sees in-game
   - Where the existing LOBBY section is (it has Auto Return toggle and Return Delay
     stepper added by the auto-return feature)
   - Where the auto-start rows were added by the previous implementation

2. If the rows are in the wrong method or don't follow the exact pattern of the
   existing working Lobby rows, fix them.

3. The 3 rows that must appear in the Lobby section:
   - AutoStartIfClosing — toggle, label "Auto Start If Closing"
   - AutoStartThreshold — number stepper, label "Auto Start Threshold", min 5, max 120
   - MinPlayersToAutoStart — number stepper, label "Min Players to Auto Start", min 1, max 15

4. Do NOT add rows to HostGuardSettingsPanel.cs.

## Acceptance criteria
- [ ] All 3 rows visible in the Lobby section of the in-game panel.
- [ ] Existing Auto Return and Return Delay rows are unchanged.
- [ ] Build succeeds.

Commit the fix to feature/auto-start when done.

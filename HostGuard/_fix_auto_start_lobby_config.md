You are fixing missing UI rows in HostGuard at
C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard

Check out the branch feature/auto-start.

## Problem
The auto-start-if-lobby-closing feature has config entries in Config.cs
(AutoStartIfClosing, AutoStartThreshold, MinPlayersToAutoStart) but they are
not visible in the Lobby section of the in-game panel.

Note: there is an EXISTING auto-start feature (auto-start when all players join)
already in the panel under GENERAL. The new rows belong in the LOBBY section,
not GENERAL.

## What to do

1. Read UI/HostGuardUI.cs fully. Find:
   - The LOBBY section (it currently has Auto Return toggle and Return Delay stepper)
   - The exact method and pattern used to add rows to that section
   - Where the auto-start rows were added by the previous implementation

2. Add the 3 missing rows to the LOBBY section, after the existing Lobby rows,
   following the exact same pattern as Auto Return and Return Delay:
   - AutoStartIfClosing — toggle, label "Auto Start If Closing"
   - AutoStartThreshold — number stepper, label "Auto Start Threshold (s)", min 5, max 120
   - MinPlayersToAutoStart — number stepper, label "Min Players", min 1, max 15

3. Do NOT touch HostGuardSettingsPanel.cs.
4. Do NOT modify the existing GENERAL auto-start feature.

## Acceptance criteria
- [ ] 3 new rows visible in the LOBBY section of the in-game panel.
- [ ] Existing Auto Return and Return Delay rows are unchanged.
- [ ] Build succeeds.

Commit the fix to feature/auto-start when done.

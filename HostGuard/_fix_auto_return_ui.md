You are adding missing UI rows in HostGuard at
C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard

Check out the branch feature/auto-return.

## Problem
AutoReturnToLobby and AutoReturnDelay were added to Config.cs but are not visible
in the in-game settings panel.

## What to do
Read UI/HostGuardUI.cs and UI/HostGuardSettingsPanel.cs to understand which file
actually renders the settings the host sees in-game. Then add the missing rows:

- AutoReturnToLobby — toggle, label "Auto Return to Lobby", section "Lobby"
- AutoReturnDelay — text row, label "Return Delay (seconds)", section "Lobby",
  validate: reject non-numeric and negative values

Add a "Lobby" section header if one doesn't already exist in that file.
Follow the exact row-creation pattern already used in the file you're editing.

## Acceptance criteria
- [ ] "Auto Return to Lobby" toggle visible in the in-game panel.
- [ ] "Return Delay (seconds)" text row visible, validates input.
- [ ] Both under a "Lobby" section header.
- [ ] Build succeeds.

Commit the fix to feature/auto-return when done.

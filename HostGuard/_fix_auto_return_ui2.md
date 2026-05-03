You are adding missing UI rows in HostGuard at
C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard

Check out the branch feature/auto-return.

## Problem
AutoReturnToLobby and AutoReturnDelay are not visible in the in-game settings panel.
Previous attempts added rows to HostGuardSettingsPanel.cs — that file does NOT render
in-game. The file the host actually sees is UI/HostGuardUI.cs.

## What to do
1. Open UI/HostGuardUI.cs and find where other config rows are added (look for existing
   toggles and text rows for features like flood protection, name filter, etc.).
2. Add rows in that same location and style:
   - AutoReturnToLobby — toggle, label "Auto Return to Lobby"
   - AutoReturnDelay — text row, label "Return Delay (seconds)", validate: reject
     non-numeric and negative values
3. Add a "Lobby" section header above them if one doesn't exist yet.
4. Do NOT touch HostGuardSettingsPanel.cs.

## Acceptance criteria
- [ ] "Auto Return to Lobby" toggle visible when opening HostGuard panel in-game.
- [ ] "Return Delay (seconds)" text row visible in-game, validates input.
- [ ] Both under a "Lobby" section header.
- [ ] Build succeeds.

Commit the fix to feature/auto-return when done.

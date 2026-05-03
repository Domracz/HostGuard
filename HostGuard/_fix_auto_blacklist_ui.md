You are fixing missing UI rows in HostGuard at
C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard

Check out the branch feature/auto-blacklist.

## Problem
6 new AutoBlacklist toggle rows were added to UI/HostGuardUI.cs but are not visible
in-game when the host opens the HostGuard panel.

## What to do

1. Read UI/HostGuardUI.cs fully. Find:
   - The method that actually builds and renders the settings rows the host sees
     (look for where existing working toggles like KickDefaultNames, BanForBadName,
     BanKnownBots etc. are created)
   - Exactly how a working toggle row is created and registered so it appears in-game
   - Where the AutoBlacklist rows were added by the previous implementation

2. If the AutoBlacklist rows were added in the wrong method, or not following the exact
   same pattern as existing working rows, fix them to match exactly.

3. The 6 toggles and where they should appear:
   - AutoBlacklistBadName — under BanForBadName row (NameFilter section)
   - AutoBlacklistDefaultName — under BanForDefaultName/KickDefaultNames row (NameFilter section)
   - AutoBlacklistLowLevel — under BanForLowLevel row (MinLevel section)
   - AutoBlacklistBannedWord — under BanForBannedWords row (ChatFilter section)
   - AutoBlacklistKnownBot — under BanKnownBots row (BotProtection section)
   - AutoBlacklistInvalidRpc — under BanOnInvalidRpc row (AntiCheat section)

4. Do NOT add rows to HostGuardSettingsPanel.cs.

## Acceptance criteria
- [ ] All 6 AutoBlacklist toggles visible in the in-game HostGuard panel.
- [ ] Each toggle appears directly under its parent feature's row.
- [ ] Build succeeds.

Commit the fix to feature/auto-blacklist when done.

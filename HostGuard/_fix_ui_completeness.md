You are fixing the UI completeness feature in HostGuard at
C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard

Check out the branch feature/ui-completeness.

## Context
There are two UI files:
- UI/HostGuardSettingsPanel.cs — older/simpler panel
- UI/HostGuardUI.cs — the main scrollable in-game panel the host actually sees

The previous implementation found these fields in HostGuardSettingsPanel.cs and stopped.
But the host reports they are NOT visible in-game, meaning the panel the user actually
sees is likely HostGuardUI.cs, not HostGuardSettingsPanel.cs.

## First step — figure out which file is actually rendered in-game
Read both UI/HostGuardSettingsPanel.cs and UI/HostGuardUI.cs fully. Determine which one
is actually responsible for rendering the settings rows the host sees in-game. Look for
which file is instantiated or called when the settings tab is opened.

## Fields that the host confirmed are NOT visible in-game
- BanListUrl (string, section "BanList" / "General")
- KnownBotNames (string, section "BotProtection")
- KnownBotUrls (string, section "BotProtection")
- RapidLeaveThreshold (int, section "FloodProtection")

## What to do
Once you know which file is actually rendered:
- If the fields are missing from that file, add them there.
- If both files are rendered (different tabs/panels), add the fields to whichever one
  is missing them.
- String list fields (KnownBotNames, KnownBotUrls): label includes "(comma-separated)".
- Int field (RapidLeaveThreshold): positive-integer validation, reject non-numeric input.
- Follow the exact row-creation pattern already used in whichever file you're editing.

## Acceptance criteria
- [ ] All 4 fields are visible in the in-game settings panel.
- [ ] String list fields show "(comma-separated)" in their label.
- [ ] Int field validates input.
- [ ] Changes write to ConfigEntry.Value immediately.
- [ ] Build succeeds.

Commit the fix to feature/ui-completeness when done.

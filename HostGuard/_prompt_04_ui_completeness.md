You are implementing a feature for HostGuard, an Among Us BepInEx lobby moderator mod at
C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard
(BepInEx plugin, .NET 6, Reactor 2.5.0, plugin ID "com.rareshonour.hostguard").

## Goal
Every feature flag in Config.cs must be visible and editable in the in-game HostGuard
settings panel. Hosts should never need to manually edit config files.

## Codebase context
- Config.cs — all ConfigEntry<T> fields with section names. Read this file first.
- UI/HostGuardSettingsPanel.cs — creates rows via UIFactory.CreateToggle() and
  UIFactory.CreateTextRow(). Read this file fully to understand current coverage.
- UI/UIFactory.cs — the row-building primitives.
- UI/HostGuardUI.cs — the outer panel/tab orchestrator; HostGuardSettingsPanel renders
  inside it.
- No localization; strings hardcoded in C#.

## Known gap (verified by code analysis)
These Config.cs fields currently have NO row in HostGuardSettingsPanel.cs:

| Field                | Type   | Section         |
|----------------------|--------|-----------------|
| NameFilterEnabled    | bool   | NameFilter      |
| ChatFilterEnabled    | bool   | ChatFilter      |
| BotProtectionEnabled | bool   | BotProtection   |
| ContainsMode         | bool   | ChatFilter      |
| SuspiciousColorIds   | string | BotProtection   |
| RapidLeaveThreshold  | int    | FloodProtection |
| BanListUrl           | string | BanList         |
| MinLevelEnabled      | bool   | MinLevel        |
| MinLevel             | int    | MinLevel        |
| BanForLowLevel       | bool   | MinLevel        |
| KnownBotNames        | string | BotProtection   |
| KnownBotUrls         | string | BotProtection   |

Before coding: read Config.cs and HostGuardSettingsPanel.cs in full to confirm this list
is accurate and catch any additional gaps.

## Behaviour spec
- Add a toggle row for each missing bool field.
- Add a text input row for each missing string and int field, pre-filled with current value.
- String fields that hold comma-separated lists (KnownBotNames, KnownBotUrls,
  SuspiciousColorIds) include "(comma-separated)" in the label.
- Int fields (RapidLeaveThreshold, MinLevel) validate input: reject non-numeric and
  values <= 0.
- Each new row goes under the correct section header matching Config.cs section names.
- Changes take effect immediately by writing to ConfigEntry.Value (same as existing rows).
- No new config entries — only UI rows for existing fields.

## Implementation hints
- Follow the exact CreateToggle / CreateTextRow pattern already in HostGuardSettingsPanel.cs.
- Look at how MinLevel and BannedWords rows are handled in HostGuardUI.cs for reference
  (they may already exist there — do not duplicate).
- Only edit UI/HostGuardSettingsPanel.cs.

## Acceptance criteria
- [ ] All 12 fields listed above (and any additional gaps found) have corresponding rows.
- [ ] Section grouping matches Config.cs section names.
- [ ] Bool fields are toggles; string/int fields are text rows.
- [ ] Changes persist across sessions (writes to ConfigEntry.Value).
- [ ] Comma-separated fields show "(comma-separated)" in their label.
- [ ] Int fields reject non-numeric input without crashing.
- [ ] Build succeeds.

## Out of scope
Redesigning panel layout, adding validation beyond basic type checks, adding new config fields.

Before starting, create and switch to a new git branch named feature/ui-completeness.
Commit all changes to that branch when done.

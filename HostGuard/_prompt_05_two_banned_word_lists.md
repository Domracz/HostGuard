You are implementing a feature for HostGuard, an Among Us BepInEx lobby moderator mod at
C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard
(BepInEx plugin, .NET 6, Reactor 2.5.0, plugin ID "com.rareshonour.hostguard").

## Goal
Replace single-list + ContainsMode toggle systems with two independent lists (exact match
+ contains/substring match) for BOTH the chat banned words filter AND the name filter.
Both pairs of lists work simultaneously.

## Codebase context
- Config.cs — read this fully first to find:
  - BannedWords (ConfigEntry<string>, section "ChatFilter") — current single chat banned words list.
  - ContainsMode (ConfigEntry<bool>, section "ChatFilter") — global mode toggle to remove.
  - GetBannedWordsList() — parses BannedWords (comma-split, trim, lowercase).
  - The name filter banned names field (section "NameFilter") — find its exact name by reading Config.cs.
    This is a single list of banned names that needs the same two-list treatment.
- Patches/ChatPatch.cs — detection logic for banned words. Read fully before editing.
- The name filter patch (likely CreatePlayerPatch.cs or JoinPatch.cs) — find where banned
  names are checked. Read fully before editing.
- UI/HostGuardUI.cs — this is the file that renders in-game. Add all new rows here.
  Also read UI/HostGuardSettingsPanel.cs to find and remove stale references (ContainsMode),
  but do NOT add any new rows to HostGuardSettingsPanel.cs.
- No localization; strings hardcoded in C#.

## Part 1 — Chat banned words (two lists)
- Add a new ConfigEntry<string> BannedWordsContains (section "ChatFilter",
  key "BannedWordsContains", default "").
- BannedWords becomes the exact-match list. Rename its UI label to
  "Exact Match List (comma-separated)".
- BannedWordsContains is the contains list. UI label: "Contains List (comma-separated)".
- Detection in ChatPatch.cs: trigger if message equals any word in BannedWords
  OR message contains any word in BannedWordsContains. Both checks run simultaneously.
- Remove ContainsMode entirely from Config.cs, HostGuardSettingsPanel.cs, and HostGuardUI.cs.
- The existing BanForBannedWords toggle still applies to both lists.
- If both lists are empty, skip the check entirely.
- Add GetContainsBannedWords() helper in Config.cs matching the existing parsing pattern.

## Part 2 — Name filter banned names (two lists)
- Find the existing single banned names ConfigEntry in Config.cs (section "NameFilter").
  This becomes the exact-match list. Rename its UI label to
  "Exact Match Names (comma-separated)".
- Add a new ConfigEntry<string> BannedNamesContains (section "NameFilter",
  key "BannedNamesContains", default "").
  UI label: "Contains Match Names (comma-separated)".
- Detection in the name filter patch: trigger if player name equals any name in the
  exact list OR player name contains any name in the contains list.
  Both checks run simultaneously.
- The existing BanForBadName toggle still applies to both lists.
- If both lists are empty, skip the name check entirely.
- Add a GetContainsBannedNames() helper in Config.cs matching the existing parsing pattern.

## Files to edit
- Config.cs: add BannedWordsContains, BannedNamesContains, their parse helpers,
  remove ContainsMode.
- Patches/ChatPatch.cs: update to check both chat banned word lists.
- Name filter patch (whichever file it's in): update to check both name lists.
- UI/HostGuardSettingsPanel.cs: remove ContainsMode row and update labels only — do NOT add new rows here.
- UI/HostGuardUI.cs: remove ContainsMode reference, add all new rows here.

## Acceptance criteria
- [ ] Chat: words in BannedWords trigger on exact full-message match only.
- [ ] Chat: words in BannedWordsContains trigger if message contains the substring.
- [ ] Names: names in exact list trigger on exact player name match only.
- [ ] Names: names in contains list trigger if player name contains the substring.
- [ ] Both chat lists and both name lists work simultaneously.
- [ ] ContainsMode fully removed — no compile errors, no stale references.
- [ ] All 4 new/updated rows visible in the panel with correct labels.
- [ ] Empty lists are skipped without errors.
- [ ] Build succeeds.

## Out of scope
Per-word mode switching, case sensitivity options, regex support, separate kick/ban
settings per list.

Before starting, create and switch to a new git branch named feature/two-banned-word-lists.
Commit all changes to that branch when done.

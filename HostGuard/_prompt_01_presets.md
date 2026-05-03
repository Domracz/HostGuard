You are implementing features for HostGuard, an Among Us BepInEx lobby moderator mod at
C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard
(BepInEx plugin, .NET 6, Reactor 2.5.0, plugin ID "com.rareshonour.hostguard").

## Goal
Allow hosts to rename existing presets and import/export them via the filesystem,
so presets can be shared between machines or backed up.

## Codebase context
Two preset systems exist:
- Presets/PresetManager.cs — HostGuard config presets, KEY=VALUE .txt files in
  BepInEx/config/HostGuard_Presets/. SanitizeFileName() already exists for name validation.
- Presets/GamePresetManager.cs — Among Us game settings presets, same format in
  BepInEx/config/HostGuard_GamePresets/.
- UI/HostGuardUI.cs lines 543–593 — the Presets tab, shows Load/Delete buttons per preset.
- UI/UIFactory.cs — CreateToggle(), CreateTextRow() primitives for building rows.
- ChatHelper.SendLocalMessage(string) — send a host-visible chat message.
- HostGuardPlugin.Logger.LogInfo/LogWarning — logging.
- No localization system; all strings hardcoded in C#.

## Behaviour spec
**Rename:**
- Add a "Rename" button next to each preset entry (both config and game presets).
- Clicking opens a text input row inline. Pressing Enter/confirm renames the .txt file on disk
  using File.Move, sanitizing the new name via the existing SanitizeFileName().
- Refresh the preset list after rename.
- If the new name conflicts with an existing preset, send a chat error and do nothing.

**Export (open folder):**
- Add an "Open Folder" button (one per preset section, not per preset).
- Clicking calls System.Diagnostics.Process.Start("explorer.exe", folderPath) to open the
  preset directory in Windows Explorer.
- Also calls ChatHelper.SendLocalMessage("[HostGuard] Preset folder opened.").

**Import (refresh from folder):**
- Add a "Refresh" button that rescans the preset folder and rebuilds the list in the UI.
- Add a chat tip button or label: "[HostGuard] Drop .txt preset files into the opened folder,
  then press Refresh."
- Both sections (config presets, game presets) get their own Refresh button.

## Implementation hints
- Modify Presets/PresetManager.cs, Presets/GamePresetManager.cs, UI/HostGuardUI.cs only.
- No new config entries needed — this is purely UI + file I/O.
- Button creation follows the existing UIFactory pattern in HostGuardUI.cs.
- Log all file operations via HostGuardPlugin.Logger.LogInfo.

## Acceptance criteria
- [ ] Rename button appears per preset; renames file; refreshes list.
- [ ] Conflicting names show a chat error and abort.
- [ ] "Open Folder" button opens the correct directory in Explorer.
- [ ] "Refresh" button reloads the preset list from disk.
- [ ] Both config and game preset sections have all three controls.
- [ ] SanitizeFileName() is reused — do not duplicate it.
- [ ] Build succeeds with no warnings.

## Out of scope
Native file picker dialogs, encrypting exports, merging presets.

Before starting, create and switch to a new git branch named feature/presets.
Commit all changes to that branch when done.

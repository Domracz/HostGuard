You are making a small QoL improvement to HostGuard, an Among Us BepInEx lobby moderator
mod at C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard
(BepInEx plugin, .NET 6, Reactor 2.5.0, plugin ID "com.rareshonour.hostguard").

## Goal
When a preset is loaded, the host gets no feedback — config values silently update.
Add a chat notification confirming which preset was loaded.

## Codebase context
- Presets/PresetManager.cs — handles HostGuard config preset loading. Find the Load
  method that applies a preset's values.
- Presets/GamePresetManager.cs — same for game settings presets.
- ChatHelper.SendLocalMessage(string) — host-visible chat message.

## Behaviour spec
- In PresetManager.cs, at the end of the preset load method, call:
  ChatHelper.SendLocalMessage($"[HostGuard] Config preset '{presetName}' loaded.");
- In GamePresetManager.cs, same:
  ChatHelper.SendLocalMessage($"[HostGuard] Game preset '{presetName}' loaded.");
- No new config entries, no UI changes — this is a 2-line fix per file.

## Acceptance criteria
- [ ] Loading a config preset sends a chat message with its name.
- [ ] Loading a game preset sends a chat message with its name.
- [ ] Build succeeds.

## Out of scope
Anything beyond adding the two SendLocalMessage calls.

Before starting, create and switch to a new git branch named feature/fix-preset-load-feedback.
Commit all changes to that branch when done.

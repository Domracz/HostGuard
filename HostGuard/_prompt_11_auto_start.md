You are implementing a feature for HostGuard, an Among Us BepInEx lobby moderator mod at
C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard
(BepInEx plugin, .NET 6, Reactor 2.5.0, plugin ID "com.rareshonour.hostguard").

## Goal
When the Among Us lobby countdown timer runs low (lobby is about to close/expire),
automatically start the game so players are not kicked to the main menu.

## Codebase context
- No lobby-timer patch currently exists. You must create one.
- Patches/ folder: follow the *Patch.cs naming convention.
- Among Us interop at:
  D:\SteamLibrary\steamapps\common\Among Us\BepInEx\interop\Assembly-CSharp.dll
  READ GameStartManager in the interop before writing code to confirm:
  - The exact name of the countdown timer field (likely countDownTimer or similar).
  - The method to programmatically start the game (likely ReallyBegin() or similar).
  - Whether a minimum-player check already exists on that method.
- AmongUsClient.Instance.AmHost — host check.
- AmongUsClient.Instance.AllPlayers.Count — current player count.
- ChatHelper.SendLocalMessage(string) — host-visible messages.
- Config.cs — ConfigEntry<T> bound in Initialize().
- UI/HostGuardUI.cs — add rows to the "Lobby" section here (create the header if absent).
  IMPORTANT: Do NOT add rows to HostGuardSettingsPanel.cs — that file does not render in-game.
  UI/HostGuardUI.cs is the file the host actually sees.

## Behaviour spec
- Patch GameStartManager.Update() (Postfix) to monitor the countdown timer field.
- When the timer drops to <= AutoStartThreshold AND AutoStartIfClosing.Value == true
  AND AmHost == true AND AllPlayers.Count >= MinPlayersToAutoStart:
  - Set a _triggered flag to true (prevents firing every frame).
  - Invoke the start-game method on GameStartManager.
  - ChatHelper.SendLocalMessage("[HostGuard] Lobby closing — auto-starting game!");
  - Log: HostGuardPlugin.Logger.LogInfo("[AutoStart] Lobby closing, auto-starting.");
- Reset _triggered = false in a Postfix on GameStartManager.Start() (new lobby).
- Do nothing if AmHost == false, feature disabled, or already triggered.

## Config & UI surface
- AutoStartIfClosing — bool, default false, section "Lobby".
- AutoStartThreshold — int, default 30 (seconds), section "Lobby".
- MinPlayersToAutoStart — int, default 1, section "Lobby".
- New "Lobby" section header (if absent) + 3 rows in UI/HostGuardUI.cs.
  Int fields: text rows with positive-integer validation.

## Acceptance criteria
- [ ] New patch file Patches/LobbyTimerPatch.cs created.
- [ ] When enabled and timer <= threshold, game starts automatically.
- [ ] Does not fire if player count < MinPlayersToAutoStart.
- [ ] Does not fire if AmHost == false or feature disabled.
- [ ] Only triggers once per countdown (re-arms on new lobby).
- [ ] Chat message fires on trigger.
- [ ] All 3 config rows visible in settings panel.
- [ ] Build succeeds.

## Out of scope
Forcing a start with zero players, kicking AFK players before start, overriding Among Us's
own minimum player count enforcement.

Before starting, create and switch to a new git branch named feature/auto-start.
Commit all changes to that branch when done.

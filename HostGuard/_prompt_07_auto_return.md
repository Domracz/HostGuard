You are implementing a feature for HostGuard, an Among Us BepInEx lobby moderator mod at
C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard
(BepInEx plugin, .NET 6, Reactor 2.5.0, plugin ID "com.rareshonour.hostguard").

## Goal
After a game ends, automatically click through the post-game screens to return to the lobby
without host input. Non-host players cannot rejoin until the host is back in the lobby, so
reducing the time the host spends on the end screen benefits all players.

## Codebase context
- There is currently NO patch for the game-end state. You must create one.
- The Patches/ folder has 9 existing patches; follow the naming convention *Patch.cs.
- The Among Us Assembly-CSharp interop is at:
  D:\SteamLibrary\steamapps\common\Among Us\BepInEx\interop\Assembly-CSharp.dll
  Read EndGameManager using dnSpy or by grepping the interop to find the correct method
  and button field names before writing any code.
- AmongUsClient.Instance.AmHost — bool, true only for the lobby host.
- HostGuardPlugin.Instance.StartCoroutine(IEnumerator) — for delays.
- ChatHelper.SendLocalMessage(string) — host-visible chat messages.
- Config.cs — ConfigEntry<T> bound in Initialize().
- UI/HostGuardSettingsPanel.cs — add rows to a "Lobby" section (create the header if absent).

## Behaviour spec
- Patch EndGameManager (inspect to find the right method — likely ShowRole() or similar).
- Only fire if AutoReturnToLobby.Value == true AND AmongUsClient.Instance.AmHost == true.
- After the patch fires: wait AutoReturnDelay seconds (coroutine), then invoke the first
  post-game button (advances past role screen), wait a short fixed delay (1 second), then
  invoke the second button (returns to lobby).
- Determine exact button names/fields by reading EndGameManager in the interop first.
  Do not assume button names.
- Send "[HostGuard] Returning to lobby..." via ChatHelper.SendLocalMessage after clicking.
- Log: HostGuardPlugin.Logger.LogInfo("[AutoReturn] Clicking to return to lobby");

## Config & UI surface
- AutoReturnToLobby — bool, default false, section "Lobby".
- AutoReturnDelay — float, default 3.0, section "Lobby",
  description "Seconds to wait after game ends before auto-clicking to return to lobby".
- New "Lobby" section header + 2 rows in HostGuardSettingsPanel.cs.
  Float field: text row with float parsing, reject non-numeric and negative values.

## Acceptance criteria
- [ ] New patch file Patches/GameEndPatch.cs created following existing conventions.
- [ ] When enabled, host auto-clicks through post-game screens after the delay.
- [ ] Nothing happens if AmHost == false.
- [ ] Nothing happens if AutoReturnToLobby == false.
- [ ] Chat message "[HostGuard] Returning to lobby..." fires after click sequence.
- [ ] Toggle and delay rows visible in settings panel under "Lobby" section.
- [ ] Build succeeds.

## Out of scope
Kicking players who don't return, tracking who left during the game, non-host return behavior.

Before starting, create and switch to a new git branch named feature/auto-return.
Commit all changes to that branch when done.

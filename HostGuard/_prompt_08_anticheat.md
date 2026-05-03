You are improving the anti-cheat system in HostGuard, an Among Us BepInEx lobby moderator
mod at C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard
(BepInEx plugin, .NET 6, Reactor 2.5.0, plugin ID "com.rareshonour.hostguard").

## Goal
Make the anticheat significantly more robust without creating false positives that affect
legitimate crewmates. This session is intentionally open-ended — begin with research and
discussion before writing any code.

## MANDATORY first steps (do these before writing any code)

1. Read these files in full:
   - Patches/RpcValidationPatch.cs
   - Patches/CosmeticValidationPatch.cs
   - Config.cs (especially the AntiCheat section and GetSuspiciousColorIds())

2. Inspect the Among Us interop to understand available hooks:
   D:\SteamLibrary\steamapps\common\Among Us\BepInEx\interop\Assembly-CSharp.dll
   Focus on: PlayerControl, CustomNetworkTransform, RpcCalls enum, EndGameManager,
   InnerNetClient. Note what state the host can observe from these.

3. Ask the user the following questions and WAIT for answers before coding:
   a. "What specific cheats are you currently seeing in your lobbies that the anticheat
      is missing? (e.g. speed hacks, kill through walls, task skip, cooldown abuse,
      role switching, anything else?)"
   b. "Have any anticheat checks ever triggered false positives on legitimate players?
      If so, which ones?"
   c. "SuspiciousColorIds is defined in Config.cs and parsed but never wired to a patch.
      Should I connect it to CosmeticValidationPatch to kick players with suspicious
      color IDs? What color IDs should be in that list by default?"

4. After receiving answers: design each new check carefully. For every proposed check,
   confirm with the user that it won't affect legitimate crewmates before implementing.

## Known gap to fix regardless of discussion
SuspiciousColorIds is parsed by Config.cs:GetSuspiciousColorIds() but not used anywhere.
Wire it to CosmeticValidationPatch.cs: if a player's colorId is in the suspicious list,
kick them (configurable ban via existing BanOnInvalidRpc or a new dedicated flag).

## Implementation constraints (apply to all new checks)
- All new checks must be gated by AntiCheatEnabled.Value.
- Every new check must have its own bool ConfigEntry (section "AntiCheat", default false)
  so the host can enable them individually.
- Whitelist bypass: check HostGuardConfig.GetWhitelistedCodes().Contains(data.FriendCode)
  before any new check. Skip whitelisted players.
- All kicks go through KickQueue.Enqueue(clientId, BanOnInvalidRpc.Value, name).
- Log each detection: HostGuardPlugin.Logger.LogInfo and KickLog.Log.
- New toggles go in UI/HostGuardUI.cs under the AntiCheat section.
  IMPORTANT: Do NOT add rows to HostGuardSettingsPanel.cs — that file does not render in-game.
  UI/HostGuardUI.cs is the file the host actually sees.

## Acceptance criteria (finalize after discussion)
- [ ] SuspiciousColorIds wired and functional in CosmeticValidationPatch.
- [ ] Each newly agreed-upon check is implemented with its own config toggle.
- [ ] All new checks default to false (opt-in).
- [ ] Whitelisted players bypass all new checks.
- [ ] No false positives on normal gameplay (discuss safeguards per check with user).
- [ ] Build succeeds.

## Out of scope
Client-side visual hacks the host cannot observe, ML-based classification, checks that
require modifying the AU server.

Before starting, create and switch to a new git branch named feature/anticheat.
Commit all changes to that branch when done.

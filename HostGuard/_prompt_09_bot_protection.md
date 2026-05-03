You are improving bot detection in HostGuard, an Among Us BepInEx lobby moderator mod at
C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard
(BepInEx plugin, .NET 6, Reactor 2.5.0, plugin ID "com.rareshonour.hostguard").

## Goal
Make bot detection more effective without flagging legitimate players. Begin with research
and discussion before writing code.

## MANDATORY first steps (do these before writing any code)

1. Read these files in full:
   - Patches/CreatePlayerPatch.cs
   - Patches/JoinPatch.cs
   - Patches/ChatPatch.cs
   - Config.cs (BotProtection section, KnownBotNames, KnownBotUrls, SuspiciousColorIds,
     BanKnownBots, CosmeticDetectionEnabled)

2. Ask the user these questions and WAIT for answers before coding:
   a. "What bot patterns are slipping past the current detection? Are you seeing bots
      with random/changing names, names not in the KnownBotNames list, specific cosmetics,
      unusual join behaviors, or something else?"
   b. "The cosmetic detection (SuspiciousColorIds, CosmeticDetectionEnabled) is currently
      disabled by default. Should it be enabled by default, or stay opt-in? Are there
      specific color IDs that are exclusively used by bots?"
   c. "Are you open to a heuristic scoring approach — accumulate signals before kicking
      (e.g. suspicious name + no friend code + rapid join = kick) — or do you prefer
      simple per-signal rules?"

3. After answers: design improvements matching the actual bot patterns the user describes.

## Current bot protection summary (read code to confirm)
- Known bot name matching: exact, underscore-normalized, substring — in CreatePlayerPatch.cs
  and JoinPatch.cs, gated by BotProtectionEnabled.
- Bot URL detection in chat — in ChatPatch.cs, hardcoded ban.
- Cosmetic detection infrastructure (SuspiciousColorIds) exists but disabled by default.
- No join-timing or behavior-based heuristics.

## Implementation constraints (apply to all new checks)
- All new checks gated by BotProtectionEnabled.Value.
- Whitelist bypass: HostGuardConfig.GetWhitelistedCodes().Contains(data.FriendCode).
- New checks belong in CreatePlayerPatch.cs (primary) with fallback in JoinPatch.cs.
- Any new thresholds or config values: section "BotProtection", conservative defaults.
- New toggles/rows in UI/HostGuardUI.cs under the BotProtection section.
  IMPORTANT: Do NOT add rows to HostGuardSettingsPanel.cs — that file does not render in-game.
  UI/HostGuardUI.cs is the file the host actually sees.
- Log all detections via HostGuardPlugin.Logger and KickLog.Log.

## Acceptance criteria (finalize after discussion)
- [ ] Agreed bot patterns are detected and acted upon.
- [ ] Legitimate players are not affected.
- [ ] Whitelisted players bypass all new checks.
- [ ] All new options visible in the settings panel.
- [ ] All new options default to conservative (opt-in) values.
- [ ] Build succeeds.

## Out of scope
Server-side account verification API calls, ML classification, browser fingerprinting.

Before starting, create and switch to a new git branch named feature/bot-protection.
Commit all changes to that branch when done.

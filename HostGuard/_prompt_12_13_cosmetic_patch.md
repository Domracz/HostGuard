You are fixing two issues in HostGuard, an Among Us BepInEx lobby moderator mod at
C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard
(BepInEx plugin, .NET 6, Reactor 2.5.0, plugin ID "com.rareshonour.hostguard").

## Goal
Fix two problems in CosmeticValidationPatch.cs:
1. SuspiciousColorIds is parsed in Config.cs but never used anywhere — wire it up.
2. All cosmetic violations hardcode ban=true with no config option — make it configurable.

## Codebase context
- Patches/CosmeticValidationPatch.cs — read this fully first. Every KickQueue.Enqueue
  call here currently passes ban=true hardcoded.
- Config.cs — SuspiciousColorIds (ConfigEntry<string>, section "BotProtection"),
  GetSuspiciousColorIds() returns a parsed HashSet<int>. Read this method.
- HostGuardConfig.GetWhitelistedCodes() — whitelist bypass.
- Config.cs — ConfigEntry<T> bound in Initialize(). New entries go here.
- UI/HostGuardSettingsPanel.cs — add new toggle rows.

## Fix 1 — Wire SuspiciousColorIds
- Add a new bool ConfigEntry: SuspiciousColorDetectionEnabled
  (section "BotProtection", default false).
- In CosmeticValidationPatch.cs, after the existing color ID range check, add:
  if SuspiciousColorDetectionEnabled.Value is true AND the player's colorId is in
  GetSuspiciousColorIds(), call KickQueue.Enqueue(clientId, BanForBadCosmetic.Value, name).
  (Uses the new BanForBadCosmetic toggle from Fix 2 — implement Fix 2 first.)
- Skip whitelisted players before the check.
- Log: HostGuardPlugin.Logger.LogInfo($"[AntiCheat] Suspicious color {colorId}: {name}");
- Add a toggle row in UI/HostGuardUI.cs under the BotProtection section.

## Fix 2 — Configurable ban for bad cosmetics
- Add a new bool ConfigEntry: BanForBadCosmetic (section "AntiCheat", default false).
  Default false = kick only, not ban (conservative — cosmetic bugs can affect legit players).
- Replace every hardcoded ban=true in CosmeticValidationPatch.cs with
  HostGuardConfig.BanForBadCosmetic.Value.
- Add a toggle row in UI/HostGuardUI.cs under the AntiCheat section.
  IMPORTANT: Do NOT add rows to HostGuardSettingsPanel.cs — that file does not render in-game.
  UI/HostGuardUI.cs is the file the host actually sees.

## Acceptance criteria
- [ ] BanForBadCosmetic toggle visible in panel under AntiCheat.
- [ ] When false (default): bad cosmetic -> kick only.
- [ ] When true: bad cosmetic -> ban.
- [ ] No hardcoded ban=true remaining in CosmeticValidationPatch.cs.
- [ ] SuspiciousColorDetectionEnabled toggle visible in panel under BotProtection.
- [ ] Player with suspicious color ID gets kicked/banned when enabled.
- [ ] Whitelisted players are not affected by suspicious color check.
- [ ] Both new toggles default to false.
- [ ] Build succeeds.

## Out of scope
Changing what cosmetics are considered invalid, per-violation-type ban settings.

Before starting, create and switch to a new git branch named feature/fix-cosmetic-patch.
Commit all changes to that branch when done.

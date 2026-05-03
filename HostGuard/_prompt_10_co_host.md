You are implementing a feature for HostGuard, an Among Us BepInEx lobby moderator mod at
C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard
(BepInEx plugin, .NET 6, Reactor 2.5.0, plugin ID "com.rareshonour.hostguard").

## Goal
Allow the host to promote players to "co-host" status. Co-hosts can type /kick and /ban
in game chat; HostGuard (running only on the host's machine) intercepts these and executes
them on the co-host's behalf.

## Codebase context
- HostGuard runs ONLY on the host's machine. Co-hosts cannot run HostGuard.
  All co-host commands are intercepted on the host's side via chat patches.
- Patches/CommandPatch.cs — handles existing slash commands from the host. Read this fully
  to understand the command pattern before adding new commands.
- Patches/ChatPatch.cs — intercepts all chat messages. Read this to understand how to
  intercept messages from non-host players.
- KickQueue.Enqueue(clientId, ban, name) — kick/ban API.
- Blacklist.Add(string friendCode) — persistent blacklist.
- ChatHelper.SendLocalMessage(string) — host-visible messages only.
- AmongUsClient.Instance.AllPlayers — iterate to find players by name.
- AmongUsClient.Instance.AmHost — host check.
- HostGuardConfig.GetWhitelistedCodes() — whitelist bypass list.
- Config.cs — ConfigEntry<T> bound in Initialize().
- UI/HostGuardSettingsPanel.cs — add toggle to new "Co-Host" section.
- KickLog.Log(name, code, banned, reason) — audit log.

## Behaviour spec

**Co-host list (in-memory, resets on lobby reset):**
- Create CoHosts/CoHostManager.cs — static class with:
  Promote(string friendCode, string displayName),
  Demote(string friendCode),
  IsCoHost(string friendCode): bool,
  GetAll(): IReadOnlyList<(string FriendCode, string Name)>,
  Clear() (called on lobby reset).
- Store by friend code (not name) to survive in-game name changes.

**Host commands (intercepted in CommandPatch.cs):**
- /promote [name] — finds player by name in AllPlayers, calls CoHostManager.Promote,
  sends "[HostGuard] [Name] is now a co-host."
- /demote [name] — calls CoHostManager.Demote,
  sends "[HostGuard] [Name] is no longer a co-host."
- /cohosts — lists current co-hosts:
  "[HostGuard] Co-hosts: Name1, Name2" or "[HostGuard] No co-hosts set."

**Co-host commands (intercepted in ChatPatch.cs, from non-host players):**
- When a player's message starts with /kick or /ban:
  1. Check CoHostEnabled.Value — if false, ignore.
  2. Check CoHostManager.IsCoHost(sender.FriendCode) — if not a co-host, ignore.
  3. Find target player by name in AllPlayers.
  4. Safety checks: target is not the host, not another co-host, not whitelisted.
     If any check fails: ChatHelper.SendLocalMessage("[HostGuard] Cannot target that player.")
  5. /kick -> KickQueue.Enqueue(targetId, false, targetName).
  6. /ban -> KickQueue.Enqueue(targetId, true, targetName) +
     optionally Blacklist.Add(targetFriendCode) if AutoBlacklistCoHostBan.Value == true.
  7. ChatHelper.SendLocalMessage("[HostGuard] [CoHostName] kicked/banned [TargetName].")
  8. KickLog.Log(targetName, targetFriendCode, isBan, $"co-host:{coHostName}").

**Lobby reset:**
- Call CoHostManager.Clear() when a new lobby begins. Patch the appropriate reset point
  (check HudStartPatch or equivalent already in the codebase).

## Config & UI surface
- CoHostEnabled — bool, default false, section "CoHost".
- AutoBlacklistCoHostBan — bool, default false, section "CoHost".
- New "Co-Host" section header + 2 toggle rows in UI/HostGuardUI.cs.
  IMPORTANT: Do NOT add rows to HostGuardSettingsPanel.cs — that file does not render in-game.
  UI/HostGuardUI.cs is the file the host actually sees.

## Acceptance criteria
- [ ] /promote, /demote, /cohosts work from host chat.
- [ ] Co-host /kick and /ban are executed by HostGuard on the host machine.
- [ ] Co-hosts cannot target the host, other co-hosts, or whitelisted players.
- [ ] Co-host list resets on lobby reset.
- [ ] CoHostEnabled = false silently ignores all co-host commands.
- [ ] All co-host actions are in KickLog.
- [ ] Build succeeds.

## Out of scope
Persistent co-host list across sessions, co-hosts promoting others, co-hosts accessing
the HostGuard UI panel.

Before starting, create and switch to a new git branch named feature/co-host.
Commit all changes to that branch when done.

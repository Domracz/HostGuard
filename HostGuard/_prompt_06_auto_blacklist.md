You are implementing a feature for HostGuard, an Among Us BepInEx lobby moderator mod at
C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard
(BepInEx plugin, .NET 6, Reactor 2.5.0, plugin ID "com.rareshonour.hostguard").

## Goal
For each configurable kick/ban trigger, add an optional "auto-add to persistent blacklist"
toggle. When enabled, the player's friend code is written to the blacklist file immediately
when they are kicked or banned by that feature — ensuring they can't rejoin even after a
lobby reset.

## Codebase context
- Blacklist.cs — Blacklist.Add(string friendCode): adds to persistent file-backed blacklist.
  Returns false if duplicate (silent). Blacklist.Contains(), GetAll(), Remove() also exist.
- Patches/CreatePlayerPatch.cs — primary enforcement point. Most kick triggers live here.
  Player friend code: playerData.FriendCode (NetworkedPlayerInfo field).
- Patches/JoinPatch.cs — fallback enforcement. Same friend code access pattern.
- Patches/ChatPatch.cs — banned word trigger (chat-time, not join-time).
- KickQueue.Enqueue(clientId, ban, name) does NOT add to Blacklist automatically.
- Config.cs — ConfigEntry<T> bound in Initialize(). New toggles go here.
- UI/HostGuardUI.cs — add new toggle rows under the correct parent section here.
  IMPORTANT: Do NOT add rows to HostGuardSettingsPanel.cs — that file does not render in-game.
  UI/HostGuardUI.cs is the file the host actually sees.

## Behaviour spec
Add these new bool ConfigEntry fields (all default false — opt-in):

| Config key               | Section         | Trigger it pairs with         |
|--------------------------|-----------------|-------------------------------|
| AutoBlacklistBadName     | NameFilter      | bad name (contains filter)    |
| AutoBlacklistDefaultName | NameFilter      | default/unchanged name        |
| AutoBlacklistLowLevel    | MinLevel        | low level kick                |
| AutoBlacklistBannedWord  | ChatFilter      | banned word in chat           |
| AutoBlacklistKnownBot    | BotProtection   | known bot name match          |
| AutoBlacklistInvalidRpc  | AntiCheat       | invalid RPC detected          |

When the feature fires AND its AutoBlacklist* toggle is true:
- If playerData.FriendCode is null or empty, skip and log a warning — do not crash.
- Call Blacklist.Add(playerData.FriendCode).
- Log: HostGuardPlugin.Logger.LogInfo(
    $"[Blacklist] Auto-added {friendCode} ({name}) — reason: {reason}");

Hardcoded-ban triggers (flood, cosmetic, bot URL) do NOT get new toggles — they always
ban via the AU system already. Skip them.

## Implementation hints
- At each call site in CreatePlayerPatch.cs and JoinPatch.cs, the friend code is available
  on the NetworkedPlayerInfo object at that point — read each method to confirm the variable
  name.
- For the ChatPatch.cs banned-word site (runtime, not join), the friend code must be looked
  up from the PlayerControl sending the message.
- Each new AutoBlacklist* toggle row in UI/HostGuardUI.cs goes directly below its
  parent feature's existing row (e.g., AutoBlacklistBadName under the BanForBadName row).

## Acceptance criteria
- [ ] 6 new toggles appear in the settings panel, each under their parent section.
- [ ] Enabling a toggle causes the matching trigger to call Blacklist.Add(friendCode).
- [ ] Addition is logged with reason.
- [ ] Null/empty friend codes are skipped without crashing.
- [ ] Duplicate entries silently ignored (Blacklist.Add handles this).
- [ ] All 6 default to false.
- [ ] Build succeeds.

## Out of scope
A global "auto-blacklist everything" mega-toggle, auto-blacklisting hardcoded bans,
retroactive blacklisting of previously kicked players.

Before starting, create and switch to a new git branch named feature/auto-blacklist.
Commit all changes to that branch when done.

You are fixing an inconsistency in HostGuard, an Among Us BepInEx lobby moderator mod at
C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard
(BepInEx plugin, .NET 6, Reactor 2.5.0, plugin ID "com.rareshonour.hostguard").

## Goal
The blacklist stores one friend code per line in a standalone file. The whitelist stores
all codes as a comma-separated string inside the BepInEx config file. This inconsistency
makes the whitelist harder to edit externally and limits its scalability. Migrate the
whitelist to its own standalone file using the same one-per-line format as the blacklist.

## Codebase context
- Blacklist.cs — model to follow exactly. Uses a HashSet<string>, loads from
  <BepInEx Config>/hostguard_blacklist.txt, one code per line. API: Load(), Save(),
  Add(), Remove(), Contains(), GetAll(). Read this file fully first.
- Config.cs — WhitelistedCodes is currently a ConfigEntry<string> (comma-separated).
  GetWhitelistedCodes() parses it. Read Config.cs fully to find all usages.
- UI/HostGuardUI.cs — currently reads WhitelistedCodes for display and Remove buttons.
  Will need updating to use the new Whitelist class.
- PresetManager.cs — explicitly skips WhitelistedCodes on preset load (intentional,
  must be preserved — the whitelist should never be overwritten by a preset).

## Behaviour spec
- Create Whitelist.cs modelled exactly on Blacklist.cs, storing codes in
  <BepInEx Config>/hostguard_whitelist.txt (one code per line).
- API: Load(), Save(), Add(string), Remove(string), Contains(string), GetAll().
- In Plugin.cs (or wherever Blacklist.Load() is called on startup), also call Whitelist.Load().
- Migration: on first load, if hostguard_whitelist.txt does not exist but
  WhitelistedCodes config entry is non-empty, migrate the existing comma-separated codes
  into the new file automatically, then clear WhitelistedCodes config entry.
- Replace all calls to HostGuardConfig.GetWhitelistedCodes() with Whitelist.Contains()
  or Whitelist.GetAll() as appropriate. Audit every usage site.
- Update UI/HostGuardUI.cs whitelist section to use Whitelist.GetAll() and Whitelist.Remove().
- Keep PresetManager.cs's skip of WhitelistedCodes — it no longer matters since it's not
  in config anymore, but don't break that logic.
- Remove WhitelistedCodes ConfigEntry from Config.cs and GetWhitelistedCodes() helper
  after migration is complete.

## Acceptance criteria
- [ ] Whitelist.cs created, matching Blacklist.cs structure and API.
- [ ] hostguard_whitelist.txt created on first run.
- [ ] Existing codes from WhitelistedCodes config entry are auto-migrated on first run.
- [ ] All GetWhitelistedCodes() call sites replaced with Whitelist.Contains()/GetAll().
- [ ] Whitelist UI (display + remove buttons) works with new class.
- [ ] No remaining references to the old WhitelistedCodes config entry.
- [ ] Build succeeds.

## Out of scope
Changing the blacklist format, adding new whitelist UI features beyond what already exists.

Before starting, create and switch to a new git branch named feature/fix-whitelist-format.
Commit all changes to that branch when done.

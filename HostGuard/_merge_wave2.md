You are working on the HostGuard mod at C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard

Merge these completed feature branches into main, one at a time:
1. feature/toast-notifications
2. feature/two-banned-word-lists
3. feature/auto-blacklist
4. feature/auto-start
5. feature/import-export-lists (re-merge — panel refresh fix was added after Wave 1)
6. feature/fix-remove-commands

Steps:
- Make sure you are on the main branch first (git checkout main)
- Merge each branch with: git merge feature/<name>
- If there are merge conflicts, they will almost certainly be in Config.cs or
  HostGuardSettingsPanel.cs — both files just have new lines added in different
  sections, so accept all changes from both sides (keep everything).
- After all merges succeed, verify the project builds: dotnet build
- Fix any build errors before finishing.
- In Plugin.cs, update the version in the [BepInPlugin] attribute from its current value to "3.2.0"
- Push main to GitHub: git push origin main
- Build the project: dotnet build
- Create a GitHub release tagged "hg v3.2.0" and attach the DLL: with this description:

  ## What's new in v3.2.0
  - Toast notifications: top-left HUD alerts whenever a player is kicked, banned, or whitelisted
  - Banned words: two separate lists — exact match and contains match — both active simultaneously
  - Bad names: same two-list system for the name filter
  - Auto-blacklist: optional per-feature toggle to persist kicked players to the blacklist file
  - Auto-start: automatically starts the game when the lobby countdown timer runs low

  Use: gh release create "hg v3.2.0" "D:\SteamLibrary\steamapps\common\Among Us\BepInEx\plugins\HostGuard.dll" --title "hg v3.2.0" --notes "..."
  (fill in the notes with the description above)
- Report what was merged, whether the build succeeded, and the release URL.

You are working on the HostGuard mod at C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard

Merge this completed feature branch into main:
1. feature/fix-whitelist-format

Steps:
- Make sure you are on the main branch first (git checkout main)
- Merge with: git merge feature/fix-whitelist-format
- If there are merge conflicts, they will almost certainly be in Config.cs or
  HostGuardSettingsPanel.cs — accept all changes from both sides (keep everything).
- After the merge, verify the project builds: dotnet build
- Fix any build errors before finishing.
- In Plugin.cs, update the version in the [BepInPlugin] attribute from its current value to "3.4.0"
- Push main to GitHub: git push origin main
- Build the project: dotnet build
- Create a GitHub release tagged "hg v3.4.0" and attach the DLL: with this description:

  ## What's new in v3.4.0
  - Whitelist storage migrated to a standalone file (hostguard_whitelist.txt), matching
    the blacklist format. Existing whitelist entries are auto-migrated on first run.

  Use: gh release create "hg v3.4.0" "D:\SteamLibrary\steamapps\common\Among Us\BepInEx\plugins\HostGuard.dll" --title "hg v3.4.0" --notes "..."
  (fill in the notes with the description above)
- Report what was merged, whether the build succeeded, and the release URL.

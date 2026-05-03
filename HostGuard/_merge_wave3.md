You are working on the HostGuard mod at C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard

Merge these completed feature branches into main, one at a time:
1. feature/co-host
2. feature/fix-cosmetic-patch
3. feature/fix-preset-load-feedback
4. feature/fix-kickqueue-batch

Steps:
- Make sure you are on the main branch first (git checkout main)
- Merge each branch with: git merge feature/<name>
- If there are merge conflicts, they will almost certainly be in Config.cs or
  HostGuardSettingsPanel.cs — both files just have new lines added in different
  sections, so accept all changes from both sides (keep everything).
- After all merges succeed, verify the project builds: dotnet build
- Fix any build errors before finishing.
- In Plugin.cs, update the version in the [BepInPlugin] attribute from its current value to "3.3.0"
- Push main to GitHub: git push origin main
- Build the project: dotnet build
- Create a GitHub release tagged "hg v3.3.0" and attach the DLL: with this description:

  ## What's new in v3.3.0
  - Co-host system: promote players to co-host so they can kick/ban via chat commands
  - Cosmetic fixes: BanForBadCosmetic toggle (default kick, not ban) + SuspiciousColorIds now wired up
  - Preset load feedback: chat message confirms which preset was loaded
  - KickQueue performance: batch processing during bot waves (up to 3 kicks per frame when queue is large)

  Use: gh release create "hg v3.3.0" "D:\SteamLibrary\steamapps\common\Among Us\BepInEx\plugins\HostGuard.dll" --title "hg v3.3.0" --notes "..."
  (fill in the notes with the description above)
- Report what was merged, whether the build succeeded, and the release URL.

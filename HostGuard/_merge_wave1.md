You are working on the HostGuard mod at C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard

Merge these completed feature branches into main, one at a time:
1. feature/ui-completeness
2. feature/presets
3. feature/import-export-lists
4. feature/auto-return

Steps:
- Make sure you are on the main branch first (git checkout main)
- Merge each branch with: git merge feature/<name>
- If there are merge conflicts, they will almost certainly be in Config.cs or
  HostGuardSettingsPanel.cs — both files just have new lines added in different
  sections, so accept all changes from both sides (keep everything).
- After all merges succeed, verify the project builds: dotnet build
- Fix any build errors before finishing.
- Report what was merged and whether the build succeeded.

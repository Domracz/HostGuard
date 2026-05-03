You are working on the HostGuard mod at C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard

The GitHub release "hg v3.1.0" is missing the compiled DLL. Do the following:

1. Build the project to make sure the DLL is up to date: dotnet build
2. Upload the DLL to the existing release:
   gh release upload "hg v3.1.0" "D:\SteamLibrary\steamapps\common\Among Us\BepInEx\plugins\HostGuard.dll" --clobber

Report whether the upload succeeded.

You are fixing the auto-return to lobby feature in HostGuard at
C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard

Check out the branch feature/auto-return.

## Problem
The feature does nothing when a game ends. The patch likely fires on the wrong method
or the button invocation fails silently.

## Step 1 — Read what was implemented
Read Patches/GameEndPatch.cs fully to see exactly what method was patched and how
buttons are being clicked.

## Step 2 — Find the correct hook
Inspect EndGameManager in the Among Us interop at:
D:\SteamLibrary\steamapps\common\Among Us\BepInEx\interop\Assembly-CSharp.dll

Find:
- Every public method on EndGameManager
- Every button or PassiveButton field on EndGameManager
- The actual method that fires when the game end screen appears

Common candidates to check: ShowRole, Start, Begin, OnDestroy, OnEnable.
Do NOT assume — verify against the actual interop.

## Step 3 — Add a logging probe first
Before attempting button clicks, add a log line at the very start of the patch:
  HostGuardPlugin.Logger.LogInfo("[AutoReturn] Patch fired on [MethodName]");
This confirms the patch is actually triggering. If this log never appears in the
BepInEx console when a game ends, the wrong method is being patched.

## Step 4 — Fix the button invocation
Once the patch fires correctly, find the actual button fields on EndGameManager
from the interop and invoke them properly. Do not guess button names.
Use gameObject.GetComponent<PassiveButton>()?.ReceiveClickDown() or
the button's onClick.Invoke() to simulate a click.

## Step 5 — Verify config is wired
Confirm AutoReturnToLobby.Value and AutoReturnDelay are actually being read.
Confirm AmongUsClient.Instance.AmHost is checked correctly.

## Acceptance criteria
- [ ] Log line "[AutoReturn] Patch fired" appears in BepInEx console when game ends.
- [ ] After the configured delay, the post-game screens are clicked through automatically.
- [ ] Does nothing if AutoReturnToLobby == false or AmHost == false.
- [ ] Build succeeds.

Commit the fix to feature/auto-return when done.

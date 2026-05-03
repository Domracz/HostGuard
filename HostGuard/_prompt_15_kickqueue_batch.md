You are making a performance improvement to HostGuard, an Among Us BepInEx lobby moderator
mod at C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard
(BepInEx plugin, .NET 6, Reactor 2.5.0, plugin ID "com.rareshonour.hostguard").

## Goal
KickQueue currently processes 1 kick per 15 frames (~4/sec at 60fps). During a bot wave
with many simultaneous joins, the queue drains too slowly. Add batch processing when the
queue is large.

## Codebase context
- KickQueue.cs — read this file fully. ProcessNext() is called once per frame from
  HudUpdatePatch.cs. It currently throttles to 1 kick per 15 frames via a _frameSkip
  counter.

## Behaviour spec
- If KickQueue.Count >= 5, process up to 3 kicks per call to ProcessNext() instead of 1.
- If KickQueue.Count < 5, keep the existing 1-per-15-frames throttle unchanged.
- The threshold (5) and batch size (3) can be hardcoded constants — no config needed,
  this is an internal performance detail.
- Keep all existing safety checks (re-verify client still exists before kicking).

## Acceptance criteria
- [ ] When queue has 5+ pending kicks, up to 3 are processed per ProcessNext() call.
- [ ] When queue has < 5 pending kicks, existing 15-frame throttle is unchanged.
- [ ] No NullReferenceException if clients disconnect mid-queue.
- [ ] Build succeeds.

## Out of scope
Making batch size configurable, changing the throttle logic for the normal case.

Before starting, create and switch to a new git branch named feature/fix-kickqueue-batch.
Commit all changes to that branch when done.

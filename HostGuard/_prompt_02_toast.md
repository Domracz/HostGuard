You are implementing a feature for HostGuard, an Among Us BepInEx lobby moderator mod at
C:\Users\radoo\RiderProjects\AmongUsMods\HostGuard
(BepInEx plugin, .NET 6, Reactor 2.5.0, plugin ID "com.rareshonour.hostguard").

## Goal
Show a brief in-game toast notification in the top-left of the HUD whenever HostGuard takes
an action (kick, ban, whitelist add, etc.), giving the host live visual feedback.

## Codebase context
- UI/HostGuardUI.cs — main HUD overlay. Parents everything to HudManager.Instance.transform.
  Auto-discovers sorting layer name and material (lines 82–95) into fields used by all sprites.
  MakeBg() (lines 599–608): creates a SpriteRenderer background GameObject.
  MakeLabel() (lines 610–625): creates a TextMeshPro label GameObject.
  Color palette constants at lines 31–38: PanelBg, BtnGreen, HdrColor, Dim, etc.
  Exposes _hudSpriteMaterial and _hudSortingLayer — make these internal static so the new
  NotificationManager can read them.
- ChatHelper.SendLocalMessage(string) — chat-only; do NOT use this for toast display.
- Config.cs — ConfigEntry<T> fields bound in Initialize(). New config goes here.
- UI/HostGuardSettingsPanel.cs — declarative settings rows via UIFactory.
- KickQueue.Enqueue(clientId, ban, name) — called at every kick/ban site.
- HostGuardPlugin.Logger.LogInfo/LogWarning — logging.

## Behaviour spec
- Create a static NotificationManager class in a new file UI/NotificationManager.cs.
- Public API: NotificationManager.Show(string message).
- Each toast: one line of text, slightly transparent dark gray background (PanelBg color,
  alpha ~0.7), positioned top-left of the HUD.
- Toasts auto-dismiss after 4 seconds (use a coroutine via HostGuardPlugin.Instance.StartCoroutine).
- Up to 4 toasts visible simultaneously, stacked vertically. When a 5th arrives, oldest dismisses
  immediately. Each toast shifts position as others dismiss.
- Only shown when HudManager.Instance is active and non-null.
- Feature toggle: ShowNotifications (bool, default true, section "UI").
- NotificationManager.Show() does nothing if ShowNotifications.Value == false.

**Call NotificationManager.Show() at every KickQueue.Enqueue call site, with a message like:**
- "PlayerName was kicked (banned word)"
- "PlayerName was banned (bot detected)"
- "PlayerName was kicked (invalid RPC)"
- "PlayerName was banned (invalid cosmetic)"
- "PlayerName added to whitelist" (at the whitelist-add command site)
Adapt the message text to the actual trigger at each call site.

## Implementation hints
- Parent toast GameObjects to HudManager.Instance.transform (same as main panel).
- Use sorting order > 500 so toasts appear above the HostGuard panel.
- Use the same _hudSpriteMaterial and _hudSortingLayer from HostGuardUI (make them
  internal static). Mirror the MakeBg/MakeLabel pattern exactly — do not invent new
  Unity object setup.
- Add ShowNotifications ConfigEntry<bool> to Config.cs.
- Add a toggle row for ShowNotifications in UI/HostGuardUI.cs under a new "UI" section header.
  IMPORTANT: add the row to UI/HostGuardUI.cs — that is the file that renders in-game.
  Do NOT add rows to HostGuardSettingsPanel.cs — that file does not render in-game.

## Config & UI surface
- ShowNotifications — bool, default true, section "UI", key "ShowNotifications".
- New "UI" section header + one toggle row in UI/HostGuardUI.cs.

## Acceptance criteria
- [ ] Toast appears top-left when any player is kicked/banned by any feature.
- [ ] Toast auto-dismisses after ~4 seconds.
- [ ] Up to 4 stack without overlap; 5th displaces oldest.
- [ ] ShowNotifications = false suppresses all toasts.
- [ ] Toggle visible in settings panel.
- [ ] No NullReferenceException if HudManager is not yet active.
- [ ] Build succeeds.

## Out of scope
Click-to-dismiss, sound effects, different colors per action type, toast history log.

Before starting, create and switch to a new git branch named feature/toast-notifications.
Commit all changes to that branch when done.

using HarmonyLib;

[HarmonyPatch(typeof(GameStartManager))]
public static class LobbyTimerPatch
{
    private static bool _triggered;

    [HarmonyPatch(nameof(GameStartManager.Start))]
    [HarmonyPostfix]
    public static void StartPostfix()
    {
        _triggered = false;
    }

    [HarmonyPatch(nameof(GameStartManager.Update))]
    [HarmonyPostfix]
    public static void UpdatePostfix(GameStartManager __instance)
    {
        if (_triggered) return;
        if (!HostGuardConfig.AutoStartIfClosing.Value) return;
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return;

        float timer = __instance.countDownTimer;
        int threshold = HostGuardConfig.AutoStartThreshold.Value;

        // Only act if timer is actively counting down (positive) and at/below threshold
        if (timer <= 0f || timer > threshold) return;

        int playerCount = GameData.Instance != null ? GameData.Instance.PlayerCount : 0;
        if (playerCount < HostGuardConfig.MinPlayersToAutoStart.Value) return;

        _triggered = true;

        // Set countdown to 5 seconds to trigger normal game start flow
        if (__instance.countDownTimer > 5f)
            __instance.countDownTimer = 5f;

        ChatHelper.SendLocalMessage("[HostGuard] Lobby closing \u2014 auto-starting game!");
        HostGuardPlugin.Logger.LogInfo("[AutoStart] Lobby closing, auto-starting.");
    }
}

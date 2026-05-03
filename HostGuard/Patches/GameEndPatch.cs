using System.Collections;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.Start))]
public static class GameEndPatch
{
    public static void Postfix(EndGameManager __instance)
    {
        if (!HostGuardConfig.AutoReturnToLobby.Value) return;
        if (!AmongUsClient.Instance.AmHost) return;

        HostGuardPlugin.Logger.LogInfo("[AutoReturn] Game ended, scheduling return to lobby...");
        __instance.StartCoroutine(AutoReturnCoroutine(__instance));
    }

    private static IEnumerator AutoReturnCoroutine(EndGameManager manager)
    {
        float delay = HostGuardConfig.AutoReturnDelay.Value;
        if (delay < 0f) delay = 0f;

        yield return new WaitForSeconds(delay);

        HostGuardPlugin.Logger.LogInfo("[AutoReturn] Clicking to return to lobby");

        var navigation = manager.Navigation;
        if (navigation == null)
        {
            HostGuardPlugin.Logger.LogWarning("[AutoReturn] Navigation is null, cannot auto-return.");
            yield break;
        }

        // Click continue (advances past role/results screen)
        if (navigation.ContinueButton != null)
        {
            var passive = navigation.ContinueButton.GetComponent<PassiveButton>();
            if (passive != null)
                passive.OnClick.Invoke();
        }

        yield return new WaitForSeconds(1f);

        // Click play again (returns to lobby)
        navigation.NextGame();

        ChatHelper.SendLocalMessage("[HostGuard] Returning to lobby...");
    }
}

using System.Collections;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.ShowButtons))]
public static class GameEndPatch
{
    public static void Postfix(EndGameManager __instance)
    {
        HostGuardPlugin.Logger.LogInfo("[AutoReturn] Patch fired on ShowButtons");

        if (!HostGuardConfig.AutoReturnToLobby.Value)
        {
            HostGuardPlugin.Logger.LogInfo("[AutoReturn] Disabled in config, skipping.");
            return;
        }
        if (!AmongUsClient.Instance.AmHost)
        {
            HostGuardPlugin.Logger.LogInfo("[AutoReturn] Not host, skipping.");
            return;
        }

        HostGuardPlugin.Logger.LogInfo($"[AutoReturn] Scheduling return in {HostGuardConfig.AutoReturnDelay.Value}s...");
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

        // Click continue (advances past role/XP screen)
        if (navigation.ContinueButton != null)
        {
            HostGuardPlugin.Logger.LogInfo("[AutoReturn] Clicking ContinueButton...");
            var passive = navigation.ContinueButton.GetComponent<PassiveButton>();
            if (passive != null)
            {
                passive.ReceiveClickDown();
                passive.ReceiveClickUp();
            }
            else
            {
                HostGuardPlugin.Logger.LogWarning("[AutoReturn] ContinueButton has no PassiveButton component.");
            }
        }
        else
        {
            HostGuardPlugin.Logger.LogInfo("[AutoReturn] No ContinueButton, skipping to NextGame.");
        }

        yield return new WaitForSeconds(1f);

        // Click play again (returns to lobby)
        HostGuardPlugin.Logger.LogInfo("[AutoReturn] Calling NextGame()...");
        navigation.NextGame();

        ChatHelper.SendLocalMessage("[HostGuard] Returning to lobby...");
    }
}

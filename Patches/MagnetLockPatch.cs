using HarmonyLib;
using MagnetLock.Helpers;
using System.Collections;
using UnityEngine;

namespace MagnetLock.Patches;

public class PatchMagnet
{
    public static Sprite? defaultHoverIcon;
    public static Sprite? defaultDisabledHoverIcon;
    public static InteractTrigger? magnetLeverTrigger;

    public static void Initialize()
    {
        var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

        harmony.PatchAll(typeof(StartOfRound));
    }

    [HarmonyPatch(typeof(StartOfRound))]
    private class StartOfRoundPatch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        private static void UpdateMagnetStateOrbitInit()
        {
            var magnetLeverTriggerLocal = StartOfRound.Instance.magnetLever.GetComponent<InteractTrigger>();
            if (magnetLeverTriggerLocal != null)
            {
                defaultHoverIcon = magnetLeverTriggerLocal.hoverIcon;
                defaultDisabledHoverIcon = magnetLeverTriggerLocal.disabledHoverIcon;
                magnetLeverTrigger = magnetLeverTriggerLocal;
            }
            MagnetLock.Logger.LogInfo("Awake() patch triggered");
            ChangeMagnetStateLocal(false);

        }

        [HarmonyPatch("StartGame")]
        [HarmonyPostfix]
        private static void UpdateMagnetStateGameStart()
        {
            CoroutineManager.StartCoroutine(EnableMagnetLever());
        }

        [HarmonyPatch(typeof(StartMatchLever), "EndGame")]
        [HarmonyPrefix]
        private static void UpdateMagnetStateGameEnd()
        {
            StartOfRound.Instance.SetMagnetOnServerRpc(true);
            MagnetLock.Logger.LogInfo("Ship Magnet non-operational");
            ChangeMagnetStateLocal(false);
        }

        [HarmonyPatch("SetMagnetOnServerRpc")]
        [HarmonyPrefix]
        private static bool AllowMagnetInteraction(StartOfRound __instance)
        {
            return __instance.shipHasLanded;
        }

        private static void ChangeMagnetStateLocal(bool enabled)
        {
            if (magnetLeverTrigger != null)
            {
                if (enabled)
                {
                    magnetLeverTrigger.interactable = true;
                    magnetLeverTrigger.hoverIcon = defaultHoverIcon;
                    magnetLeverTrigger.disabledHoverIcon = defaultDisabledHoverIcon;
                }
                else
                {
                    magnetLeverTrigger.interactable = false;
                    magnetLeverTrigger.hoverIcon = null;
                    magnetLeverTrigger.disabledHoverIcon = null;
                }
                MagnetLockNetworkHelper.Instance.UpdateMagnetStateClientRpc(enabled);
            }
        }

        private static IEnumerator EnableMagnetLever()
        {
            yield return new WaitUntil(() => StartOfRound.Instance.shipHasLanded);
            MagnetLock.Logger.LogInfo("Ship Magnet operational");
            ChangeMagnetStateLocal(true);
        }
    }
}
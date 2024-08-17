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
            MagnetLock.Logger.LogInfo("Triggering Awake() patch");
            ChangeMagnetStateLocal(false);

        }

        [HarmonyPatch("StartGame")]
        [HarmonyPostfix]
        private static void UpdateMagnetStateGameStart()
        {
            MagnetLock.Logger.LogInfo("Triggering StartGame() patch");
            CoroutineManager.StartCoroutine(UpdateMagnetState(true));
        }

        [HarmonyPatch("SetShipReadyToLand")]
        [HarmonyPostfix]
        private static void UpdateMagnetStateOrbit()
        {
            MagnetLock.Logger.LogInfo("Triggering SetShipReadyToLand() patch");
            ChangeMagnetStateLocal(false);
        }

        [HarmonyPatch("SetMagnetOnServerRpc")]
        [HarmonyPrefix]
        private static bool AllowMagnetInteraction(StartOfRound __instance)
        {
            if (!__instance.shipHasLanded) return false;
            return true;
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

        private static IEnumerator UpdateMagnetState(bool state)
        {
            yield return new WaitUntil(() => StartOfRound.Instance.shipHasLanded);
            ChangeMagnetStateLocal(state);
        }
    }

}
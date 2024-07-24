using HarmonyLib;
using MagnetLock.Helpers;
using UnityEngine;
using UnityEngine.UIElements.UIR;

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

            MagnetLock.Logger.LogDebug("Triggering Awake() patch");
            ChangeMagnetStateLocal(false);
            
        }

        [HarmonyPatch("StartGame")]
        [HarmonyPostfix]
        private static void UpdateMagnetStateGameStart()
        {
            MagnetLock.Logger.LogDebug("Triggering StartGame() patch");
            ChangeMagnetStateLocal(true);
        }

        [HarmonyPatch("SetShipReadyToLand")]
        [HarmonyPostfix]
        private static void UpdateMagnetStateOrbit()
        {
            MagnetLock.Logger.LogDebug("Triggering SetShipReadyToLand() patch");
            ChangeMagnetStateLocal(false);
        }

        public static void ChangeMagnetStateLocal(bool state)
        {
            if (magnetLeverTrigger != null)
            {
                if (state == true)
                {
                    magnetLeverTrigger.interactable = true;
                    magnetLeverTrigger.hoverIcon = defaultHoverIcon;
                    magnetLeverTrigger.disabledHoverIcon = defaultDisabledHoverIcon;

                } else {

                    magnetLeverTrigger.interactable = false;
                    magnetLeverTrigger.hoverIcon = null;
                    magnetLeverTrigger.disabledHoverIcon = null;
                }

                MagnetLockNetworkHelper.Instance.UpdateMagnetStateClientRpc(state);

            }
        }
    }
    
}
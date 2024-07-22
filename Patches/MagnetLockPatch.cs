using HarmonyLib;
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

            UpdateMagnetLock(false);
        }

        [HarmonyPatch("StartGame")]
        [HarmonyPostfix]
        private static void UpdateMagnetStateGameStart()
        {
            UpdateMagnetLock(true);
        }

        [HarmonyPatch("SetShipReadyToLand")]
        [HarmonyPostfix]
        private static void UpdateMagnetStateOrbit()
        {
            UpdateMagnetLock(false);
        }
    }

    public static void UpdateMagnetLock(bool state)
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
            
        }
    }
    
}
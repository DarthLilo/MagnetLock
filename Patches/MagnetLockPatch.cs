using HarmonyLib;
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

        harmony.PatchAll(typeof(PatchMagnet));
    }

    [HarmonyPatch(typeof(StartOfRound), "Awake")]
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
        ChangeMagnetState(enabled: false);
    }

    [HarmonyPatch(typeof(RoundManager), "FinishGeneratingNewLevelClientRpc")]
    [HarmonyPostfix]
    private static void UpdateMagnetStateGameStart()
    {
        GameNetworkManager.Instance.StartCoroutine(EnableMagnetLever());
    }

    [HarmonyPatch(typeof(StartMatchLever), "EndGame")]
    [HarmonyPrefix]
    private static void EnableMagnetOnShipLeave()
    {
        StartOfRound.Instance.SetMagnetOnServerRpc(true);
    }

    [HarmonyPatch(typeof(StartOfRound), "EndGameClientRpc")]
    [HarmonyPrefix]
    private static void UpdateMagnetStateGameEnd()
    {
        MagnetLock.Logger.LogInfo("Lever disabled");
        ChangeMagnetState(enabled: false);
    }

    [HarmonyPatch(typeof(StartOfRound), "SetMagnetOnServerRpc")]
    [HarmonyPrefix]
    private static bool AllowMagnetInteraction(StartOfRound __instance)
    {
        return __instance.shipHasLanded;
    }

    private static IEnumerator EnableMagnetLever()
    {
        yield return new WaitUntil(() => StartOfRound.Instance.shipHasLanded);
        ChangeMagnetState(enabled: true);
        MagnetLock.Logger.LogInfo("Lever enabled");
    }

    private static void ChangeMagnetState(bool enabled)
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
        }
    }
}
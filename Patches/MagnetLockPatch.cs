using HarmonyLib;

namespace MagnetLock.Patches;

public class PatchMagnet
{   

    public static void Initialize()
    {
        var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

        harmony.PatchAll(typeof(MLAnimatedObjectTrigger));
    }
    

    [HarmonyPatch(typeof(AnimatedObjectTrigger))]
    private class MLAnimatedObjectTrigger
    {
        [HarmonyPatch("TriggerAnimation")]
        [HarmonyPrefix]
        private static bool TriggerAnimationPrefix(AnimatedObjectTrigger __instance)
        {   
            if (__instance.name == "MagnetLever")
            { 
                if (StartOfRound.Instance.inShipPhase)
                {
                    return false;
                }
            }
            return true;
        }
    }
    
}
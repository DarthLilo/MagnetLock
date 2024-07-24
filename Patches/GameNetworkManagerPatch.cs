using HarmonyLib;
using MagnetLock.Helpers;
using Unity.Netcode;

namespace MagnetLock.Patches;

[HarmonyPatch(typeof(GameNetworkManager))]
public class GameNetworkManagerPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    private static void StartPatch(GameNetworkManager __instance)
    {
        __instance.gameObject.AddComponent<MagnetLockNetworkHelper>();
        __instance.gameObject.AddComponent<NetworkObject>();
        MagnetLock.Logger.LogInfo("Network Helper Added!");
    }
}
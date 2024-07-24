using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MagnetLock.Patches;
using UnityEngine;

namespace MagnetLock;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]

public class MagnetLock : BaseUnityPlugin
{
    public static MagnetLock Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony? Harmony { get; set; }

    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;

        Patch();
        PatchMagnet.Initialize();

        // NETCODE PATCHING STUFF

        Logger.LogInfo($"Running netcode patchers");
        
        var types = Assembly.GetExecutingAssembly().GetTypes();
           foreach (var type in types)
           {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {   
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
           }
        
        // NETCODE PATCHING STUFF

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    internal static void Patch()
    {
        Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

        Logger.LogDebug("Patching...");

        Harmony.PatchAll();

        Logger.LogDebug("Finished patching!");
    }

}

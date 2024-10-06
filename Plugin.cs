using BepInEx;
using BepInEx.Logging;
using BepInExPlugin;
using HarmonyLib;

namespace CardSeller;

[BepInPlugin(CSInfo.PLUGIN_ID, CSInfo.PLUGIN_NAME, CSInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private readonly Harmony harmony = new Harmony(CSInfo.PLUGIN_NAME);
        
    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        this.harmony.PatchAll();
        Logger.LogInfo($"Plugin {CSInfo.PLUGIN_ID} is loaded!");
    }
}

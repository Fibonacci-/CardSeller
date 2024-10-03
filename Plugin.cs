using BepInEx;
using BepInEx.Logging;
using BepInExPlugin;

namespace CardSeller;

[BepInPlugin(CSInfo.PLUGIN_ID, CSInfo.PLUGIN_NAME, CSInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
        
    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {CSInfo.PLUGIN_ID} is loaded!");
    }
}

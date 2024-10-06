using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInExPlugin;
using HarmonyLib;
using System;
using UnityEngine;

namespace CardSeller;

[BepInPlugin(CSInfo.PLUGIN_ID, CSInfo.PLUGIN_NAME, CSInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private readonly Harmony harmony = new Harmony(CSInfo.PLUGIN_NAME);

    //internal static float SellOnlyGreaterThanMP = 0.50f;
    //internal static float SellOnlyLessThanMP = 100.00f;
    internal static ConfigEntry<float> m_ConfigSellOnlyGreaterThanMP;
    internal static ConfigEntry<float> m_ConfigSellOnlyLessThanMP;
    internal static ConfigEntry<KeyboardShortcut> m_ConfigKeyboardTriggerCardSet;

    //private ConfigEntry<bool> m_SetLowerPriceLimit;
    //private ConfigEntry<bool> m_SetUpperPriceLimit;

    private void InitConfig()
    {
        m_ConfigSellOnlyGreaterThanMP = Config.Bind("General", "SellOnlyGreaterThan", 0.50f, "Ignore cards in the album with a market value below this.");
        m_ConfigSellOnlyLessThanMP = Config.Bind("General", "SellOnlyLessThan", 100.00f, "Ignore cards in the album with a market value above this.");
        m_ConfigKeyboardTriggerCardSet = Config.Bind<KeyboardShortcut>("General", "SetOutCards", new KeyboardShortcut(KeyCode.F9, Array.Empty<KeyCode>()), "Keyboard shortcut to set out cards.");
    }
        
    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        InitConfig();
        this.harmony.PatchAll();
        Logger.LogInfo($"Plugin {CSInfo.PLUGIN_ID} is loaded!");
    }

    private void OnDestroy()
    {
        this.harmony?.UnpatchSelf();
        Logger.LogInfo($"Plugin {CSInfo.PLUGIN_ID} is unloaded!");
    }
}

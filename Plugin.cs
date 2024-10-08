using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInExPlugin;
using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;

namespace CardSeller;

[BepInPlugin(CSInfo.PLUGIN_ID, CSInfo.PLUGIN_NAME, CSInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private readonly Harmony harmony = new Harmony(CSInfo.PLUGIN_NAME);

    //general config settings
    internal static ConfigEntry<float> m_ConfigSellOnlyGreaterThanMP;
    internal static ConfigEntry<float> m_ConfigSellOnlyLessThanMP;
    internal static ConfigEntry<KeyboardShortcut> m_ConfigKeyboardTriggerCardSet;
    internal static ConfigEntry<bool> m_ConfigOnlySellDuplicates;

    //filter config settings
    internal static ConfigEntry<bool> m_ConfigShouldSellTetramonCards;
    internal static ConfigEntry<bool> m_ConfigShouldSellDestinyCards;
    internal static ConfigEntry<bool> m_ConfigShouldSellGhostCards;
    internal static ConfigEntry<bool> m_ConfigShouldSellDestinyGhostCards;

    //trigger config settings
    internal static ConfigEntry<bool> m_ConfigShouldTriggerOnCustomerCardPickup;
    internal static ConfigEntry<bool> m_ConfigShouldTriggerOnDayStart;

    //mod int config settings
    internal static ConfigEntry<bool> m_ConfigTryTriggerAutoSetPricesMod;

    private void InitConfig()
    {
        //general config init
        m_ConfigSellOnlyGreaterThanMP = Config.Bind("General", "SellOnlyGreaterThan", 0.50f, "Ignore cards in the album with a market value below this.");
        m_ConfigSellOnlyLessThanMP = Config.Bind("General", "SellOnlyLessThan", 100.00f, "Ignore cards in the album with a market value above this.");
        m_ConfigKeyboardTriggerCardSet = Config.Bind<KeyboardShortcut>("General", "SetOutCardsKey", new KeyboardShortcut(KeyCode.F9, Array.Empty<KeyCode>()), "Keyboard shortcut to set out cards.");
        m_ConfigOnlySellDuplicates = Config.Bind("General", "SellOnlyDuplicates", false, "Ignore cards in the album with a quantity of 1");

        //filter config init
        m_ConfigShouldSellTetramonCards = Config.Bind("Filters", "ShouldSellTetramonCards", true, "Do you want to sell your Tetramon set cards?");
        m_ConfigShouldSellDestinyCards = Config.Bind("Filters", "ShouldSellDestinyCards", true, "Do you want to sell your Destiny set cards?");
        m_ConfigShouldSellGhostCards = Config.Bind("Filters", "ShouldSellGhostCards", false, "Do you want to sell your white Ghost cards?");
        m_ConfigShouldSellDestinyGhostCards = Config.Bind("Filters", "ShouldSellDestinyGhostCards", false, "Do you want to sell your dimension (aka black/Destiny) ghost cards?");

        //trigger config init
        m_ConfigShouldTriggerOnCustomerCardPickup = Config.Bind("Triggers", "ShouldTriggerOnCardPickup", false, "Do you want your cards to automatically be placed on all empty shelves whenever a customer picks up a card?");
        m_ConfigShouldTriggerOnDayStart = Config.Bind("Triggers", "ShouldTriggerOnDayStart", true, "Do you want your cards to automatically be placed on all empty shelves when the day begins?");
        
        //mod int config init
        m_ConfigTryTriggerAutoSetPricesMod = Config.Bind("Mod_Integration", "ShouldTriggerAutoSetPricesMod", true, "If Auto Set Prices mod is installed, ask it to set the price of cards this mod sets on shelves? NOTE: Make sure AutoSetPrices option 'NewDayCardAutoPrice' is enabled!");

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

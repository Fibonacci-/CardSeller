using BepInEx.Logging;
using CardSeller;
using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CardSeller;

[HarmonyPatch]
public static class PatchIt
{
    private static bool isRunning = false;

    private static void DoShelfPut()
    {
        if(isRunning) return;
        isRunning = true;

        List<CardShelf> cardShelfList = CSingleton<ShelfManager>.Instance.m_CardShelfList;
        foreach (CardShelf shelf in cardShelfList)
        {
            List<InteractableCardCompartment> cardCompartments = shelf.GetCardCompartmentList();
            foreach (InteractableCardCompartment cardCompart in cardCompartments)
            {
                if (cardCompart.m_StoredCardList.Count == 0 && !cardCompart.m_ItemNotForSale)
                {
                    //instantiate card
                    //find items in the CardCollectedList where we have more than 0 in inventory
                    List<int> cardList = CPlayerData.GetCardCollectedList(ECardExpansionType.Tetramon, false);

                    CardData cardData = null;

                    for (int i = 0; i < cardList.Count; i++)
                    {
                        int numHeldCards = cardList[i];
                        if (numHeldCards > 0)
                        {
                            //when we find a held card, grab the card data for it and break out of the loop
                            CardData insp = CPlayerData.GetCardData(i, ECardExpansionType.Tetramon, false);
                            Plugin.Logger.LogInfo("Finding held cards: Player has " + numHeldCards + " cards of monster type " + insp.monsterType.ToString());
                            float currentMP = CPlayerData.GetCardMarketPrice(insp);
                            if (currentMP > Plugin.m_ConfigSellOnlyGreaterThanMP.Value && currentMP < Plugin.m_ConfigSellOnlyLessThanMP.Value)
                            {
                                cardData = insp;
                                Plugin.Logger.LogInfo("Card price " + currentMP + " meets price restrictions. Continuing to place...");
                                break;
                            }
                            else
                            {
                                Plugin.Logger.LogInfo("Card price is outside price restriction band (" + currentMP + "). Continuing search...");
                            }

                        }
                    }

                    if (cardData != null && cardData.monsterType != EMonsterType.None)
                    {
                        //ref card shelf load from save method
                        Plugin.Logger.LogInfo("Initing game object for " + cardData.monsterType.ToString());
                        Card3dUIGroup cardUi = CSingleton<Card3dUISpawner>.Instance.GetCardUI();
                        InteractableCard3d component = ShelfManager.SpawnInteractableObject(EObjectType.Card3d).GetComponent<InteractableCard3d>();
                        cardUi.m_IgnoreCulling = true;
                        cardUi.m_CardUI.SetFoilCullListVisibility(true);
                        cardUi.m_CardUI.ResetFarDistanceCull();
                        cardUi.m_CardUI.SetCardUI(cardData);
                        cardUi.transform.position = component.transform.position;
                        cardUi.transform.rotation = component.transform.rotation;
                        component.SetCardUIFollow(cardUi);
                        component.SetEnableCollision(false);
                        cardCompart.SetCardOnShelf(component);
                        cardUi.m_IgnoreCulling = false;
                        Plugin.Logger.LogInfo("Reducing held card count by 1 for monster " + cardData.monsterType.ToString());
                        CPlayerData.ReduceCard(cardData, 1);
                    }

                }
            }
        }
        isRunning = false;
    }

    [HarmonyPatch(typeof(CPlayerData), "ReduceCard")]
    [HarmonyPostfix]
    private static void OnReduceCardPostFix(CPlayerData __instance, CardData cardData, int reduceAmount)
    {
        Plugin.Logger.LogInfo("Reduce card was called!");

        Plugin.Logger.LogMessage("Reduced " + cardData.monsterType.ToString() + " by " + reduceAmount);
        int cardIndex = CPlayerData.GetCardSaveIndex(cardData);
        int heldCards = CPlayerData.GetCardAmountByIndex(cardIndex, ECardExpansionType.Tetramon, false);
        Plugin.Logger.LogMessage("Player card amount for monster " + cardData.monsterType.ToString() + " with index " + cardIndex + " is " + heldCards + " num cards.");
    }

    [HarmonyPatch(typeof(Customer), "TakeCardFromShelf")]
    [HarmonyPostfix]
    private static void OnCustomerTakeCardFromShelfPostFix(Customer __instance, List<InteractableCard3d> ___m_CardInBagList)
    {
        Plugin.Logger.LogInfo("Customer took a card from the shelf! Filling all empty card sale shelves...");

        if (Plugin.m_ConfigShouldTriggerOnCustomerCardPickup.Value)
        {
            DoShelfPut();
        }
        
    }

    [HarmonyPatch(typeof(CGameManager), "Update")]
    [HarmonyPostfix]
    public static void OnGameManagerUpdatePostfix()
    {
        if (Plugin.m_ConfigKeyboardTriggerCardSet.Value.IsDown())
        {
            Plugin.Logger.LogInfo("Shortcut key was pressed!");
            DoShelfPut();
        }
    }

}

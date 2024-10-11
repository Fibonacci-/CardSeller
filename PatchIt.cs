using BepInEx.Logging;
using CardSeller;
using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace CardSeller;

[HarmonyPatch]
public static class PatchIt
{
    private static bool isRunning = false;

    //retrieve an unsorted list of all cards in a given expansion that match plugin settings
    private static List<CardData> GetCompatibleCards(ECardExpansionType expansionType, bool findGhostDimensionCards = false)
    {
        List<CardData> retVal = new List<CardData>();

        //allow for selling only duplicates
        int numHeldCardsShouldBeOver = Plugin.m_ConfigKeepCardQty.Value;

        //find items in the CardCollectedList where we have more than X in inventory
        List<int> cardList = CPlayerData.GetCardCollectedList(expansionType, findGhostDimensionCards);

        for (int i = 0; i < cardList.Count; i++)
        {
            //integer value in list index i contains the count of held cards of that index
            int numHeldCards = cardList[i];
            if (numHeldCards > numHeldCardsShouldBeOver)
            {
                //when we find a held card, grab the card data for it and put it in the card array
                CardData insp = CPlayerData.GetCardData(i, expansionType, findGhostDimensionCards);
                float currentMP = CPlayerData.GetCardMarketPrice(insp);

                //check to see if the card's price is within the bounds of the high/low inclusion configuration
                if (currentMP > Plugin.m_ConfigSellOnlyGreaterThanMP.Value && currentMP < Plugin.m_ConfigSellOnlyLessThanMP.Value)
                {
                    retVal.Add(insp);
                    Plugin.Logger.LogInfo("Finding held cards: Player has " + numHeldCards + " cards of monster type " + insp.monsterType.ToString() + ". Card price " + currentMP + " meets price restrictions. Adding to list...");
                }
                else
                {
                    Plugin.Logger.LogInfo("Card price is outside price restriction band (" + currentMP + "). Continuing search...");
                }

            }
        }
        return retVal;
    }

    private static async Task<List<CardData>> GetCompatibleCards()
    {
        return await Task<List<CardData>>.Run(() =>
        {
            List<CardData> allMatchingCards = new List<CardData>();
            //pull in whatever card lists are enabled
            if (Plugin.m_ConfigShouldSellTetramonCards.Value)
            {
                Plugin.Logger.LogInfo("Tetramon selling enabled. Searching for compatible Tetramon cards...");
                allMatchingCards.AddRange(GetCompatibleCards(ECardExpansionType.Tetramon));
            }
            if (Plugin.m_ConfigShouldSellDestinyCards.Value)
            {
                Plugin.Logger.LogInfo("Destiny selling enabled. Searching for compatible Destiny cards...");
                allMatchingCards.AddRange(GetCompatibleCards(ECardExpansionType.Destiny));
            }
            if (Plugin.m_ConfigShouldSellGhostCards.Value)
            {
                Plugin.Logger.LogInfo("Ghost selling enabled. Searching for compatible Ghost cards...");
                allMatchingCards.AddRange(GetCompatibleCards(ECardExpansionType.Ghost, false));
            }
            if (Plugin.m_ConfigShouldSellDestinyGhostCards.Value)
            {
                Plugin.Logger.LogInfo("Destiny Ghost selling enabled. Searching for compatible Destiny Ghost cards...");
                allMatchingCards.AddRange(GetCompatibleCards(ECardExpansionType.Ghost, true));
            }


            if (allMatchingCards.Count > 0)
            {
                //get the largest card
                allMatchingCards.Sort((c, d) => CPlayerData.GetCardMarketPrice(d).CompareTo(CPlayerData.GetCardMarketPrice(c)));
                Plugin.Logger.LogInfo("First index of sorted array MP: " + CPlayerData.GetCardMarketPrice(allMatchingCards[0]));
                Plugin.Logger.LogInfo("Last index of sorted array MP: " + CPlayerData.GetCardMarketPrice(allMatchingCards[allMatchingCards.Count - 1]));
            }
            Plugin.Logger.LogInfo("Finished finding cards.");
            return allMatchingCards;
        });
    }



    private static async void DoShelfPut()
    {
        if(isRunning) return;
        isRunning = true;

        List<CardData> allCardsSorted = await GetCompatibleCards();
        
        int totalMatchingCards = 0;
        foreach (CardData card in allCardsSorted)
        {
            int tempNumCards = CPlayerData.GetCardAmount(card);
            if (tempNumCards > Plugin.m_ConfigKeepCardQty.Value)
            {
                //we would have already gotten only cards with 2 or more quantity
                //subtract 1 from expected qty
                tempNumCards -= Plugin.m_ConfigKeepCardQty.Value;
                totalMatchingCards += tempNumCards;
            }
        }
        Plugin.Logger.LogInfo("Got " + totalMatchingCards + " cards to place");

        int placedCards = 0;

        //find all card shelves in the shop
        List<CardShelf> cardShelfList = CSingleton<ShelfManager>.Instance.m_CardShelfList;
        foreach (CardShelf shelf in cardShelfList)
        {
            //find all the compartments in that shelf
            List<InteractableCardCompartment> cardCompartments = shelf.GetCardCompartmentList();
            int maxLoop = cardCompartments.Count;
            if(maxLoop > totalMatchingCards) maxLoop = totalMatchingCards;

            for(int j = 0; j < cardCompartments.Count; j++)
            {
                //will pass by ref later. can't use foreach
                InteractableCardCompartment cardCompart = cardCompartments[j];
                if (cardCompart.m_StoredCardList.Count == 0 && !cardCompart.m_ItemNotForSale
                    && !shelf.GetIsBoxedUp())
                {
                    //instantiate card
                    CardData cardData = allCardsSorted.FirstOrDefault();
                    
                    if (cardData != null && cardData.monsterType != EMonsterType.None)
                    {
                        //ref card shelf load from save method
                        //dunno what most of it does precisely but it works
                        //basically, create a card such that it can be placed on a shelf and be picked up by customers
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

                        //actually set the card on the shelf
                        cardCompart.SetCardOnShelf(component);
                        cardUi.m_IgnoreCulling = false;

                        //decrement inventory for that card
                        Plugin.Logger.LogInfo("Reducing held card count by 1 for monster " + cardData.monsterType.ToString());
                        CPlayerData.ReduceCard(cardData, 1);

                        //allow for selling only duplicates
                        int numHeldCardsShouldBeOver = Plugin.m_ConfigKeepCardQty.Value;
                        if (CPlayerData.GetCardAmount(cardData) == numHeldCardsShouldBeOver)
                        {
                            allCardsSorted.Remove(cardData);
                        }

                        placedCards++;

                        if (Harmony.HasAnyPatches("AutoSetPrices") && Plugin.m_ConfigTryTriggerAutoSetPricesMod.Value)
                        {
                            //try to ask it to update prices
                            try
                            {
                                Plugin.Logger.LogInfo("Asking AutoSetPrices to update price in card slot.");
                                //this lives in another function because the library is loaded on entry to the function that references it
                                TellAutoSetPrices(ref cardCompart);
                            }
                            catch (Exception e)
                            {
                                Plugin.Logger.LogError("Couldn't ask AutoSetPrices to update price in card slot! Stacktrace:\r\n" + e.Message);
                            }
                        }
                    }

                }
            }
        }
        isRunning = false;
        if (Plugin.m_ConfigShouldShowProgressPopUp.Value)
        {
            string translation = "";
            if (totalMatchingCards == 0)
            {
                translation = "Auto Card Place: No cards matching configured filters!";
            }
            else
            {
                translation = placedCards + " cards of " + totalMatchingCards + " possible matching cards placed.";
            }
            for (int index = 0; index < CSingleton<NotEnoughResourceTextPopup>.Instance.m_ShowTextGameObjectList.Count; ++index)
            {
                if (!CSingleton<NotEnoughResourceTextPopup>.Instance.m_ShowTextGameObjectList[index].activeSelf)
                {
                    CSingleton<NotEnoughResourceTextPopup>.Instance.m_ShowTextList[index].text = translation;
                    CSingleton<NotEnoughResourceTextPopup>.Instance.m_ShowTextGameObjectList[index].gameObject.SetActive(true);
                    break;
                }
            }
        }
    }

    private static void TellAutoSetPrices(ref InteractableCardCompartment cardCompart)
    {
        //this needs to stay in its own function
        //otherwise, Unity will try to load AutoSetPrices.dll when DoShelfPut() is triggered
        //regardless of the Harmony.HasAnyPatches check
        //which will obviously crash the mod's shelf put attempt
        //so: only reference the function that uses the AutoSetPrices library AFTER checking to see if it's present
        AutoSetPrices.AllPatchs.CardCompartOnMouseButtonUpPostfix(ref cardCompart);
    }

    [HarmonyPatch(typeof(CPlayerData), "ReduceCard")]
    [HarmonyPostfix]
    private static void OnReduceCardPostFix(CPlayerData __instance, CardData cardData, int reduceAmount)
    {
        Plugin.Logger.LogInfo("Reduce card was called!");

        Plugin.Logger.LogInfo("Reduced " + cardData.monsterType.ToString() + " by " + reduceAmount);
        int cardIndex = CPlayerData.GetCardSaveIndex(cardData);
        int heldCards = CPlayerData.GetCardAmountByIndex(cardIndex, cardData.expansionType, cardData.isDestiny);
        Plugin.Logger.LogInfo("Player card amount for monster " + cardData.monsterType.ToString() + " with index " + cardIndex + " is " + heldCards + " num cards.");
    }

    [HarmonyPatch(typeof(Customer), "TakeCardFromShelf")]
    [HarmonyPostfix]
    private static void OnCustomerTakeCardFromShelfPostFix(Customer __instance, List<InteractableCard3d> ___m_CardInBagList)
    {
        if (Plugin.m_ConfigShouldTriggerOnCustomerCardPickup.Value)
        {
            Plugin.Logger.LogInfo("Customer took a card from the shelf! Filling all empty card sale shelves...");
            DoShelfPut();
        }
        
    }
    [HarmonyPatch(typeof(PriceChangeManager), "OnDayStarted")]
    [HarmonyPostfix]
    private static void OnOnDayStarted()
    {
        if (Plugin.m_ConfigShouldTriggerOnDayStart.Value)
        {
            Plugin.Logger.LogInfo("Day started. Filling shelves...");
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

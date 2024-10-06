using BepInEx.Logging;
using CardSeller;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CardSeller;

[HarmonyPatch]
public static class PatchIt
{

    [HarmonyPatch(typeof(InteractableCard3d), "RegisterScanCard")]
    [HarmonyPostfix]
    private static void OnRegisterScanCardPostFix(InteractableCard3d __instance)
    {
        Plugin.Logger.LogMessage("Card scanned!");
        //use this as a trigger to go find it's original slot and replace it?
        //or iterate through all empty sellable slots
        Plugin.Logger.LogMessage("Price is " + __instance.GetCurrentPrice());
    }

    [HarmonyPatch(typeof(CPlayerData), "ReduceCard")]
    [HarmonyPostfix]
    private static void OnReduceCardPostFix(CPlayerData __instance)
    {
        Plugin.Logger.LogMessage("Reduce card was called!");
        Plugin.Logger.LogMessage("OK");
    }

    [HarmonyPatch(typeof(Customer), "TakeCardFromShelf")]
    [HarmonyPostfix]
    private static void OnCustomerTakeCardFromShelfPostFix(Customer __instance, List<InteractableCard3d> ___m_CardInBagList)
    {
        Plugin.Logger.LogMessage("Customer took a card from the shelf! Filling all empty shelves...");
        Plugin.Logger.LogMessage(___m_CardInBagList.Count);
        //List<int> cardList = CPlayerData.GetCardCollectedList(ECardExpansionType.Tetramon, false);
        //foreach (int cardIndex in cardList)
        //{
        //    CardData cardData = CPlayerData.GetCardData(cardIndex, ECardExpansionType.Tetramon, false);
            
        //}

        List<CardShelf> cardShelfList = CSingleton<ShelfManager>.Instance.m_CardShelfList;
        foreach (CardShelf shelf in cardShelfList)
        {
            List<InteractableCardCompartment> cardCompartments = shelf.GetCardCompartmentList();
            foreach (InteractableCardCompartment cardCompart in cardCompartments)
            {
                if (cardCompart.m_StoredCardList.Count == 0)
                {
                    //instantiate card?
                    List<int> cardList = CPlayerData.GetCardCollectedList(ECardExpansionType.Tetramon, false);
                    if (cardList.Count > 0)
                    {
                        CardData cardData = CPlayerData.GetCardData(cardList[0], ECardExpansionType.Tetramon, false);
                        if (cardData != null && cardData.monsterType != EMonsterType.None)
                        {
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
                            CPlayerData.ReduceCard(cardData, 1);
                        }
                    }
                }
            }
        }
    }

}

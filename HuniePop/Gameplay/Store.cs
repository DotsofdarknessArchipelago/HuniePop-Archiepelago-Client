using HarmonyLib;
using HuniePopArchiepelagoClient.Archipelago;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HuniePopArchiepelagoClient.HuniePop.Gameplay
{
    [HarmonyPatch]
    public class Store
    {
        /// <summary>
        /// when buying an item in the shopif its an archipelago location sent it otherwise if its an item make sure it dosent get put back in the shop
        /// </summary>
        [HarmonyPatch(typeof(StoreCellApp), "OnStoreItemSlotPressed")]
        [HarmonyPrefix]
        public static bool storepurchase(StoreItemSlot storeItemSlot, StoreCellApp __instance, ref int ____currentStoreTab)
        {
            if (____currentStoreTab == 1)
            {
                if (storeItemSlot.itemDefinition.id > 42069500)
                {
                    ArchipelagoConsole.LogMessage($"PURCHASED ITEM:{storeItemSlot.itemDefinition.name}");
                    ArchipelagoConsole.LogMessage($"SENDING LOCATION {storeItemSlot.itemDefinition.id}");
                    Plugin.curse.sendLoc(storeItemSlot.itemDefinition.id);
                    return true;
                }
                long a = IDs.itemidtoarchid(storeItemSlot.itemDefinition.id);
                for (int i = 0; i < CursedArchipelagoClient.alist.list.Count; i++)
                {
                    if (CursedArchipelagoClient.alist.list[i].Id == a && CursedArchipelagoClient.alist.list[i].putinshop)
                    {
                        CursedArchipelagoClient.alist.list[i].putinshop = false;
                        return true;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// populate the store with stuff we want
        /// </summary>
        [HarmonyPatch(typeof(PlayerManager), "RollNewStoreList")]
        [HarmonyPrefix]
        public static bool storeoverite(StoreItemPlayerData[] storeList, ItemType itemType)
        {
            //populate the gift tab of the shop
            if (itemType == ItemType.GIFT)
            {
                //get all gift items that have been recieved by the client
                List<ItemDefinition> p = new List<ItemDefinition>();
                for (int i = 49; i < 121; i++)
                {
                    if (CursedArchipelagoClient.alist.hasitem(IDs.itemidtoarchid(i)))
                    {
                        p.Add(GameManager.Data.Items.Get(i));
                    }
                }

                //populate the store with random gift items or NULL items if none left
                ListUtils.Shuffle<ItemDefinition>(p);
                if (p.Count > 12)
                {
                    p.RemoveRange(12, p.Count - 12);
                }
                for (int l = 0; l < 12; l++)
                {
                    if (p.Count > l)
                    {
                        storeList[l].itemDefinition = p[l];
                        storeList[l].soldOut = false;
                    }
                    else
                    {
                        storeList[l].itemDefinition = null;
                        storeList[l].soldOut = true;
                    }
                }

                return false;
            }
            //populate the Unique Gift tab for the shop
            else if (itemType == ItemType.UNIQUE_GIFT)
            {

                List<ItemDefinition> p = new List<ItemDefinition>();

                //get all tossed unique gifts to put in the shop
                for (int i = 0; i < CursedArchipelagoClient.alist.list.Count; i++)
                {
                    if (CursedArchipelagoClient.alist.list[i].putinshop && CursedArchipelagoClient.alist.list[i].Id >= 42069097 && CursedArchipelagoClient.alist.list[i].Id <= 42069168)
                    {
                        p.Add(GameManager.Data.Items.Get(IDs.archidtoitemid(CursedArchipelagoClient.alist.list[i].Id)));
                    }
                }

                //get all archipelago shop locations to put in the store
                int shopslots = Convert.ToInt32(Plugin.curse.connected.slot_data["number_of_shop_items"]);
                int[] pre = [293, 294, 295, 296, 297, 298];
                for (int j = 0; j < shopslots; j++)
                {
                    if (Plugin.curse.connected.checked_locations != null)
                    {
                        if (Plugin.curse.connected.checked_locations.Contains(42069511 + j)) { continue; }
                    }
                    ItemDefinition item = new ItemDefinition();
                    item.type = ItemType.PRESENT;
                    item.hidden = false;
                    item.iconName = GameManager.Data.Items.Get(pre[UnityEngine.Random.Range(0, pre.Count() - 1)]).iconName;

                    item.name = "ARCH ITEM:" + (j + 1).ToString();
                    item.id = 42069511 + j;
                    item.description = "LOCATION CHECK FOR ARCHIPELAGO WILL BE REMOVED FROM INVENTORY WHEN MOVING LOCATIONS";
                    p.Add(item);
                }

                //randomise and populate the store slots with items
                ListUtils.Shuffle<ItemDefinition>(p);
                if (p.Count > 12)
                {
                    p.RemoveRange(12, p.Count - 12);
                }
                for (int l = 0; l < 12; l++)
                {
                    if (p.Count > l)
                    {
                        storeList[l].itemDefinition = p[l];
                        storeList[l].soldOut = false;
                    }
                    else
                    {
                        storeList[l].itemDefinition = null;
                        storeList[l].soldOut = true;
                    }
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// set store cost for items what we want them to be
        /// </summary>
        [HarmonyPatch(typeof(ItemDefinition), "storeCost", MethodType.Getter)]
        [HarmonyPrefix]
        public static bool itemcost(ref int __result, ItemDefinition __instance)
        {
            if (__instance.type == ItemType.PRESENT)
            {
                __result = Convert.ToInt32(Plugin.curse.connected.slot_data["shop_item_cost"]);
                return false;
            }
            else if (__instance.type == ItemType.GIFT)
            {
                __result = Convert.ToInt32(Plugin.curse.connected.slot_data["shop_gift_cost"]);
                return false;
            }
            else if (__instance.type == ItemType.UNIQUE_GIFT)
            {
                __result = 0;
                return false;
            }
            return true;
        }
    }
}

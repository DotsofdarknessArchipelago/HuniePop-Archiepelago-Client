using HarmonyLib;
using HuniePopArchiepelagoClient.Archipelago;
using System;
using System.ComponentModel;

namespace HuniePopArchiepelagoClient.HuniePop.Girls
{
    [HarmonyPatch]
    public class Gifts
    {
        /// <summary>
        /// send an archipelago location for gifting a gift item to a girl
        /// </summary>
        [HarmonyPatch(typeof(GirlPlayerData), "AddItemToCollection")]
        [HarmonyPostfix]
        public static void cgiftloc(ItemDefinition item, GirlPlayerData __instance, ref bool __result)
        {
            if (__result)
            {
                Plugin.curse.sendLoc(42069061 + ((__instance.GetGirlDefinition().id - 1) * 24) + (__instance.GetGirlDefinition().collection.IndexOf(item)));
            }
        }

        /// <summary>
        /// send archipelago location when gifting panties to kyu and send completion if all panties have been turned in
        /// </summary>
        [HarmonyPatch(typeof(GirlManager), "GiveItem")]
        [HarmonyPostfix]
        public static void releaseallitems(ItemDefinition item, GirlManager __instance, ref bool __result)
        {
            if (__result && item.type == ItemType.PANTIES)
            {
                Plugin.curse.sendLoc(item.id - 277 + 42069493);
            }

            if (GameManager.System.Player.pantiesTurnedIn.Count >= 12) { Plugin.curse.sendCompletion(); }
        }

        /// <summary>
        /// allow for buying gits in the gift collection for girls if allowed by archipelago logic
        /// </summary>
        [HarmonyPatch(typeof(GirlProfileCellApp), "OnCollectionSlotPressed")]
        [HarmonyPrefix]
        public static bool collectionoverite(GirlProfileCollectionSlot collectionSlot, ref GirlProfileCellApp __instance)
        {
            int hunie = Convert.ToInt32(Plugin.curse.connected.slot_data["hunie_gift_cost"]);

            //only allow buying if not in date, have inventory slots, has enougth hunie and if the item has already been recieved by the client
            if (collectionSlot.itemDefinition.type == ItemType.GIFT &&
                GameManager.System.GameState == GameState.SIM &&
                !GameManager.System.Player.IsInventoryFull(GameManager.System.Player.inventory) &&
                GameManager.System.Player.hunie >= hunie &&
                CursedArchipelagoClient.alist.hasitem(IDs.itemidtoarchid(collectionSlot.itemDefinition.id)))
            {
                GameManager.System.Player.AddItem(collectionSlot.itemDefinition, GameManager.System.Player.inventory, true, false);
                GameManager.System.Player.hunie -= hunie;
                GameManager.System.Audio.Play(AudioCategory.SOUND, __instance.orderItemSound, false, 1f, true);
                GameManager.Stage.cellNotifications.Notify(CellNotificationType.INVENTORY, GirlManager.FAIRY_PRESENT_NOTIFICATIONS[global::UnityEngine.Random.Range(0, GirlManager.FAIRY_PRESENT_NOTIFICATIONS.Length)]);
                __instance.Refresh();
                GameManager.Stage.tooltip.Refresh();
            }
            return false;
        }

        /// <summary>
        /// enable or disable the button ability for buying a gift through the girls collection
        /// </summary>
        [HarmonyPatch(typeof(GirlProfileCollectionSlot), "Refresh")]
        [HarmonyPrefix]
        public static bool collectionoverite2(GirlProfileCollectionSlot __instance)
        {
            //only enable button if not in date, have inventory slots, has enougth hunie and if the item has already been recieved by the client
            if (__instance.itemDefinition != null &&
                __instance.itemDefinition.type == ItemType.GIFT &&
                GameManager.System.GameState == GameState.SIM &&
                GameManager.System.Player.endingSceneShown &&
                !GameManager.System.Player.IsInventoryFull(GameManager.System.Player.inventory) &&
                GameManager.System.Player.hunie >= Convert.ToInt32(Plugin.curse.connected.slot_data["hunie_gift_cost"]) &&
                CursedArchipelagoClient.alist.hasitem(IDs.itemidtoarchid(__instance.itemDefinition.id)))
            {
                __instance.button.Enable();
            }
            else
            {
                __instance.button.Disable();
            }
            return false;
        }
    }
}

using HarmonyLib;
using HuniePopArchiepelagoClient.Archipelago;
using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

namespace HuniePopArchiepelagoClient.HuniePop.Gameplay
{
    [HarmonyPatch]
    public class DepartLocation
    {

        /// <summary>
        /// process archipelago item list and check for game completion
        /// </summary>
        [HarmonyPatch(typeof(LocationManager), "DepartLocation")]
        [HarmonyPrefix]
        public static void archcheck(LocationManager __instance, ref LocationDefinition ____destinationLocation)
        {
            //dont do anything if going to a date location
            if (____destinationLocation.type == LocationType.DATE) { return; }

            PlayerManager player = GameManager.System.Player;

            //process archipelago list items
            processarch();

            //remove any shop location item that is in the inventory
            for (int i = 0; i < player.inventory.Length; i++)
            {
                if (player.inventory[i].itemDefinition == null) { continue; }
                if (player.inventory[i].itemDefinition.name.StartsWith("ARCH ITEM:"))
                {
                    player.inventory[i].itemDefinition = null;
                    player.inventory[i].presentDefinition = null;
                }
            }
            for (int j = 0; j < player.gifts.Length; j++)
            {
                if (player.gifts[j].itemDefinition == null) { continue; }
                if (player.gifts[j].itemDefinition.name.StartsWith("ARCH ITEM:"))
                {
                    player.gifts[j].itemDefinition = null;
                    player.gifts[j].presentDefinition = null;
                }
            }
            for (int j = 0; j < player.dateGifts.Length; j++)
            {
                if (player.dateGifts[j].itemDefinition == null) { continue; }
                if (player.dateGifts[j].itemDefinition.name.StartsWith("ARCH ITEM:"))
                {
                    player.dateGifts[j].itemDefinition = null;
                    player.dateGifts[j].presentDefinition = null;
                }
            }

            //save archdata
            using (StreamWriter archfile = File.CreateText(Application.persistentDataPath + "/archdata"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(archfile, CursedArchipelagoClient.alist);
            }

            //make sure we are not comming back from a date and roll everything for a new day
            if (__instance.currentLocation.type != LocationType.DATE)
            {
                GameManager.System.Player.RollNewDay();
            }

            //check for if goal is completed
            if (Convert.ToBoolean(Plugin.curse.connected.slot_data["goal"]))
            {
                //if goal is give kyu all panties
                if (player.alphaModeActive)
                {
                    Plugin.curse.sendCompletion();
                }
            }
            else
            {
                //if goal is to have sex with all girls
                bool goalreached = true;

                if (Convert.ToBoolean(Plugin.curse.connected.slot_data["tiffany_enabled"])) { if (!player.girls[0].gotPanties) { goalreached = false; } }
                if (Convert.ToBoolean(Plugin.curse.connected.slot_data["aiko_enabled"])) { if (!player.girls[1].gotPanties) { goalreached = false; } }
                if (Convert.ToBoolean(Plugin.curse.connected.slot_data["kyanna_enabled"])) { if (!player.girls[2].gotPanties) { goalreached = false; } }
                if (Convert.ToBoolean(Plugin.curse.connected.slot_data["audrey_enabled"])) { if (!player.girls[3].gotPanties) { goalreached = false; } }
                if (Convert.ToBoolean(Plugin.curse.connected.slot_data["lola_enabled"])) { if (!player.girls[4].gotPanties) { goalreached = false; } }
                if (Convert.ToBoolean(Plugin.curse.connected.slot_data["nikki_enabled"])) { if (!player.girls[5].gotPanties) { goalreached = false; } }
                if (Convert.ToBoolean(Plugin.curse.connected.slot_data["jessie_enabled"])) { if (!player.girls[6].gotPanties) { goalreached = false; } }
                if (Convert.ToBoolean(Plugin.curse.connected.slot_data["beli_enabled"])) { if (!player.girls[7].gotPanties) { goalreached = false; } }
                if (Convert.ToBoolean(Plugin.curse.connected.slot_data["kyu_enabled"])) { if (!player.girls[8].gotPanties) { goalreached = false; } }
                if (Convert.ToBoolean(Plugin.curse.connected.slot_data["momo_enabled"])) { if (!player.girls[9].gotPanties) { goalreached = false; } }
                if (Convert.ToBoolean(Plugin.curse.connected.slot_data["celeste_enabled"])) { if (!player.girls[10].gotPanties) { goalreached = false; } }
                if (Convert.ToBoolean(Plugin.curse.connected.slot_data["venus_enabled"])) { if (!player.girls[11].gotPanties) { goalreached = false; } }

                if (goalreached) { Plugin.curse.sendCompletion(); }
            }

        }

        /// <summary>
        /// process archipelago item list
        /// </summary>
        public static void processarch()
        {

            PlayerManager player = GameManager.System.Player;
            ArchipelagoConsole.LogMessage("processing items");


            for (int i = 0; i < CursedArchipelagoClient.alist.list.Count; i++)
            {
                ArchipelagoItem item = CursedArchipelagoClient.alist.list[i];

                //skip item if it has been processed
                if (item.processed) { continue; }

                //if item id is between 42069000 and 42069013 process it as a Panties item
                if (item.Id > 42069000 && item.Id < 42069013)
                {
                    //skip item and flag it as processed if player has the panties in inventory or has already turned it in
                    if (player.pantiesTurnedIn.Contains(IDs.archidtoitemid(item.Id)))
                    {
                        ArchipelagoConsole.LogMessage(item.itemname + " already turned in skipping");
                        item.processed = true;
                        continue;
                    }
                    if (player.HasItem(GameManager.Data.Items.Get(IDs.archidtoitemid(item.Id))))
                    {
                        ArchipelagoConsole.LogMessage(item.itemname + " already in inventory skipping");
                        item.processed = true;
                        continue;
                    }

                    //only process panties if player has inventory for it
                    if (!player.IsInventoryFull())
                    {
                        ArchipelagoConsole.LogMessage(item.itemname + " recieved");
                        player.AddItem(GameManager.Data.Items.Get(IDs.archidtoitemid(item.Id)), player.inventory, false, false);
                        item.processed = true;
                    }
                }
                //if item id is between 42069012 and 42069025 process it as a Girl Unlock
                else if (item.Id > 42069012 && item.Id < 42069025)
                {
                    //find girl and set metStatus as GirlMetStatus.MET
                    int girlid = (int)item.Id - 42069012;
                    for (int j = 0; j < player.girls.Count; j++)
                    {
                        if (player.girls[j].GetGirlDefinition().id == girlid)
                        {
                            if (player.girls[j].metStatus == GirlMetStatus.MET)
                            {
                                ArchipelagoConsole.LogMessage(player.girls[j].GetGirlDefinition().name + " already unlocked skipping");
                                item.processed = true;
                                break;
                            }
                            ArchipelagoConsole.LogMessage(player.girls[j].GetGirlDefinition().name + " unlocked");
                            player.girls[j].metStatus = GirlMetStatus.MET;
                            item.processed = true;
                            break;
                        }
                    }
                }
                //if item id is between 42069024 and 42069097 process it as a Gift Item
                else if (item.Id > 42069024 && item.Id < 42069097)
                {
                    //Make sure players inventory isnt full and give the relevent item
                    if (!player.IsInventoryFull())
                    {
                        player.AddItem(GameManager.Data.Items.Get(IDs.archidtoitemid(item.Id)), player.inventory, false, false);
                        ArchipelagoConsole.LogMessage(GameManager.Data.Items.Get(IDs.archidtoitemid(item.Id)).name + " recieved");
                        ArchipelagoConsole.LogMessage(GameManager.Data.Items.Get(IDs.archidtoitemid(item.Id)).name + " can now be bought in the shop");
                        item.processed = true;
                    }
                }
                //if item id is between 42069096 and 42069169 process it as a Unique Gift Item
                else if (item.Id > 42069096 && item.Id < 42069169)
                {
                    //Make sure players inventory isnt full and give the relevent item
                    if (!player.IsInventoryFull())
                    {
                        player.AddItem(GameManager.Data.Items.Get(IDs.archidtoitemid(item.Id)), player.inventory, false, false);
                        ArchipelagoConsole.LogMessage(GameManager.Data.Items.Get(IDs.archidtoitemid(item.Id)).name + " recieved");
                        item.processed = true;
                    }
                }
                //if item id is between 42069168 and 42069177 process it as a Trait Upgrade Item
                else if (item.Id > 42069168 && item.Id < 42069177)
                {
                    ArchipelagoConsole.LogMessage($"{item.itemname} recieved");
                    if (item.Id == 42069169)
                    {
                        //TALENT
                        player.UpgradeTraitLevel(PlayerTraitType.TALENT);
                        item.processed = true;
                    }
                    else if (item.Id == 42069170)
                    {
                        //FLIRTATION
                        player.UpgradeTraitLevel(PlayerTraitType.FLIRTATION);
                        item.processed = true;
                    }
                    else if (item.Id == 42069171)
                    {
                        //ROMANCE
                        player.UpgradeTraitLevel(PlayerTraitType.ROMANCE);
                        item.processed = true;
                    }
                    else if (item.Id == 42069172)
                    {
                        //SEXUALITY
                        player.UpgradeTraitLevel(PlayerTraitType.SEXUALITY);
                        item.processed = true;
                    }
                    else if (item.Id == 42069173)
                    {
                        //PASSION
                        player.UpgradeTraitLevel(PlayerTraitType.PASSION);
                        item.processed = true;
                    }
                    else if (item.Id == 42069174)
                    {
                        //SENSITIVITY
                        player.UpgradeTraitLevel(PlayerTraitType.SENSITIVITY);
                        item.processed = true;
                    }
                    else if (item.Id == 42069175)
                    {
                        //CHRISMA
                        player.UpgradeTraitLevel(PlayerTraitType.CHARISMA);
                        item.processed = true;
                    }
                    else
                    {
                        //LUCK
                        player.UpgradeTraitLevel(PlayerTraitType.LUCK);
                        item.processed = true;
                    }

                }
                //if item id is between 42069176 and 42069309 process it as a Filler Item
                else if (item.Id > 42069176 && item.Id < 42069309)
                {
                    //make sure that the players inventory isnt full then give the relevent item
                    if (!player.IsInventoryFull())
                    {
                        player.AddItem(GameManager.Data.Items.Get(IDs.archidtoitemid(item.Id)), player.inventory, false, false);
                        ArchipelagoConsole.LogMessage(GameManager.Data.Items.Get(IDs.archidtoitemid(item.Id)).name + " recieved");
                        item.processed = true;
                    }
                }
            }
        }
    }
}

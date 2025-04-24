using HarmonyLib;
using HuniePopArchiepelagoClient.Archipelago;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace HuniePopArchiepelagoClient.HuniePop.Gameplay
{
    [HarmonyPatch]
    public class Game
    {
        /// <summary>
        /// setup game for starting as female
        /// </summary>
        [HarmonyPatch(typeof(LoadScreenSaveFile), "OnStartFemaleButtonPressed")]
        [HarmonyPrefix]
        public static bool newgirloveride(ref int ____saveFileIndex)
        {
            //make sure we are connected to an archipelago server
            if (!Plugin.tringtoconnect)
            {
                ArchipelagoConsole.LogImportant("Please connect to a server in top right to start a game");
                return false;
            }
            if (!Plugin.curse.fullconnection && Plugin.tringtoconnect)
            {
                ArchipelagoConsole.LogImportant("There was an error setting up the connection to the server please restart the game and try again");
                return false;
            }

            //make sure that we are using the bottom left slot
            if (____saveFileIndex != 3)
            {
                return false;
            }
            SaveFile saveFile = SaveUtils.GetSaveFile(____saveFileIndex);

            if (Plugin.curse.fullconnection && !saveFile.started)
            {
                //set some default values
                saveFile.started = true;
                saveFile.tutorialComplete = true;
                saveFile.cellphoneUnlocked = true;
                saveFile.endingSceneShown = true;
                saveFile.settingsGender = 1;
                saveFile.tutorialStep = 10;

                //if a girl is disabled in logic add their panties to panties turned in to allow for completion goal
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["tiffany_enabled"])) { saveFile.pantiesTurnedIn.Add(277); }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["aiko_enabled"])) { saveFile.pantiesTurnedIn.Add(278); }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["kyanna_enabled"])) { saveFile.pantiesTurnedIn.Add(279); }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["audrey_enabled"])) { saveFile.pantiesTurnedIn.Add(280); }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["lola_enabled"])) { saveFile.pantiesTurnedIn.Add(281); }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["nikki_enabled"])) { saveFile.pantiesTurnedIn.Add(282); }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["jessie_enabled"])) { saveFile.pantiesTurnedIn.Add(283); }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["beli_enabled"])) { saveFile.pantiesTurnedIn.Add(284); }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["kyu_enabled"])) { saveFile.pantiesTurnedIn.Add(285); }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["momo_enabled"])) { saveFile.pantiesTurnedIn.Add(286); }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["celeste_enabled"])) { saveFile.pantiesTurnedIn.Add(287); }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["venus_enabled"])) { saveFile.pantiesTurnedIn.Add(288); }

                //set starting girl and starting location to be bedroom
                saveFile.currentGirl = Convert.ToInt32(Plugin.curse.connected.slot_data["start_girl"]);
                saveFile.currentLocation = 22;

                //process archipelago item list for any unlocked girls and set them as been met
                for (int i = 0; i < CursedArchipelagoClient.alist.list.Count; i++)
                {
                    if (CursedArchipelagoClient.alist.list[i].Id > 42069012 && CursedArchipelagoClient.alist.list[i].Id < 42069025)
                    {
                        saveFile.girls[(int)CursedArchipelagoClient.alist.list[i].Id - 42069012].metStatus = 3;
                        CursedArchipelagoClient.alist.list[i].processed = true;
                    }
                }

                //throw an error if there is no item in the archipelago item list or starting girl setting being null
                if (CursedArchipelagoClient.alist.list.Count == 0)
                {
                    ArchipelagoConsole.LogMessage("ERROR IN SETTING UP SAVE DATA(no items recieved) PLEASE RESTART YOUR GAME");
                    return false;
                }
                if (saveFile.girls[saveFile.currentGirl].metStatus != 3)
                {
                    ArchipelagoConsole.LogMessage("ERROR IN SETTING UP SAVE DATA(starting girl not unlocked properly) PLEASE RESTART YOUR GAME");
                    return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// setup game for starting as male
        /// </summary>
        [HarmonyPatch(typeof(LoadScreenSaveFile), "OnStartMaleButtonPressed")]
        [HarmonyPrefix]
        public static bool newguyoveride(ref int ____saveFileIndex)
        {
            //make sure we are connected to an archipelago server
            if (!Plugin.tringtoconnect)
            {
                ArchipelagoConsole.LogImportant("Please connect to a server in top right to start a game");
                return false;
            }
            if (!Plugin.curse.fullconnection && Plugin.tringtoconnect)
            {
                ArchipelagoConsole.LogImportant("There was an error setting up the connection to the server please restart the game and try again");
                return false;
            }

            //make sure that we are using the bottom left slot
            if (____saveFileIndex != 3)
            {
                return false;
            }
            SaveFile saveFile = SaveUtils.GetSaveFile(____saveFileIndex);

            if (Plugin.curse.fullconnection && !saveFile.started)
            {
                //set some default values
                saveFile.started = true;
                saveFile.tutorialComplete = true;
                saveFile.cellphoneUnlocked = true;
                saveFile.endingSceneShown = true;
                saveFile.settingsGender = 0;
                saveFile.tutorialStep = 10;

                //if a girl is disabled in logic add their panties to panties turned in to allow for completion goal
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["tiffany_enabled"])) { saveFile.pantiesTurnedIn.Add(277); }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["aiko_enabled"])) { saveFile.pantiesTurnedIn.Add(278); }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["kyanna_enabled"])) { saveFile.pantiesTurnedIn.Add(279); }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["audrey_enabled"])) { saveFile.pantiesTurnedIn.Add(280); }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["lola_enabled"])) { saveFile.pantiesTurnedIn.Add(281); }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["nikki_enabled"])) { saveFile.pantiesTurnedIn.Add(282); }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["jessie_enabled"])) { saveFile.pantiesTurnedIn.Add(283); }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["beli_enabled"])) { saveFile.pantiesTurnedIn.Add(284); }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["kyu_enabled"])) { saveFile.pantiesTurnedIn.Add(285); }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["momo_enabled"])) { saveFile.pantiesTurnedIn.Add(286); }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["celeste_enabled"])) { saveFile.pantiesTurnedIn.Add(287); }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["venus_enabled"])) { saveFile.pantiesTurnedIn.Add(288); }

                //set starting girl and starting location to be bedroom
                saveFile.currentGirl = Convert.ToInt32(Plugin.curse.connected.slot_data["start_girl"]);
                saveFile.currentLocation = 22;

                //process archipelago item list for any unlocked girls and set them as been met
                for (int i = 0; i < CursedArchipelagoClient.alist.list.Count; i++)
                {
                    if (CursedArchipelagoClient.alist.list[i].Id > 42069012 && CursedArchipelagoClient.alist.list[i].Id < 42069025)
                    {
                        saveFile.girls[(int)CursedArchipelagoClient.alist.list[i].Id - 42069012].metStatus = 3;
                        CursedArchipelagoClient.alist.list[i].processed = true;
                    }
                }

                //throw an error if there is no item in the archipelago item list or starting girl setting being null
                if (CursedArchipelagoClient.alist.list.Count == 0)
                {
                    ArchipelagoConsole.LogMessage("ERROR IN SETTING UP SAVE DATA(no items recieved) PLEASE RESTART YOUR GAME");
                    return false;
                }
                if (saveFile.girls[saveFile.currentGirl].metStatus != 3)
                {
                    ArchipelagoConsole.LogMessage("ERROR IN SETTING UP SAVE DATA(starting girl not unlocked properly) PLEASE RESTART YOUR GAME");
                    return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// checks for continuing a game
        /// </summary>
        [HarmonyPatch(typeof(LoadScreenSaveFile), "OnContinueButtonPressed")]
        [HarmonyPrefix]
        public static bool continueoveride(ref int ____saveFileIndex)
        {
            //make sure we are connected to a server
            if (!Plugin.curse.fullconnection && Plugin.tringtoconnect)
            {
                ArchipelagoConsole.LogImportant("There was an error setting up the connection to the server please restart the game and try again");
                return false;
            }

            //only continue a game in the bottom left slot
            if (____saveFileIndex != 3) { return false; }

            //check if we have items that need to be processed and inform player
            bool n = false;
            foreach (ArchipelagoItem i in CursedArchipelagoClient.alist.list)
            {
                if (!i.processed) { n = true; break; }
            }
            if (n) { ArchipelagoConsole.LogMessage("Items need to be processed please move locations to process them"); }
            return true;
        }

        /// <summary>
        /// dont know will look into later probally unneeded dupe from monlithic patch file
        /// </summary>
        [HarmonyPatch(typeof(LoadScreenSaveFile), "OnContinueButtonPressed")]
        [HarmonyPrefix]
        public static bool finderoveride(ref int ____saveFileIndex)
        {
            //if (____saveFileIndex != 3) { return false; }
            return true;
        }

        /// <summary>
        /// make sure that when reseting a file that the inventory is clear of items and starting location/girl is -1
        /// </summary>
        [HarmonyPatch(typeof(SaveFile), "ResetFile")]
        [HarmonyPostfix]
        public static void savereset(SaveFile __instance)
        {
            __instance.currentGirl = -1;
            __instance.currentLocation = -1;

            __instance.inventory = new InventoryItemSaveData[30];
            for (int j = 0; j < __instance.inventory.Length; j++)
            {
                __instance.inventory[j] = new InventoryItemSaveData();
            }
        }

        /// <summary>
        /// make it so every time the same is saved that we save at archdata as well
        /// </summary>
        [HarmonyPatch(typeof(GameManager), "SaveGame")]
        [HarmonyPostfix]
        public static void saveflags()
        {
            using (StreamWriter file = File.CreateText(Application.persistentDataPath + "/archdata"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, CursedArchipelagoClient.alist);
            }
        }

        /// <summary>
        /// dont know really but its there and dosent break anything
        /// </summary>
        [HarmonyPatch(typeof(GameManager), "LoadGame")]
        [HarmonyILManipulator]
        public static void loadgamereset(ILContext ctx, MethodBase orig)
        {
            for (int i = 0; i < ctx.Instrs.Count; i++)
            {
                if (ctx.Instrs[i].OpCode == OpCodes.Brtrue) { ctx.Instrs[i].OpCode = OpCodes.Brfalse; break; }
            }
        }
    }
}

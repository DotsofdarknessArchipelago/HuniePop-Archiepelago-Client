using System;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using HuniePopArchiepelagoClient.Archipelago;
using MonoMod.Cil;
using Newtonsoft.Json;
using Mono.Cecil.Cil;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;


namespace HuniePopArchiepelagoClient.Utils
{
    internal class Patches
    {
        public static CursedArchipelagoClient arch;
        public static void patch(CursedArchipelagoClient a)
        {
            arch = a;
            Harmony.CreateAndPatchAll(typeof(Patches));

        }


        /// <summary>
        /// DEBUG PATCH TO MAKE PUZZLES COMPLETE IN 1 MOVE
        /// </summary>
        [HarmonyPatch(typeof(PuzzleGame), "AddResourceValue")]
        [HarmonyPrefix]
        public static void puzzleautocomplete(PuzzleGame __instance)
        {
            //__instance.SetResourceValue(PuzzleGameResourceType.AFFECTION, 9999, true);
        }


        /// <summary>
        /// PROCESS THE ITEMS IN THE ARCH QUEUE AND SAVE THEM TO FLAGS
        /// </summary>
        [HarmonyPatch(typeof(LocationManager), "DepartLocation")]
        [HarmonyPrefix]
        public static void archcheck(LocationManager __instance, ref LocationDefinition ____destinationLocation)
        {
            if (CursedArchipelagoClient.alist.needtoreset) 
            {
                CursedArchipelagoClient.alist = new ArchipelageItemList();
                Plugin.curse.sendJson("{{\"cmd\":\"Sync\"}");
                ArchipelagoConsole.LogMessage("RESETTING ITEM LIST");
                Thread.Sleep(5000);
            }

            PlayerManager player = GameManager.System.Player;

            Util.processarch();


            using (StreamWriter archfile = File.CreateText(Application.persistentDataPath + "/archdata"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(archfile, CursedArchipelagoClient.alist);
            }

            if (player.alphaModeActive)
            {
                arch.sendCompletion();
            }

            GameManager.System.Player.RollNewDay();

        }

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


        [HarmonyPatch(typeof(PuzzleManager), "OnPuzzleGameComplete")]
        [HarmonyPrefix]
        public static void datefin(PuzzleManager __instance, ref PuzzleGame ____activePuzzleGame)
        {
            GirlPlayerData girlData = GameManager.System.Player.GetGirlData(GameManager.System.Location.currentGirl);
            if (____activePuzzleGame.isVictorious)
            {
                if (girlData.relationshipLevel == 1)
                {
                    arch.sendLoc(42069013 + ((girlData.GetGirlDefinition().id - 1) * 4));
                }
                else if (girlData.relationshipLevel == 2)
                {
                    arch.sendLoc(42069014 + ((girlData.GetGirlDefinition().id - 1) * 4));
                }
                else if (girlData.relationshipLevel == 3)
                {
                    arch.sendLoc(42069015 + ((girlData.GetGirlDefinition().id - 1) * 4));
                }
                else if (girlData.relationshipLevel == 4)
                {
                    arch.sendLoc(42069016 + ((girlData.GetGirlDefinition().id - 1) * 4));
                }
            }
            if (____activePuzzleGame.isBonusRound && !girlData.gotPanties)
            {
                arch.sendLoc(42069001 + (girlData.GetGirlDefinition().id - 1));
            }
        }

        [HarmonyPatch(typeof(GirlPlayerData), "AddItemToCollection")]
        [HarmonyPostfix]
        public static void cgiftloc(ItemDefinition item, GirlPlayerData __instance, ref bool __result)
        {
            if (__result)
            {
                arch.sendLoc(42069061 + ((__instance.GetGirlDefinition().id - 1) * 24) + (__instance.GetGirlDefinition().collection.IndexOf(item)));
            }
        }

        [HarmonyPatch(typeof(StoreCellApp), "OnStoreItemSlotPressed")]
        [HarmonyPrefix]
        public static bool storepurchase(StoreItemSlot storeItemSlot, StoreCellApp __instance, ref int ____currentStoreTab)
        {
            if (____currentStoreTab == 0)
            {
                long a = Util.itemidtoarchid(storeItemSlot.itemDefinition.id);
                for (int i = 0; i < CursedArchipelagoClient.alist.list.Count; i++)
                {
                    if (CursedArchipelagoClient.alist.list[i].Id == a && CursedArchipelagoClient.alist.list[i].putinshop)
                    {
                        ArchipelagoConsole.LogMessage("PURCHASED ITEM: " + storeItemSlot.itemDefinition.name);
                        CursedArchipelagoClient.alist.list[i].putinshop = false;
                        return true;
                    }
                }
            }
            return true;
        }


        [HarmonyPatch(typeof(LoadScreenSaveFile), "Refresh")]
        [HarmonyPostfix]
        public static void savetextoveride(LoadScreenSaveFile __instance, ref int ____saveFileIndex)
        {
            if (____saveFileIndex == 3)
            {
                __instance.titleLabel.SetText("ARCHIPELAGO FILE");
            }
            else
            {
                __instance.titleLabel.SetText("DISABLED");
            }
        }

        [HarmonyPatch(typeof(LoadScreenSaveFile), "OnStartFemaleButtonPressed")]
        [HarmonyPrefix]
        public static bool newgirloveride(ref int ____saveFileIndex)
        {
            if (!Plugin.curse.fullconnect)
            {
                helper.MessageBox(IntPtr.Zero, "There was an error setting up the connection to the server please restart the game and try again", "WEBSOCKET SETUP ERROR", 0);
                return false; 
            }

            if (____saveFileIndex != 3)
            {
                return false;
            }
            SaveFile saveFile = SaveUtils.GetSaveFile(____saveFileIndex);

            if (Plugin.curse.Authenticated && !saveFile.started)
            {

                saveFile.started = true;
                saveFile.tutorialComplete = true;
                saveFile.cellphoneUnlocked = true;
                saveFile.endingSceneShown = true;

                saveFile.tutorialStep = 10;

                if (!Convert.ToBoolean(Plugin.curse.connected.SlotData["tiffany_enabled"])) { saveFile.pantiesTurnedIn.Add(277); }
                if (!Convert.ToBoolean(Plugin.curse.connected.SlotData["aiko_enabled"])) { saveFile.pantiesTurnedIn.Add(278); }
                if (!Convert.ToBoolean(Plugin.curse.connected.SlotData["kyanna_enabled"])) { saveFile.pantiesTurnedIn.Add(279); }
                if (!Convert.ToBoolean(Plugin.curse.connected.SlotData["audrey_enabled"])) { saveFile.pantiesTurnedIn.Add(280); }
                if (!Convert.ToBoolean(Plugin.curse.connected.SlotData["lola_enabled"])) { saveFile.pantiesTurnedIn.Add(281); }
                if (!Convert.ToBoolean(Plugin.curse.connected.SlotData["nikki_enabled"])) { saveFile.pantiesTurnedIn.Add(282); }
                if (!Convert.ToBoolean(Plugin.curse.connected.SlotData["jessie_enabled"])) { saveFile.pantiesTurnedIn.Add(283); }
                if (!Convert.ToBoolean(Plugin.curse.connected.SlotData["beli_enabled"])) { saveFile.pantiesTurnedIn.Add(284); }
                if (!Convert.ToBoolean(Plugin.curse.connected.SlotData["kyu_enabled"])) { saveFile.pantiesTurnedIn.Add(285); }
                if (!Convert.ToBoolean(Plugin.curse.connected.SlotData["momo_enabled"])) { saveFile.pantiesTurnedIn.Add(286); }
                if (!Convert.ToBoolean(Plugin.curse.connected.SlotData["celeste_enabled"])) { saveFile.pantiesTurnedIn.Add(287); }
                if (!Convert.ToBoolean(Plugin.curse.connected.SlotData["venus_enabled"])) { saveFile.pantiesTurnedIn.Add(288); }
                
                saveFile.currentGirl = Convert.ToInt32(Plugin.curse.connected.SlotData["start_girl"]);
                saveFile.currentLocation = 22;

                for (int i = 0; i < CursedArchipelagoClient.alist.list.Count; i++)
                {
                    if (CursedArchipelagoClient.alist.list[i].Id > 42069012 && CursedArchipelagoClient.alist.list[i].Id < 42069025)
                    {
                        saveFile.girls[(int)CursedArchipelagoClient.alist.list[i].Id - 42069012].metStatus = 3;
                    }
                }

                return true;

            }
            return false;
        }

        [HarmonyPatch(typeof(LoadScreenSaveFile), "OnStartMaleButtonPressed")]
        [HarmonyPrefix]
        public static bool newguyoveride(ref int ____saveFileIndex)
        {

            if (!Plugin.curse.fullconnect)
            {
                helper.MessageBox(IntPtr.Zero, "There was an error setting up the connection to the server please restart the game and try again", "WEBSOCKET SETUP ERROR", 0);
                return false;
            }

            if (____saveFileIndex != 3)
            {
                return false;
            }
            SaveFile saveFile = SaveUtils.GetSaveFile(____saveFileIndex);

            if (Plugin.curse.Authenticated && !saveFile.started)
            {

                saveFile.started = true;
                saveFile.tutorialComplete = true;
                saveFile.cellphoneUnlocked = true;
                saveFile.endingSceneShown = true;

                saveFile.tutorialStep = 10;

                if (!Convert.ToBoolean(Plugin.curse.connected.SlotData["tiffany_enabled"])) { saveFile.pantiesTurnedIn.Add(277); }
                if (!Convert.ToBoolean(Plugin.curse.connected.SlotData["aiko_enabled"])) { saveFile.pantiesTurnedIn.Add(278); }
                if (!Convert.ToBoolean(Plugin.curse.connected.SlotData["kyanna_enabled"])) { saveFile.pantiesTurnedIn.Add(279); }
                if (!Convert.ToBoolean(Plugin.curse.connected.SlotData["audrey_enabled"])) { saveFile.pantiesTurnedIn.Add(280); }
                if (!Convert.ToBoolean(Plugin.curse.connected.SlotData["lola_enabled"])) { saveFile.pantiesTurnedIn.Add(281); }
                if (!Convert.ToBoolean(Plugin.curse.connected.SlotData["nikki_enabled"])) { saveFile.pantiesTurnedIn.Add(282); }
                if (!Convert.ToBoolean(Plugin.curse.connected.SlotData["jessie_enabled"])) { saveFile.pantiesTurnedIn.Add(283); }
                if (!Convert.ToBoolean(Plugin.curse.connected.SlotData["beli_enabled"])) { saveFile.pantiesTurnedIn.Add(284); }
                if (!Convert.ToBoolean(Plugin.curse.connected.SlotData["kyu_enabled"])) { saveFile.pantiesTurnedIn.Add(285); }
                if (!Convert.ToBoolean(Plugin.curse.connected.SlotData["momo_enabled"])) { saveFile.pantiesTurnedIn.Add(286); }
                if (!Convert.ToBoolean(Plugin.curse.connected.SlotData["celeste_enabled"])) { saveFile.pantiesTurnedIn.Add(287); }
                if (!Convert.ToBoolean(Plugin.curse.connected.SlotData["venus_enabled"])) { saveFile.pantiesTurnedIn.Add(288); }

                saveFile.currentGirl = Convert.ToInt32(Plugin.curse.connected.SlotData["start_girl"]);
                saveFile.currentLocation = 22;

                for (int i = 0; i < CursedArchipelagoClient.alist.list.Count; i++)
                {
                    if (CursedArchipelagoClient.alist.list[i].Id > 42069012 && CursedArchipelagoClient.alist.list[i].Id < 42069025)
                    {
                        saveFile.girls[(int)CursedArchipelagoClient.alist.list[i].Id - 42069012].metStatus = 3;
                    }
                }

                return true;

            }
            return false;

        }

        [HarmonyPatch(typeof(LoadScreenSaveFile), "OnContinueButtonPressed")]
        [HarmonyPrefix]
        public static bool continueoveride(ref int ____saveFileIndex)
        {

            if (!Plugin.curse.fullconnect)
            {
                helper.MessageBox(IntPtr.Zero, "There was an error setting up the connection to the server please restart the game and try again", "WEBSOCKET SETUP ERROR", 0);
                return false;
            }

            if (____saveFileIndex != 3) { return false; }
            return true;
        }

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

            //__instance.started = true;
            //__instance.tutorialComplete = true;
            //__instance.tutorialStep = 10;
            //__instance.cellphoneUnlocked = true;
            //__instance.currentGirl = 9;
            //__instance.currentLocation = 22;
            //__instance.girls[9].metStatus = 3;
            //__instance.girls[8].metStatus = 3;
        }


        [HarmonyPatch(typeof(LoadScreenSaveFile), "OnContinueButtonPressed")]
        [HarmonyPrefix]
        public static bool finderoveride(ref int ____saveFileIndex)
        {
            //if (____saveFileIndex != 3) { return false; }
            return true;
        }

        //[HarmonyPatch(typeof(GirlFinderIcon), "Init")]
        //[HarmonyPostfix]
        //public static void finderoverite(GirlFinderIcon __instance, ref LocationDefinition ____girlLocation)
        //{
        //    int[] l = [2, 3, 4, 5, 7, 8, 9, 11, 22];
        //    ____girlLocation = GameManager.Data.Locations.Get(l[UnityEngine.Random.Range(0, l.Count() - 1)]);
        //}

        [HarmonyPatch(typeof(GirlDefinition), "IsAtLocationAtTime")]
        [HarmonyPrefix]
        public static bool sceduleoverite(ref LocationDefinition __result)
        {
            int[] l = [2, 3, 4, 5, 7, 8, 9, 11, 22];
            __result = GameManager.Data.Locations.Get(l[UnityEngine.Random.Range(0, l.Count() - 1)]);
            return false;
        }

        [HarmonyPatch(typeof(GirlPlayerData), "KnowDetail")]
        [HarmonyPrefix]
        public static void favlocation(GirlDetailType type, GirlPlayerData __instance)
        {
            int loc = 42069349 + (int)type + (12 * (__instance.GetGirlDefinition().id - 1));
            arch.sendLoc(loc);
        }

        [HarmonyPatch(typeof(TraitsCellApp), "OnStoreItemSlotPressed")]
        [HarmonyPrefix]
        public static bool talentoveride()
        {
            return false;
        }

        [HarmonyPatch(typeof(LocationManager), "CheckForSecretGirlUnlock")]
        [HarmonyPrefix]
        public static bool secretgirlunlockoverite(ref GirlDefinition __result)
        {
            __result = null;
            return false;
        }

        [HarmonyPatch(typeof(GirlProfileCellApp), "OnCollectionSlotPressed")]
        [HarmonyPrefix]
        public static bool collectionoverite()
        {
            return false;
        }

        [HarmonyPatch(typeof(GirlManager), "GiveItem")]
        [HarmonyPrefix]
        public static void releaseallitems()
        {
            if (GameManager.System.Player.pantiesTurnedIn.Count >= 12) { arch.sendCompletion(); }
        }

        [HarmonyPatch(typeof(PlayerManager), "RollNewStoreList")]
        [HarmonyPrefix]
        public static bool storeoverite(StoreItemPlayerData[] storeList, ItemType itemType)
        {
            if (itemType == ItemType.GIFT)
            {
                ArchipelagoConsole.LogMessage("PROCESSING SHOP ITEMS");
                List<ItemDefinition> p = new List<ItemDefinition>();
                for (int i = 0; i < CursedArchipelagoClient.alist.list.Count; i++)
                {
                    if (CursedArchipelagoClient.alist.list[i].putinshop)
                    {
                        p.Add(GameManager.Data.Items.Get(Util.archidtoitemid(CursedArchipelagoClient.alist.list[i].Id)));
                    }
                }
                ArchipelagoConsole.LogMessage("NUMBER OF ITEMS THAT NEED TO BE PUT INTO SHOP:" + p.Count);
                ListUtils.Shuffle<ItemDefinition>(p);
                if (p.Count > 12)
                {
                    p.RemoveRange(12, p.Count - 12);
                }
                for (int l = 0; l < 12; l++)
                {
                    if (p.Count > l)
                    {
                        ArchipelagoConsole.LogMessage(p[l].name + " ADDED TO SHOP");
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
            else if (itemType == ItemType.UNIQUE_GIFT)
            {
                for (int l = 0; l < 12; l++)
                {
                    storeList[l].itemDefinition = null;
                    storeList[l].soldOut = true;
                }
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(PlayerManager), "LogTossedItem")]
        [HarmonyPrefix]
        public static void toss(ItemDefinition item)
        {
            ArchipelagoConsole.LogMessage("ITEM TOSSED WITH ID:" + item.id.ToString());            
            long d = Util.itemidtoarchid(item.id);
            ArchipelagoConsole.LogMessage("ARCHID:" + d.ToString());
            if (d == -1) { return; }
            for (int l = 0; l < CursedArchipelagoClient.alist.list.Count; l++)
            {
                if (CursedArchipelagoClient.alist.list[l].Id == d && CursedArchipelagoClient.alist.list[l].processed && !CursedArchipelagoClient.alist.list[l].putinshop)
                {
                    ArchipelagoConsole.LogMessage(item.name + " will now will be put in shop");
                    CursedArchipelagoClient.alist.list[l].putinshop = true;
                    return;
                }
            }

        }






        [HarmonyPatch(typeof(GameManager), "LoadGame")]
        [HarmonyILManipulator]
        public static void loadgamereset(ILContext ctx, MethodBase orig)
        {
            for (int i = 0; i < ctx.Instrs.Count; i++)
            {
                if (ctx.Instrs[i].OpCode == OpCodes.Brtrue) { ctx.Instrs[i].OpCode = OpCodes.Brfalse; break; }
            }
        }


        [HarmonyPatch(typeof(PuzzleManager), "OnPuzzleGameComplete")]
        [HarmonyILManipulator]
        public static void removepantiesgift(ILContext ctx, MethodBase orig)
        {        
            for (int i = 0; i < ctx.Instrs.Count; i++)
            {        
                if (i < 10) { continue; }
                if (ctx.Instrs[i-1].OpCode == OpCodes.Br && ctx.Instrs[i].OpCode == OpCodes.Ldloc_0 && ctx.Instrs[i+1].OpCode == OpCodes.Callvirt)
                {
                    ctx.Instrs[i].OpCode = OpCodes.Nop;
                    //ctx.Instrs[i].Operand = null;
                    ctx.Instrs[i+1].OpCode = OpCodes.Ldc_I4_1;
                    //ctx.Instrs[i+1].Operand = null;
                }

                if (ctx.Instrs[i - 2].OpCode == OpCodes.Callvirt && ctx.Instrs[i - 1].OpCode == OpCodes.Br && ctx.Instrs[i].OpCode == OpCodes.Call && ctx.Instrs[i + 1].OpCode == OpCodes.Callvirt)
                {
                    ctx.Instrs[i].OpCode = OpCodes.Nop;
                    //ctx.Instrs[i].Operand = null;
                    ctx.Instrs[i + 1].OpCode = OpCodes.Nop;
                    //ctx.Instrs[i + 1].Operand = null;
                    ctx.Instrs[i + 2].OpCode = OpCodes.Nop;
                    //ctx.Instrs[i + 2].Operand = null;
                    ctx.Instrs[i + 3].OpCode = OpCodes.Ldc_I4_1;
                    //ctx.Instrs[i + 4].Operand = null;
                }
            }
        }

        //[HarmonyPatch(typeof(GirlFinderIcon), "Init")]
        //[HarmonyILManipulator]
        //public static void findernull(ILContext ctx, MethodBase orig)
        //{
        //    int call = 0;
        //
        //    for (int i = 0; i < ctx.Instrs.Count; i++)
        //    {
        //        if (call == 17 && ctx.Instrs[i].OpCode == OpCodes.Call)
        //        {
        //            ctx.Instrs[i].OpCode = OpCodes.Ldc_I4_0;
        //            ctx.Instrs[i].Operand = null;
        //            break;
        //        }
        //        if (ctx.Instrs[i].OpCode == OpCodes.Call) { call++; }
        //    }
        //}

    }

    class Util
    {
        public static int girlgifttoloc(GirlPlayerData girl, ItemDefinition item)
        {
            return 0;
        }

        public static void processarch()
        {

            PlayerManager player = GameManager.System.Player;
            ArchipelagoConsole.LogMessage("processing items");
            

            for (int i = 0; i < CursedArchipelagoClient.alist.list.Count; i++)
            {
                ArchipelagoItem item = CursedArchipelagoClient.alist.list[i];

                if (item.processed) { continue; }

                if (item.Id > 42069000 && item.Id < 42069013)
                {
                    //PANTIES ITEMS

                    if (player.pantiesTurnedIn.Contains(archidtoitemid(item.Id)))
                    {
                        ArchipelagoConsole.LogMessage(idtoitem((int)item.Id) + " already turned in skipping");
                        item.processed = true;
                        continue;
                    }

                    if (player.HasItem(GameManager.Data.Items.Get(archidtoitemid(item.Id)))){
                        ArchipelagoConsole.LogMessage(idtoitem((int)item.Id) + " already in inventory skipping");
                        item.processed = true;
                        continue;
                    }

                    if (!player.IsInventoryFull())
                    {
                        ArchipelagoConsole.LogMessage(idtoitem((int)item.Id) + " recieved");
                        player.AddItem(GameManager.Data.Items.Get(archidtoitemid(item.Id)), player.inventory, false, false);
                        item.processed = true;
                    }
                }
                else if (item.Id > 42069012 && item.Id < 42069025)
                {
                    //GIRL UNLOCKS
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
                else if (item.Id > 42069024 && item.Id < 42069097)
                {
                    //GIFT ITEMS
                    if (!player.IsInventoryFull())
                    {
                        player.AddItem(GameManager.Data.Items.Get(archidtoitemid(item.Id)), player.inventory, false, false);
                        ArchipelagoConsole.LogMessage(GameManager.Data.Items.Get(archidtoitemid(item.Id)).name + " recieved");
                        item.processed = true;
                    }
                }
                else if (item.Id > 42069096 && item.Id < 42069169)
                {
                    //UNIQUE GIFT ITEMS
                    if (!player.IsInventoryFull())
                    {
                        player.AddItem(GameManager.Data.Items.Get(archidtoitemid(item.Id)), player.inventory, false, false);
                        ArchipelagoConsole.LogMessage(GameManager.Data.Items.Get(archidtoitemid(item.Id)).name + " recieved");
                        item.processed = true;
                    }
                }
                else if (item.Id > 42069168 && item.Id < 42069217)
                {
                    //TOKEN ITEMS
                    ArchipelagoConsole.LogMessage("token recieved");
                    if (item.Id < 42069175)
                    {
                        //TALENT
                        player.UpgradeTraitLevel(PlayerTraitType.TALENT);
                        item.processed = true;
                    }
                    else if (item.Id < 42069181)
                    {
                        //FLIRTATION
                        player.UpgradeTraitLevel(PlayerTraitType.FLIRTATION);
                        item.processed = true;
                    }
                    else if (item.Id < 42069187)
                    {
                        //ROMANCE
                        player.UpgradeTraitLevel(PlayerTraitType.ROMANCE);
                        item.processed = true;
                    }
                    else if (item.Id < 42069193)
                    {
                        //SEXUALITY
                        player.UpgradeTraitLevel(PlayerTraitType.SEXUALITY);
                        item.processed = true;
                    }
                    else if (item.Id < 42069199)
                    {
                        //PASSION
                        player.UpgradeTraitLevel(PlayerTraitType.PASSION);
                        item.processed = true;
                    }
                    else if (item.Id < 42069205)
                    {
                        //SENSITIVITY
                        player.UpgradeTraitLevel(PlayerTraitType.SENSITIVITY);
                        item.processed = true;
                    }
                    else if (item.Id < 42069211)
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


            }
        }

        public static string idtoitem(int id)
        {
            switch (id)
            {
                case 42069001:
                    return "tiffany's panties";
                case 42069002:
                    return "aiko's panties";
                case 42069003:
                    return "kyanna's panties";
                case 42069004:
                    return "audrey's panties";
                case 42069005:
                    return "lola's panties";
                case 42069006:
                    return "nikki's panties";
                case 42069007:
                    return "jessie's panties";
                case 42069008:
                    return "beli's panties";
                case 42069009:
                    return "kyu's panties";
                case 42069010:
                    return "momo's panties";
                case 42069011:
                    return "celeste's panties";
                case 42069012:
                    return "venus's panties";
                case 42069013:
                    return "Unlock Girl(tiffany)";
                case 42069014:
                    return "Unlock Girl(aiko)";
                case 42069015:
                    return "Unlock Girl(kyanna)";
                case 42069016:
                    return "Unlock Girl(audrey)";
                case 42069017:
                    return "Unlock Girl(lola)";
                case 42069018:
                    return "Unlock Girl(nikki)";
                case 42069019:
                    return "Unlock Girl(jessie)";
                case 42069020:
                    return "Unlock Girl(beli)";
                case 42069021:
                    return "Unlock Girl(kyu)";
                case 42069022:
                    return "Unlock Girl(momo)";
                case 42069023:
                    return "Unlock Girl(celeste)";
                case 42069024:
                    return "Unlock Girl(venus)";
                case 42069025:
                    return "academy gift item 1";
                case 42069026:
                    return "academy gift item 2";
                case 42069027:
                    return "academy gift item 3";
                case 42069028:
                    return "academy gift item 4";
                case 42069029:
                    return "academy gift item 5";
                case 42069030:
                    return "academy gift item 6";
                case 42069031:
                    return "toys gift item 1";
                case 42069032:
                    return "toys gift item 2";
                case 42069033:
                    return "toys gift item 3";
                case 42069034:
                    return "toys gift item 4";
                case 42069035:
                    return "toys gift item 5";
                case 42069036:
                    return "toys gift item 6";
                case 42069037:
                    return "fitness gift item 1";
                case 42069038:
                    return "fitness gift item 2";
                case 42069039:
                    return "fitness gift item 3";
                case 42069040:
                    return "fitness gift item 4";
                case 42069041:
                    return "fitness gift item 5";
                case 42069042:
                    return "fitness gift item 6";
                case 42069043:
                    return "rave gift item 1";
                case 42069044:
                    return "rave gift item 2";
                case 42069045:
                    return "rave gift item 3";
                case 42069046:
                    return "rave gift item 4";
                case 42069047:
                    return "rave gift item 5";
                case 42069048:
                    return "rave gift item 6";
                case 42069049:
                    return "sports gift item 1";
                case 42069050:
                    return "sports gift item 2";
                case 42069051:
                    return "sports gift item 3";
                case 42069052:
                    return "sports gift item 4";
                case 42069053:
                    return "sports gift item 5";
                case 42069054:
                    return "sports gift item 6";
                case 42069055:
                    return "artist gift item 1";
                case 42069056:
                    return "artist gift item 2";
                case 42069057:
                    return "artist gift item 3";
                case 42069058:
                    return "artist gift item 4";
                case 42069059:
                    return "artist gift item 5";
                case 42069060:
                    return "artist gift item 6";
                case 42069061:
                    return "baking gift item 1";
                case 42069062:
                    return "baking gift item 2";
                case 42069063:
                    return "baking gift item 3";
                case 42069064:
                    return "baking gift item 4";
                case 42069065:
                    return "baking gift item 5";
                case 42069066:
                    return "baking gift item 6";
                case 42069067:
                    return "yoga gift item 1";
                case 42069068:
                    return "yoga gift item 2";
                case 42069069:
                    return "yoga gift item 3";
                case 42069070:
                    return "yoga gift item 4";
                case 42069071:
                    return "yoga gift item 5";
                case 42069072:
                    return "yoga gift item 6";
                case 42069073:
                    return "dancer gift item 1";
                case 42069074:
                    return "dancer gift item 2";
                case 42069075:
                    return "dancer gift item 3";
                case 42069076:
                    return "dancer gift item 4";
                case 42069077:
                    return "dancer gift item 5";
                case 42069078:
                    return "dancer gift item 6";
                case 42069079:
                    return "aquarium gift item 1";
                case 42069080:
                    return "aquarium gift item 2";
                case 42069081:
                    return "aquarium gift item 3";
                case 42069082:
                    return "aquarium gift item 4";
                case 42069083:
                    return "aquarium gift item 5";
                case 42069084:
                    return "aquarium gift item 6";
                case 42069085:
                    return "scuba gift item 1";
                case 42069086:
                    return "scuba gift item 2";
                case 42069087:
                    return "scuba gift item 3";
                case 42069088:
                    return "scuba gift item 4";
                case 42069089:
                    return "scuba gift item 5";
                case 42069090:
                    return "scuba gift item 6";
                case 42069091:
                    return "garden gift item 1";
                case 42069092:
                    return "garden gift item 2";
                case 42069093:
                    return "garden gift item 3";
                case 42069094:
                    return "garden gift item 4";
                case 42069095:
                    return "garden gift item 5";
                case 42069096:
                    return "garden gift item 6";
                case 42069097:
                    return "tiffany unique item 1";
                case 42069098:
                    return "tiffany unique item 2";
                case 42069099:
                    return "tiffany unique item 3";
                case 42069100:
                    return "tiffany unique item 4";
                case 42069101:
                    return "tiffany unique item 5";
                case 42069102:
                    return "tiffany unique item 6";
                case 42069103:
                    return "aiko unique item 1";
                case 42069104:
                    return "aiko unique item 2";
                case 42069105:
                    return "aiko unique item 3";
                case 42069106:
                    return "aiko unique item 4";
                case 42069107:
                    return "aiko unique item 5";
                case 42069108:
                    return "aiko unique item 6";
                case 42069109:
                    return "kyanna unique item 1";
                case 42069110:
                    return "kyanna unique item 2";
                case 42069111:
                    return "kyanna unique item 3";
                case 42069112:
                    return "kyanna unique item 4";
                case 42069113:
                    return "kyanna unique item 5";
                case 42069114:
                    return "kyanna unique item 6";
                case 42069115:
                    return "audrey unique item 1";
                case 42069116:
                    return "audrey unique item 2";
                case 42069117:
                    return "audrey unique item 3";
                case 42069118:
                    return "audrey unique item 4";
                case 42069119:
                    return "audrey unique item 5";
                case 42069120:
                    return "audrey unique item 6";
                case 42069121:
                    return "lola unique item 1";
                case 42069122:
                    return "lola unique item 2";
                case 42069123:
                    return "lola unique item 3";
                case 42069124:
                    return "lola unique item 4";
                case 42069125:
                    return "lola unique item 5";
                case 42069126:
                    return "lola unique item 6";
                case 42069127:
                    return "nikki unique item 1";
                case 42069128:
                    return "nikki unique item 2";
                case 42069129:
                    return "nikki unique item 3";
                case 42069130:
                    return "nikki unique item 4";
                case 42069131:
                    return "nikki unique item 5";
                case 42069132:
                    return "nikki unique item 6";
                case 42069133:
                    return "jessie unique item 1";
                case 42069134:
                    return "jessie unique item 2";
                case 42069135:
                    return "jessie unique item 3";
                case 42069136:
                    return "jessie unique item 4";
                case 42069137:
                    return "jessie unique item 5";
                case 42069138:
                    return "jessie unique item 6";
                case 42069139:
                    return "beli unique item 1";
                case 42069140:
                    return "beli unique item 2";
                case 42069141:
                    return "beli unique item 3";
                case 42069142:
                    return "beli unique item 4";
                case 42069143:
                    return "beli unique item 5";
                case 42069144:
                    return "beli unique item 6";
                case 42069145:
                    return "kyu unique item 1";
                case 42069146:
                    return "kyu unique item 2";
                case 42069147:
                    return "kyu unique item 3";
                case 42069148:
                    return "kyu unique item 4";
                case 42069149:
                    return "kyu unique item 5";
                case 42069150:
                    return "kyu unique item 6";
                case 42069151:
                    return "momo unique item 1";
                case 42069152:
                    return "momo unique item 2";
                case 42069153:
                    return "momo unique item 3";
                case 42069154:
                    return "momo unique item 4";
                case 42069155:
                    return "momo unique item 5";
                case 42069156:
                    return "momo unique item 6";
                case 42069157:
                    return "celeste unique item 1";
                case 42069158:
                    return "celeste unique item 2";
                case 42069159:
                    return "celeste unique item 3";
                case 42069160:
                    return "celeste unique item 4";
                case 42069161:
                    return "celeste unique item 5";
                case 42069162:
                    return "celeste unique item 6";
                case 42069163:
                    return "venus unique item 1";
                case 42069164:
                    return "venus unique item 2";
                case 42069165:
                    return "venus unique item 3";
                case 42069166:
                    return "venus unique item 4";
                case 42069167:
                    return "venus unique item 5";
                case 42069168:
                    return "venus unique item 6";
                case 42069169:
                    return "talent lv-up 1";
                case 42069170:
                    return "talent lv-up 2";
                case 42069171:
                    return "talent lv-up 3";
                case 42069172:
                    return "talent lv-up 4";
                case 42069173:
                    return "talent lv-up 5";
                case 42069174:
                    return "talent lv-up 6";
                case 42069175:
                    return "flirtation lv-up 1";
                case 42069176:
                    return "flirtation lv-up 2";
                case 42069177:
                    return "flirtation lv-up 3";
                case 42069178:
                    return "flirtation lv-up 4";
                case 42069179:
                    return "flirtation lv-up 5";
                case 42069180:
                    return "flirtation lv-up 6";
                case 42069181:
                    return "romance lv-up 1";
                case 42069182:
                    return "romance lv-up 2";
                case 42069183:
                    return "romance lv-up 3";
                case 42069184:
                    return "romance lv-up 4";
                case 42069185:
                    return "romance lv-up 5";
                case 42069186:
                    return "romance lv-up 6";
                case 42069187:
                    return "sexuality lv-up 1";
                case 42069188:
                    return "sexuality lv-up 2";
                case 42069189:
                    return "sexuality lv-up 3";
                case 42069190:
                    return "sexuality lv-up 4";
                case 42069191:
                    return "sexuality lv-up 5";
                case 42069192:
                    return "sexuality lv-up 6";
                case 42069193:
                    return "passion lv-up 1";
                case 42069194:
                    return "passion lv-up 2";
                case 42069195:
                    return "passion lv-up 3";
                case 42069196:
                    return "passion lv-up 4";
                case 42069197:
                    return "passion lv-up 5";
                case 42069198:
                    return "passion lv-up 6";
                case 42069199:
                    return "sensitivity lv-up 1";
                case 42069200:
                    return "sensitivity lv-up 2";
                case 42069201:
                    return "sensitivity lv-up 3";
                case 42069202:
                    return "sensitivity lv-up 4";
                case 42069203:
                    return "sensitivity lv-up 5";
                case 42069204:
                    return "sensitivity lv-up 6";
                case 42069205:
                    return "charisma lv-up 1";
                case 42069206:
                    return "charisma lv-up 2";
                case 42069207:
                    return "charisma lv-up 3";
                case 42069208:
                    return "charisma lv-up 4";
                case 42069209:
                    return "charisma lv-up 5";
                case 42069210:
                    return "charisma lv-up 6";
                case 42069211:
                    return "luck lv-up 1";
                case 42069212:
                    return "luck lv-up 2";
                case 42069213:
                    return "luck lv-up 3";
                case 42069214:
                    return "luck lv-up 4";
                case 42069215:
                    return "luck lv-up 5";
                case 42069216:
                    return "luck lv-up 6";
                default:
                    return "";
            }
        }

        public static int archidtoitemid(long id)
        {
            switch (id)
            {
                case 42069001:
                    return 277;//Tiffany's Panties
                case 42069002:
                    return 278;//Aiko's Panties
                case 42069003:
                    return 279;//Kyanna's Panties
                case 42069004:
                    return 280;//Audrey's Panties
                case 42069005:
                    return 281;//Lola's Panties
                case 42069006:
                    return 282;//Nikki's Panties
                case 42069007:
                    return 283;//Jessie's Panties
                case 42069008:
                    return 284;//Beli's Panties
                case 42069009:
                    return 285;//Kyu's Panties
                case 42069010:
                    return 286;//Momo's Panties
                case 42069011:
                    return 287;//Celeste's Panties
                case 42069012:
                    return 288;//Venus' Panties
                case 42069025:
                    return 49;//academy gift 1
                case 42069026:
                    return 50;//academy gift 2
                case 42069027:
                    return 51;//academy gift 3
                case 42069028:
                    return 52;//academy gift 4
                case 42069029:
                    return 53;//academy gift 5
                case 42069030:
                    return 54;//academy gift 6
                case 42069031:
                    return 55;//toys gift 1
                case 42069032:
                    return 56;//toys gift 2
                case 42069033:
                    return 57;//toys gift 3
                case 42069034:
                    return 58;//toys gift 4
                case 42069035:
                    return 59;//toys gift 5
                case 42069036:
                    return 60;//toys gift 6
                case 42069037:
                    return 61;//fitness gift 1
                case 42069038:
                    return 62;//fitness gift 2
                case 42069039:
                    return 63;//fitness gift 3
                case 42069040:
                    return 64;//fitness gift 4
                case 42069041:
                    return 65;//fitness gift 5
                case 42069042:
                    return 66;//fitness gift 6
                case 42069043:
                    return 67;//rave gift 1
                case 42069044:
                    return 68;//rave gift 2
                case 42069045:
                    return 69;//rave gift 3
                case 42069046:
                    return 70;//rave gift 4
                case 42069047:
                    return 71;//rave gift 5
                case 42069048:
                    return 72;//rave gift 6
                case 42069049:
                    return 73;//sports gift 1
                case 42069050:
                    return 74;//sports gift 2
                case 42069051:
                    return 75;//sports gift 3
                case 42069052:
                    return 76;//sports gift 4
                case 42069053:
                    return 77;//sports gift 5
                case 42069054:
                    return 78;//sports gift 6
                case 42069055:
                    return 79;//artist gift 1
                case 42069056:
                    return 80;//artist gift 2
                case 42069057:
                    return 81;//artist gift 3
                case 42069058:
                    return 82;//artist gift 4
                case 42069059:
                    return 83;//artist gift 5
                case 42069060:
                    return 84;//artist gift 6
                case 42069061:
                    return 85;//baking gift 1
                case 42069062:
                    return 86;//baking gift 2
                case 42069063:
                    return 87;//baking gift 3
                case 42069064:
                    return 88;//baking gift 4
                case 42069065:
                    return 89;//baking gift 5
                case 42069066:
                    return 90;//baking gift 6
                case 42069067:
                    return 91;//yoga gift 1
                case 42069068:
                    return 92;//yoga gift 2
                case 42069069:
                    return 93;//yoga gift 3
                case 42069070:
                    return 94;//yoga gift 4
                case 42069071:
                    return 95;//yoga gift 5
                case 42069072:
                    return 96;//yoga gift 6
                case 42069073:
                    return 97;//dancer gift 1
                case 42069074:
                    return 98;//dancer gift 2
                case 42069075:
                    return 99;//dancer gift 3
                case 42069076:
                    return 100;//dancer gift 4
                case 42069077:
                    return 101;//dancer gift 5
                case 42069078:
                    return 102;//dancer gift 6
                case 42069079:
                    return 103;//aquarium gift 1
                case 42069080:
                    return 104;//aquarium gift 2
                case 42069081:
                    return 105;//aquarium gift 3
                case 42069082:
                    return 106;//aquarium gift 4
                case 42069083:
                    return 107;//aquarium gift 5
                case 42069084:
                    return 108;//aquarium gift 6
                case 42069085:
                    return 109;//scuba gift 1
                case 42069086:
                    return 110;//scuba gift 2
                case 42069087:
                    return 111;//scuba gift 3
                case 42069088:
                    return 112;//scuba gift 4
                case 42069089:
                    return 113;//scuba gift 5
                case 42069090:
                    return 114;//scuba gift 6
                case 42069091:
                    return 115;//garden gift 1
                case 42069092:
                    return 116;//garden gift 2
                case 42069093:
                    return 117;//garden gift 3
                case 42069094:
                    return 118;//garden gift 4
                case 42069095:
                    return 119;//garden gift 5
                case 42069096:
                    return 120;//garden gift 6
                case 42069097:
                    return 193;//tiffany gift 1
                case 42069098:
                    return 194;//tiffany gift 2
                case 42069099:
                    return 195;//tiffany gift 3
                case 42069100:
                    return 196;//tiffany gift 4
                case 42069101:
                    return 197;//tiffany gift 5
                case 42069102:
                    return 198;//tiffany gift 6
                case 42069103:
                    return 199;//aiko gift 1
                case 42069104:
                    return 200;//aiko gift 2
                case 42069105:
                    return 201;//aiko gift 3
                case 42069106:
                    return 202;//aiko gift 4
                case 42069107:
                    return 203;//aiko gift 5
                case 42069108:
                    return 204;//aiko gift 6
                case 42069109:
                    return 205;//kyanna gift 1
                case 42069110:
                    return 206;//kyanna gift 2
                case 42069111:
                    return 207;//kyanna gift 3
                case 42069112:
                    return 208;//kyanna gift 4
                case 42069113:
                    return 209;//kyanna gift 5
                case 42069114:
                    return 210;//kyanna gift 6
                case 42069115:
                    return 211;//audrey gift 1
                case 42069116:
                    return 212;//audrey gift 2
                case 42069117:
                    return 213;//audrey gift 3
                case 42069118:
                    return 214;//audrey gift 4
                case 42069119:
                    return 215;//audrey gift 5
                case 42069120:
                    return 216;//audrey gift 6
                case 42069121:
                    return 217;//lola gift 1
                case 42069122:
                    return 218;//lola gift 2
                case 42069123:
                    return 219;//lola gift 3
                case 42069124:
                    return 220;//lola gift 4
                case 42069125:
                    return 221;//lola gift 5
                case 42069126:
                    return 222;//lola gift 6
                case 42069127:
                    return 223;//nikki gift 1
                case 42069128:
                    return 224;//nikki gift 2
                case 42069129:
                    return 225;//nikki gift 3
                case 42069130:
                    return 226;//nikki gift 4
                case 42069131:
                    return 227;//nikki gift 5
                case 42069132:
                    return 228;//nikki gift 6
                case 42069133:
                    return 229;//jessie gift 1
                case 42069134:
                    return 230;//jessie gift 2
                case 42069135:
                    return 231;//jessie gift 3
                case 42069136:
                    return 232;//jessie gift 4
                case 42069137:
                    return 233;//jessie gift 5
                case 42069138:
                    return 234;//jessie gift 6
                case 42069139:
                    return 235;//beli gift 1
                case 42069140:
                    return 236;//beli gift 2
                case 42069141:
                    return 237;//beli gift 3
                case 42069142:
                    return 238;//beli gift 4
                case 42069143:
                    return 239;//beli gift 5
                case 42069144:
                    return 240;//beli gift 6
                case 42069145:
                    return 241;//kyu gift 1
                case 42069146:
                    return 242;//kyu gift 2
                case 42069147:
                    return 243;//kyu gift 3
                case 42069148:
                    return 244;//kyu gift 4
                case 42069149:
                    return 245;//kyu gift 5
                case 42069150:
                    return 246;//kyu gift 6
                case 42069151:
                    return 247;//momo gift 1
                case 42069152:
                    return 248;//momo gift 2
                case 42069153:
                    return 249;//momo gift 3
                case 42069154:
                    return 250;//momo gift 4
                case 42069155:
                    return 251;//momo gift 5
                case 42069156:
                    return 252;//momo gift 6
                case 42069157:
                    return 253;//celeste gift 1
                case 42069158:
                    return 254;//celeste gift 2
                case 42069159:
                    return 255;//celeste gift 3
                case 42069160:
                    return 256;//celeste gift 4
                case 42069161:
                    return 257;//celeste gift 5
                case 42069162:
                    return 258;//celeste gift 6
                case 42069163:
                    return 259;//venus gift 1
                case 42069164:
                    return 260;//venus gift 2
                case 42069165:
                    return 261;//venus gift 3
                case 42069166:
                    return 262;//venus gift 4
                case 42069167:
                    return 263;//venus gift 5
                case 42069168:
                    return 264;//venus gift 6
                default:
                    return -1;
            }
        }

        public static long itemidtoarchid(int id)
        {
            switch (id)
            {
                case 277:
                    return 42069001;//Tiffany's Panties
                case 278:
                    return 42069002;//Aiko's Panties
                case 279:
                    return 42069003;//Kyanna's Panties
                case 280:
                    return 42069004;//Audrey's Panties
                case 281:
                    return 42069005;//Lola's Panties
                case 282:
                    return 42069006;//Nikki's Panties
                case 283:
                    return 42069007;//Jessie's Panties
                case 284:
                    return 42069008;//Beli's Panties
                case 285:
                    return 42069009;//Kyu's Panties
                case 286:
                    return 42069010;//Momo's Panties
                case 287:
                    return 42069011;//Celeste's Panties
                case 288:
                    return 42069012;//Venus' Panties
                case 49:
                    return 42069025;//academy gift 1
                case 50:
                    return 42069026;//academy gift 2
                case 51:
                    return 42069027;//academy gift 3
                case 52:
                    return 42069028;//academy gift 4
                case 53:
                    return 42069029;//academy gift 5
                case 54:
                    return 42069030;//academy gift 6
                case 55:
                    return 42069031;//toys gift 1
                case 56:
                    return 42069032;//toys gift 2
                case 57:
                    return 42069033;//toys gift 3
                case 58:
                    return 42069034;//toys gift 4
                case 59:
                    return 42069035;//toys gift 5
                case 60:
                    return 42069036;//toys gift 6
                case 61:
                    return 42069037;//fitness gift 1
                case 62:
                    return 42069038;//fitness gift 2
                case 63:
                    return 42069039;//fitness gift 3
                case 64:
                    return 42069040;//fitness gift 4
                case 65:
                    return 42069041;//fitness gift 5
                case 66:
                    return 42069042;//fitness gift 6
                case 67:
                    return 42069043;//rave gift 1
                case 68:
                    return 42069044;//rave gift 2
                case 69:
                    return 42069045;//rave gift 3
                case 70:
                    return 42069046;//rave gift 4
                case 71:
                    return 42069047;//rave gift 5
                case 72:
                    return 42069048;//rave gift 6
                case 73:
                    return 42069049;//sports gift 1
                case 74:
                    return 42069050;//sports gift 2
                case 75:
                    return 42069051;//sports gift 3
                case 76:
                    return 42069052;//sports gift 4
                case 77:
                    return 42069053;//sports gift 5
                case 78:
                    return 42069054;//sports gift 6
                case 79:
                    return 42069055;//artist gift 1
                case 80:
                    return 42069056;//artist gift 2
                case 81:
                    return 42069057;//artist gift 3
                case 82:
                    return 42069058;//artist gift 4
                case 83:
                    return 42069059;//artist gift 5
                case 84:
                    return 42069060;//artist gift 6
                case 85:
                    return 42069061;//baking gift 1
                case 86:
                    return 42069062;//baking gift 2
                case 87:
                    return 42069063;//baking gift 3
                case 88:
                    return 42069064;//baking gift 4
                case 89:
                    return 42069065;//baking gift 5
                case 90:
                    return 42069066;//baking gift 6
                case 91:
                    return 42069067;//yoga gift 1
                case 92:
                    return 42069068;//yoga gift 2
                case 93:
                    return 42069069;//yoga gift 3
                case 94:
                    return 42069070;//yoga gift 4
                case 95:
                    return 42069071;//yoga gift 5
                case 96:
                    return 42069072;//yoga gift 6
                case 97:
                    return 42069073;//dancer gift 1
                case 98:
                    return 42069074;//dancer gift 2
                case 99:
                    return 42069075;//dancer gift 3
                case 100:
                    return 42069076;//dancer gift 4
                case 101:
                    return 42069077;//dancer gift 5
                case 102:
                    return 42069078;//dancer gift 6
                case 103:
                    return 42069079;//aquarium gift 1
                case 104:
                    return 42069080;//aquarium gift 2
                case 105:
                    return 42069081;//aquarium gift 3
                case 106:
                    return 42069082;//aquarium gift 4
                case 107:
                    return 42069083;//aquarium gift 5
                case 108:
                    return 42069084;//aquarium gift 6
                case 109:
                    return 42069085;//scuba gift 1
                case 110:
                    return 42069086;//scuba gift 2
                case 111:
                    return 42069087;//scuba gift 3
                case 112:
                    return 42069088;//scuba gift 4
                case 113:
                    return 42069089;//scuba gift 5
                case 114:
                    return 42069090;//scuba gift 6
                case 115:
                    return 42069091;//garden gift 1
                case 116:
                    return 42069092;//garden gift 2
                case 117:
                    return 42069093;//garden gift 3
                case 118:
                    return 42069094;//garden gift 4
                case 119:
                    return 42069095;//garden gift 5
                case 120:
                    return 42069096;//garden gift 6
                case 193:
                    return 42069097;//tiffany gift 1
                case 194:
                    return 42069098;//tiffany gift 2
                case 195:
                    return 42069099;//tiffany gift 3
                case 196:
                    return 42069100;//tiffany gift 4
                case 197:
                    return 42069101;//tiffany gift 5
                case 198:
                    return 42069102;//tiffany gift 6
                case 199:
                    return 42069103;//aiko gift 1
                case 200:
                    return 42069104;//aiko gift 2
                case 201:
                    return 42069105;//aiko gift 3
                case 202:
                    return 42069106;//aiko gift 4
                case 203:
                    return 42069107;//aiko gift 5
                case 204:
                    return 42069108;//aiko gift 6
                case 205:
                    return 42069109;//kyanna gift 1
                case 206:
                    return 42069110;//kyanna gift 2
                case 207:
                    return 42069111;//kyanna gift 3
                case 208:
                    return 42069112;//kyanna gift 4
                case 209:
                    return 42069113;//kyanna gift 5
                case 210:
                    return 42069114;//kyanna gift 6
                case 211:
                    return 42069115;//audrey gift 1
                case 212:
                    return 42069116;//audrey gift 2
                case 213:
                    return 42069117;//audrey gift 3
                case 214:
                    return 42069118;//audrey gift 4
                case 215:
                    return 42069119;//audrey gift 5
                case 216:
                    return 42069120;//audrey gift 6
                case 217:
                    return 42069121;//lola gift 1
                case 218:
                    return 42069122;//lola gift 2
                case 219:
                    return 42069123;//lola gift 3
                case 220:
                    return 42069124;//lola gift 4
                case 221:
                    return 42069125;//lola gift 5
                case 222:
                    return 42069126;//lola gift 6
                case 223:
                    return 42069127;//nikki gift 1
                case 224:
                    return 42069128;//nikki gift 2
                case 225:
                    return 42069129;//nikki gift 3
                case 226:
                    return 42069130;//nikki gift 4
                case 227:
                    return 42069131;//nikki gift 5
                case 228:
                    return 42069132;//nikki gift 6
                case 229:
                    return 42069133;//jessie gift 1
                case 230:
                    return 42069134;//jessie gift 2
                case 231:
                    return 42069135;//jessie gift 3
                case 232:
                    return 42069136;//jessie gift 4
                case 233:
                    return 42069137;//jessie gift 5
                case 234:
                    return 42069138;//jessie gift 6
                case 235:
                    return 42069139;//beli gift 1
                case 236:
                    return 42069140;//beli gift 2
                case 237:
                    return 42069141;//beli gift 3
                case 238:
                    return 42069142;//beli gift 4
                case 239:
                    return 42069143;//beli gift 5
                case 240:
                    return 42069144;//beli gift 6
                case 241:
                    return 42069145;//kyu gift 1
                case 242:
                    return 42069146;//kyu gift 2
                case 243:
                    return 42069147;//kyu gift 3
                case 244:
                    return 42069148;//kyu gift 4
                case 245:
                    return 42069149;//kyu gift 5
                case 246:
                    return 42069150;//kyu gift 6
                case 247:
                    return 42069151;//momo gift 1
                case 248:
                    return 42069152;//momo gift 2
                case 249:
                    return 42069153;//momo gift 3
                case 250:
                    return 42069154;//momo gift 4
                case 251:
                    return 42069155;//momo gift 5
                case 252:
                    return 42069156;//momo gift 6
                case 253:
                    return 42069157;//celeste gift 1
                case 254:
                    return 42069158;//celeste gift 2
                case 255:
                    return 42069159;//celeste gift 3
                case 256:
                    return 42069160;//celeste gift 4
                case 257:
                    return 42069161;//celeste gift 5
                case 258:
                    return 42069162;//celeste gift 6
                case 259:
                    return 42069163;//venus gift 1
                case 260:
                    return 42069164;//venus gift 2
                case 261:
                    return 42069165;//venus gift 3
                case 262:
                    return 42069166;//venus gift 4
                case 263:
                    return 42069167;//venus gift 5
                case 264:
                    return 42069168;//venus gift 6
                default:
                    return -1;
            }
        }

    }
}
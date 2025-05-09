using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace HuniePopArchiepelagoClient.HuniePop.Gameplay
{
    [HarmonyPatch]
    public class Puzzle
    {
        /// <summary>
        /// send archipelago location for completing a date/having sex with a girl
        /// </summary>
        [HarmonyPatch(typeof(PuzzleManager), "OnPuzzleGameComplete")]
        [HarmonyPrefix]
        public static void datefin(PuzzleManager __instance, ref PuzzleGame ____activePuzzleGame)
        {
            GirlPlayerData girlData = GameManager.System.Player.GetGirlData(GameManager.System.Location.currentGirl);
            if (____activePuzzleGame.isVictorious)
            {
                if (girlData.relationshipLevel >= 1)
                {
                    Plugin.curse.sendLoc(42069013 + ((girlData.GetGirlDefinition().id - 1) * 4));
                }
                if (girlData.relationshipLevel >= 2)
                {
                    Plugin.curse.sendLoc(42069014 + ((girlData.GetGirlDefinition().id - 1) * 4));
                }
                if (girlData.relationshipLevel >= 3)
                {
                    Plugin.curse.sendLoc(42069015 + ((girlData.GetGirlDefinition().id - 1) * 4));
                }
                if (girlData.relationshipLevel >= 4)
                {
                    Plugin.curse.sendLoc(42069016 + ((girlData.GetGirlDefinition().id - 1) * 4));
                }
            }
            if (____activePuzzleGame.isBonusRound && !girlData.gotPanties)
            {
                girlData.gotPanties = true;
                girlData.AddPhotoEarned(3);
                Plugin.curse.sendLoc(42069001 + (girlData.GetGirlDefinition().id - 1));
            }
        }

        /// <summary>
        /// NOP instructions for when completeing a puzzle for getting panties
        /// </summary>
        [HarmonyPatch(typeof(PuzzleManager), "OnPuzzleGameComplete")]
        [HarmonyILManipulator]
        public static void removepantiesgift(ILContext ctx, MethodBase orig)
        {
            for (int i = 0; i < ctx.Instrs.Count; i++)
            {
                if (i < 10) { continue; }
                if (ctx.Instrs[i - 1].OpCode == OpCodes.Br && ctx.Instrs[i].OpCode == OpCodes.Ldloc_0 && ctx.Instrs[i + 1].OpCode == OpCodes.Callvirt)
                {
                    ctx.Instrs[i].OpCode = OpCodes.Nop;
                    ctx.Instrs[i + 1].OpCode = OpCodes.Ldc_I4_1;
                }

                if (ctx.Instrs[i - 2].OpCode == OpCodes.Callvirt && ctx.Instrs[i - 1].OpCode == OpCodes.Br && ctx.Instrs[i].OpCode == OpCodes.Call && ctx.Instrs[i + 1].OpCode == OpCodes.Callvirt)
                {
                    ctx.Instrs[i].OpCode = OpCodes.Nop;
                    ctx.Instrs[i + 1].OpCode = OpCodes.Nop;
                    ctx.Instrs[i + 2].OpCode = OpCodes.Nop;
                    ctx.Instrs[i + 3].OpCode = OpCodes.Ldc_I4_1;
                }
            }
        }

        /// <summary>
        /// set affection goal for a puzzle besed on what we want caped at 999999
        /// </summary>
        [HarmonyPatch(typeof(PuzzleManager), "GetAffectionGoal")]
        [HarmonyPrefix]
        public static bool puzzlegoalmodding(ref int __result)
        {
            int goal = Convert.ToInt32(Plugin.curse.connected.slot_data["puzzle_affection_base"]);
            int mod = 0;

            List<GirlPlayerData> girls = GameManager.System.Player.girls;
            foreach (GirlPlayerData g in girls)
            {
                mod += (g.relationshipLevel - 1);
            }
            __result = goal + (Convert.ToInt32(Plugin.curse.connected.slot_data["puzzle_affection_add"]) * mod);
            if (__result > 999999) { __result = 999999; }
            return false;
        }

        /// <summary>
        /// set number of moves when begining a puzzle game
        /// </summary>
        [HarmonyPatch(typeof(PuzzleGame), "Begin")]
        [HarmonyPrefix]
        public static void puzzlemodding(PuzzleGame __instance, ref int ____maxMoves)
        {
            ____maxMoves = 99;
            if (__instance.isBonusRound)
            {
                //ArchipelagoConsole.LogMessage("BONUS");
            }
            else
            {
                //ArchipelagoConsole.LogMessage("NOT BONUS");
                __instance.SetResourceValue(PuzzleGameResourceType.MOVES, Convert.ToInt32(Plugin.curse.connected.slot_data["puzzle_moves"]), false);
            }
        }
    }
}

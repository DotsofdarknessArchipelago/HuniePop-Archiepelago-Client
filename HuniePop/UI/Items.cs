using HarmonyLib;
using HuniePopArchiepelagoClient.Archipelago;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Reflection;
using UnityEngine;

namespace HuniePopArchiepelagoClient.HuniePop.UI
{
    [HarmonyPatch]
    public class Items
    {
        /// <summary>
        /// overwrite the tooltip for items in girls collection that you can buy
        /// NOTE mainly just the same method as vanilla method with checks for archipelago stuff
        /// </summary>
        [HarmonyPatch(typeof(ItemTooltipContent), "Refresh")]
        [HarmonyPrefix]
        public static void tooltipoverite(ref ItemTooltipContent __instance, ref ItemDefinition ____itemDefinition, ref bool ____collectionItem)
        {
            if (____itemDefinition != null && ____itemDefinition.type == ItemType.GIFT)
            {
                __instance.typeLabel.SetText(StringUtils.Titleize(____itemDefinition.type.ToString()) + "  |  " + StringUtils.Titleize(____itemDefinition.giftType.ToString()));
                __instance.valueLabel.SetText("Gift Value: " + ____itemDefinition.itemFunctionValue.ToString());
                __instance.nameLabel.SetColor(ColorUtils.HexToColor("A63E6E"));
                __instance.typeLabel.SetColor(ColorUtils.HexToColor("C06E94"));
                __instance.valueLabel.SetColor(ColorUtils.HexToColor("A16799"));

                if (____collectionItem && GameManager.System.Player.endingSceneShown)
                {
                    string text = __instance.valueLabel.label.text + "\n";
                    string text2 = ColorUtils.ColorToHex(__instance.valueLabel.label.color);
                    if (GameManager.System.GameState != GameState.SIM)
                    {
                        text += "[[Can't order while dating.]stop]";
                    }
                    else if (GameManager.System.Player.IsInventoryFull(GameManager.System.Player.inventory))
                    {
                        text += "[[Your inventory is too full.]stop]";
                    }
                    else if (CursedArchipelagoClient.alist.hasitem(IDs.itemidtoarchid(____itemDefinition.id)))
                    {
                        if (GameManager.System.Player.hunie < Convert.ToInt32(Plugin.curse.connected.slot_data["hunie_gift_cost"]))
                        {
                            text += "[[Not enough Hunie to order.]stop]";
                        }
                        else
                        {
                            text += $"[[Click to order ({Convert.ToInt32(Plugin.curse.connected.slot_data["hunie_gift_cost"])} Hunie).]go]";
                        }
                    }
                    else
                    {
                        text += "[[Item can't be ordered yet.]stop]";
                    }
                    __instance.valueLabel.SetColor(Color.white);
                    __instance.valueLabel.SetText(StringUtils.FlattenColorBunches(text, text2));
                }
            }
        }

        /// <summary>
        /// NOP the vanilla instructions for the method above to overwrite
        /// </summary>
        [HarmonyPatch(typeof(ItemTooltipContent), "Refresh")]
        [HarmonyILManipulator]
        public static void tooltipreplace(ILContext ctx, MethodBase orig)
        {
            for (int i = 10; i < ctx.Instrs.Count; i++)
            {
                if (ctx.Instrs[i].OpCode == OpCodes.Ldarg_0 &&
                    ctx.Instrs[i - 1].OpCode == OpCodes.Br &&
                    ctx.Instrs[i - 2].OpCode == OpCodes.Callvirt &&
                    ctx.Instrs[i - 3].OpCode == OpCodes.Call &&
                    ctx.Instrs[i - 4].OpCode == OpCodes.Ldstr &&
                    ctx.Instrs[i - 4].Operand.ToString() == "508591")
                {
                    for (int j = 0; j < 107; j++)
                    {
                        ctx.Instrs[i + j].OpCode = OpCodes.Nop;
                    }
                    break;
                }
            }
        }
    }
}

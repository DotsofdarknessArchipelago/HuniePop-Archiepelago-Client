using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Reflection;

namespace HuniePopArchiepelagoClient.UI
{

    [HarmonyPatch]
    public class Menu
    {
        /// <summary>
        /// Overwrite save file labels to show what save file to use
        /// </summary>
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

        /// <summary>
        /// overwite the talk button so it says "Dont Just Stare" instead of "+XX Hnnie"
        /// </summary>
        [HarmonyPatch(typeof(ActionMenuButton), "Refresh")]
        [HarmonyILManipulator]
        public static void talkedit(ILContext ctx, MethodBase orig)
        {
            bool plus = true;
            bool fifty = true;
            bool hunie = true;
            for (int i = 0; i < ctx.Instrs.Count; i++)
            {
                if (plus && ctx.Instrs[i].OpCode == OpCodes.Ldstr && ctx.Instrs[i].Operand.ToString() == "+") { ctx.Instrs[i].Operand = ""; plus = false; continue; }
                if (hunie && ctx.Instrs[i].OpCode == OpCodes.Ldstr && ctx.Instrs[i].Operand.ToString() == " Hunie") { ctx.Instrs[i].Operand = ""; hunie = false; break; }
                if (fifty && ctx.Instrs[i].OpCode == OpCodes.Ldc_R4 && ctx.Instrs[i].Operand.ToString() == "50") { ctx.Instrs[i].OpCode = OpCodes.Ldstr; ctx.Instrs[i].Operand = "Dont Just Stare"; fifty = false; continue; }
                if (!plus)
                {
                    ctx.Instrs[i].OpCode = OpCodes.Nop;
                }
            }
        }
    }
}

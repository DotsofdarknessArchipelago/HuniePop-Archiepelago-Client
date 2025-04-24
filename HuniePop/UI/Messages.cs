using HarmonyLib;

namespace HuniePopArchiepelagoClient.HuniePop.UI
{
    [HarmonyPatch]
    public class Messages
    {
        /// <summary>
        /// overwrite default text messages
        /// </summary>
        [HarmonyPatch(typeof(MessageData), "Get")]
        [HarmonyPostfix]
        public static void msgoveride(int id, ref MessageDefinition __result)
        {
            if (id == 43)
            {
                __result.messageText = "NOTE pressing F8 will expand the console at the top of the screen with the ability to send commands to the archipelago server, The \"$resync\" command will resend all locations checked to the server again in case of network issues";
            }
            if (id == 38)
            {
                __result.messageText = "Every date you go on is gonna be more challenging than the last. If you don't improve your traits you're gonna blow it, trust me.";
            }
        }
    }
}

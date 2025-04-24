using HarmonyLib;
using HuniePopArchiepelagoClient.Archipelago;

namespace HuniePopArchiepelagoClient.HuniePop.Gameplay
{
    [HarmonyPatch]
    public class TossedItem
    {
        /// <summary>
        /// make logic items that have benn tossed be put in th eshop
        /// </summary>
        [HarmonyPatch(typeof(PlayerManager), "LogTossedItem")]
        [HarmonyPrefix]
        public static void toss(ItemDefinition item)
        {
            if (item.type != ItemType.UNIQUE_GIFT)
            {
                return;
            }

            long d = IDs.itemidtoarchid(item.id);
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
    }
}

using HarmonyLib;

namespace HuniePopArchiepelagoClient.HuniePop.Gameplay
{
    [HarmonyPatch]
    public class Traits
    {
        /// <summary>
        /// disable buying talent upgrades
        /// </summary>
        [HarmonyPatch(typeof(TraitsCellApp), "OnStoreItemSlotPressed")]
        [HarmonyPrefix]
        public static bool talentoveride()
        {
            return false;
        }
    }
}

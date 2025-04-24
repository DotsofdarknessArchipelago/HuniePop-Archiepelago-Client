using HarmonyLib;
using System.Linq;

namespace HuniePopArchiepelagoClient.HuniePop.Girls
{
    [HarmonyPatch]
    public class Location
    {
        /// <summary>
        /// make all girls be avaliable at all time and put them in a random location
        /// </summary>
        [HarmonyPatch(typeof(GirlDefinition),"IsAtLocationAtTime")]
        [HarmonyPrefix]
        public static bool sceduleoverite(ref LocationDefinition __result)
        {
            int[] l = [2, 3, 4, 5, 7, 8, 9, 11, 22];
            __result = GameManager.Data.Locations.Get(l[UnityEngine.Random.Range(0, l.Count() - 1)]);
            return false;
        }

        /// <summary>
        /// disable checks for unlocking secret girls
        /// </summary>
        [HarmonyPatch(typeof(LocationManager), "CheckForSecretGirlUnlock")]
        [HarmonyPrefix]
        public static bool secretgirlunlockoverite(ref GirlDefinition __result)
        {
            __result = null;
            return false;
        }
    }
}

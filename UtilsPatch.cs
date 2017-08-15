using Harmony;

using UnityEngine;

namespace LoadAsset
{
    [HarmonyPatch(typeof(Utils), "GetInventoryIconTextureFromName")]
    internal class UtilsPatch
    {
        public static bool Prefix(string spriteName, ref Texture2D __result)
        {
            __result = (Texture2D) Resources.Load("InventoryGridIcons/" + spriteName);

            return false;
        }
    }
}

using Harmony;

using UnityEngine;

namespace AssetLoader
{
    [HarmonyPatch(typeof(Utils), "GetInventoryIconTextureFromName")]
    internal class UtilsGetInventoryIconTextureFromNamePatch
    {
        public static bool Prefix(string spriteName, ref Texture2D __result)
        {
            __result = (Texture2D)Resources.Load("InventoryGridIcons/" + spriteName);

            return false;
        }
    }
}

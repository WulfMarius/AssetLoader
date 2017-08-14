using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Harmony;

using UnityEngine;

namespace LoadAsset
{
    [HarmonyPatch(typeof(Utils), "GetInventoryIconTextureFromName")]
    internal class UtilsPatch
    {
        public static bool Prefix(string spriteName, ref Texture2D __result)
        {
            Debug.Log("Utils.GetInventoryIconTextureFromName(" + spriteName + ")");

            if (spriteName.StartsWith("MOD_ico_GearItem__"))
            {
                spriteName = "InventoryGridIcons/" + spriteName;
                Debug.Log("Redirecting to Resources.Load(" + spriteName + ")");
                __result = (Texture2D) Resources.Load(spriteName);
                return false;
            }

            return true;
        }
    }
}

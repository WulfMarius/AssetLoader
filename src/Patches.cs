using Harmony;
using System;
using UnityEngine;

namespace AssetLoader
{
    [HarmonyPatch(typeof(GameAudioManager), "LoadSoundBanks")]
    internal class GameAudioManager_LoadSoundBanksPath
    {
        public static void Postfix()
        {
            ModSoundBankManager.RegisterPendingSoundBanks();
        }
    }

    // Hinterland load assets by calling Resources.Load which ignores external AssetBundles
    // so we need to patch Resources.Load to redirect specific calls to load from the AssetBundle instead
    [HarmonyPatch(typeof(Resources), "Load", new Type[] { typeof(string) })]
    internal class ResourcesLoadPatch
    {
        public static bool Prefix(string path, ref UnityEngine.Object __result)
        {
            if (!ModAssetBundleManager.IsKnownAsset(path))
            {
                return true;
            }

            __result = ModAssetBundleManager.LoadAsset(path);
            return false;
        }
    }

    [HarmonyPatch(typeof(Utils), "GetInventoryIconTextureFromName")]
    internal class UtilsGetInventoryIconTextureFromNamePatch
    {
        public static bool Prefix(string spriteName, ref Texture2D __result)
        {
            __result = Resources.Load("InventoryGridIcons/" + spriteName) as Texture2D;

            return false;
        }
    }
}
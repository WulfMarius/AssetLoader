using System;

using Harmony;

using UnityEngine;

namespace AssetLoader
{
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
}
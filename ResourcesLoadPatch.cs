using System;

using Harmony;

using UnityEngine;

namespace LoadAsset
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

            //Debug.Log("Redirecting Resources.Load(" + path + ") to ModAssetBundleManager.LoadAsset(" + path + ")");
            __result = ModAssetBundleManager.LoadAsset(path);
            return false;
        }
    }
}
using System;
using System.Reflection;

using Harmony;

using UnityEngine;

namespace ModAsset
{
    // Hinterland load assets by calling Resources.Load which ignores external AssetBundles
    // so we need to patch Resources.Load to redirect specific calls to load from the AssetBundle instead
    [HarmonyPatch(typeof(Resources), "Load", new Type[] { typeof(string) })]
    internal class ResourcesLoadPatch
    {
        public static bool Prefix(string path, ref UnityEngine.Object __result)
        {
            string prefix = ModAssetBundleManager.MOD_PREFIX;

            if (!path.StartsWith(prefix))
            {
                return true;
            }

            //Debug.Log("Intercepting Resource.Load(" + path + ")");

            int index = path.IndexOf("/", prefix.Length);
            string modName = path.Substring(prefix.Length, index - prefix.Length);
            string assetPath = path.Substring(index + 1);
            //Debug.Log("Mod name: " + name);
            //Debug.Log("Asset path " + assetPath);

            AssetBundle assetBundle = ModAssetBundleManager.GetAssetBundle(modName);
            __result = assetBundle.LoadAsset(assetPath);

            if (!__result)
            {
                Debug.LogError("Did not find asset at path " + assetPath + ".");
            }

            return false;
        }
    }
}
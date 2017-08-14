using System.Collections.Generic;
using System.IO;
using System.Reflection;

using UnityEngine;

namespace ModAsset
{
    public class ModAssetBundleManager
    {
        public const string MOD_PREFIX = "mod/";

        private static Dictionary<string, AssetBundle> loadedAssetBundles = new Dictionary<string, AssetBundle>();

        public static void LoadAssetBundle(string name)
        {
            if (loadedAssetBundles.ContainsKey(name))
            {
                return;
            }

            string modDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string assetBundlePath = Path.Combine(modDirectory, name);
            Debug.Log("Loading mod asset bundle " + name + " from path " + assetBundlePath);

            AssetBundle assetBundle = AssetBundle.LoadFromFile(assetBundlePath);
            if (!assetBundle)
            {
                Debug.LogError("Could not load asset bundle from " + assetBundlePath + ". Make sure the file exists and was created with the correct version of Unity.");
            }

            loadedAssetBundles.Add(name, assetBundle);
        }

        public static AssetBundle GetAssetBundle(string name)
        {
            if (!loadedAssetBundles.ContainsKey(name))
            {
                LoadAssetBundle(name);
            }

            return loadedAssetBundles[name];
        }
    }
}
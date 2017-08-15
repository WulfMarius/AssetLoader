using System.Collections.Generic;
using System.IO;
using System.Reflection;

using UnityEngine;

namespace LoadAsset
{
    public class ModAssetBundleManager
    {
        private static Dictionary<string, AssetBundle> knownAssetBundles = new Dictionary<string, AssetBundle>();
        private static Dictionary<string, AssetBundle> knownAssetNames = new Dictionary<string, AssetBundle>();
        private static Dictionary<string, string> knownAssetShortNames = new Dictionary<string, string>();

        public static bool IsKnownAsset(string name)
        {
            if (name == null)
            {
                return false;
            }

            return getFullAssetName(name) != null;
        }

        public static void RegisterAssetBundle(string relativePath)
        {
            string modDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string assetBundlePath = Path.Combine(modDirectory, relativePath);

            Debug.Log("Loading mod asset bundle '" + relativePath + "' from path '" + assetBundlePath + "'.");

            AssetBundle assetBundle = AssetBundle.LoadFromFile(assetBundlePath);
            if (!assetBundle)
            {
                Debug.LogError("Could not load asset bundle from '" + assetBundlePath + "'. Make sure the file exists and was created with the correct version of Unity.");
                return;
            }

            knownAssetBundles.Add(relativePath, assetBundle);

            string message = "Registered asset bundle '" + relativePath + "' with the following assets\n";
            foreach (string eachAssetName in assetBundle.GetAllAssetNames())
            {
                Object asset = assetBundle.LoadAsset(eachAssetName);

                string shortName = getAssetShortName(eachAssetName, asset.name);
                knownAssetShortNames.Add(shortName, eachAssetName);
                knownAssetNames.Add(eachAssetName, assetBundle);

                message += "  " + shortName + " => " + eachAssetName + "\n";
            }

            Debug.Log(message);
        }

        public static Object LoadAsset(string name)
        {
            //Debug.Log("Trying to load mod asset " + name);

            string fullAssetName = getFullAssetName(name);

            AssetBundle assetBundle;
            if (knownAssetNames.TryGetValue(fullAssetName, out assetBundle))
            {
                return assetBundle.LoadAsset(fullAssetName);
            }

            Debug.LogError("Unknown asset " + name + ". Did you forget to register an asset bundle?");
            return null;
        }

        private static string getAssetShortName(string assetPath, string assetName)
        {
            string result = assetPath.ToLower();

            if (result.StartsWith("assets/"))
            {
                result = result.Substring("assets/".Length);
            }

            int index = result.LastIndexOf(assetName.ToLower());
            if (index != -1)
            {
                result = result.Substring(0, index + assetName.Length);
            }


            return result;
        }

        private static string getFullAssetName(string name)
        {
            string lowerCaseName = name.ToLower();
            if (knownAssetNames.ContainsKey(lowerCaseName))
            {
                return lowerCaseName;
            }

            if (knownAssetShortNames.ContainsKey(lowerCaseName))
            {
                return knownAssetShortNames[lowerCaseName];
            }

            return null;
        }
    }
}
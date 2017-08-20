using System.Collections.Generic;
using System.IO;
using System.Reflection;

using UnityEngine;

namespace AssetLoader
{
    public class ModAssetBundleManager
    {
        private const string PATH_PREFIX_ASSETS = "assets/";

        private const string ASSET_NAME_LOCALIZATION = "localization";

        private static Dictionary<string, AssetBundle> knownAssetBundles = new Dictionary<string, AssetBundle>();
        private static Dictionary<string, AssetBundle> knownAssetNames = new Dictionary<string, AssetBundle>();
        private static Dictionary<string, string> knownAssetShortPaths = new Dictionary<string, string>();

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
            if (knownAssetBundles.ContainsKey(relativePath))
            {
                Debug.LogWarning("Asset bundle '" + relativePath + "' has already been registered.");
                return;
            }

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
                string assetName = GetAssetName(eachAssetName);

                if (assetName == ASSET_NAME_LOCALIZATION)
                {
                    Object asset = assetBundle.LoadAsset(eachAssetName);
                    LoadLocalization(asset);
                    continue;
                }

                string shortName = getAssetShortName(eachAssetName, assetName);
                knownAssetShortPaths.Add(shortName, eachAssetName);
                knownAssetNames.Add(eachAssetName, assetBundle);

                message += "  " + shortName + " => " + eachAssetName + "\n";
            }

            Debug.Log(message);
        }

        public static Object LoadAsset(string name)
        {
            string fullAssetName = getFullAssetName(name);

            AssetBundle assetBundle;
            if (knownAssetNames.TryGetValue(fullAssetName, out assetBundle))
            {
                return assetBundle.LoadAsset(fullAssetName);
            }

            Debug.LogError("Unknown asset " + name + ". Did you forget to register an asset bundle?");
            return null;
        }

        private static string GetAssetName(string assetPath)
        {
            string result = assetPath;

            int index = assetPath.LastIndexOf('/');
            if (index != -1)
            {
                result = result.Substring(index + 1);
            }

            index = result.LastIndexOf('.');
            if (index != -1)
            {
                result = result.Substring(0, index);
            }

            return result;
        }

        private static string getAssetShortName(string assetPath, string assetName)
        {
            string result = assetPath.ToLower();

            if (result.StartsWith(PATH_PREFIX_ASSETS))
            {
                result = result.Substring(PATH_PREFIX_ASSETS.Length);
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

            if (knownAssetShortPaths.ContainsKey(lowerCaseName))
            {
                return knownAssetShortPaths[lowerCaseName];
            }

            return null;
        }

        public static void LoadLocalization(Object asset)
        {
            TextAsset textAsset = asset as TextAsset;
            if (textAsset == null)
            {
                Debug.LogError("Asset called '" + asset.name + "' is not a TextAsset as expected.");
                return;
            }

            Debug.Log("Processing asset '" + asset.name + "' as localization.");

            ByteReader byteReader = new ByteReader(textAsset);
            string[] languages = Trim(byteReader.ReadCSV().ToArray());
            string[] knownLanguages = Localization.knownLanguages;

            while (true)
            {
                BetterList<string> values = byteReader.ReadCSV();
                if (values == null)
                {
                    break;
                }

                string[] translations = new string[knownLanguages.Length];
                for (int i = 0; i < knownLanguages.Length; i++)
                {
                    string language = knownLanguages[i];
                    int index = System.Array.IndexOf(languages, language);
                    if (index == -1)
                    {
                        continue;
                    }

                    translations[i] = values[index].Trim();
                }

                Localization.dictionary.Add(values[0], translations);
            }
        }

        private static string[] Trim(string[] values)
        {
            string[] result = new string[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                result[i] = values[i].Trim();
            }

            return result;
        }
    }
}
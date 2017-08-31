using System.Collections.Generic;
using System.IO;
using System.Text;
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
                Log("Asset bundle '{0}' has already been registered.", relativePath);
                return;
            }

            string modDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string fullPath = Path.Combine(modDirectory, relativePath);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("Asset bundle '" + relativePath + "' could not be found at '" + fullPath + "'.");
            }

            LoadAssetBundle(relativePath, fullPath);
        }

        private static void LoadAssetBundle(string relativePath, string fullPath)
        {
            Log("Loading mod asset bundle from '{0}'.", fullPath);

            AssetBundle assetBundle = AssetBundle.LoadFromFile(fullPath);
            if (!assetBundle)
            {
                throw new System.Exception("Could not load asset bundle from '" + fullPath + "'. Make the file was created with the correct version of Unity (should be 5.6.x).");
            }

            knownAssetBundles.Add(relativePath, assetBundle);

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Registered asset bundle '");
            stringBuilder.Append(relativePath);
            stringBuilder.Append("' with the following assets\n");

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

                stringBuilder.Append("  ");
                stringBuilder.Append(shortName);
                stringBuilder.Append(" => ");
                stringBuilder.Append(eachAssetName);
                stringBuilder.Append("\n");
            }

            Log(stringBuilder.ToString().Trim());
        }

        public static Object LoadAsset(string name)
        {
            string fullAssetName = getFullAssetName(name);

            AssetBundle assetBundle;
            if (knownAssetNames.TryGetValue(fullAssetName, out assetBundle))
            {
                return assetBundle.LoadAsset(fullAssetName);
            }

            throw new System.Exception("Unknown asset " + name + ". Did you forget to register an asset bundle?");
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
                Log("Asset called '{0}' is not a TextAsset as expected.", asset.name);
                return;
            }

            Log("Processing asset '{0}' as localization.", asset.name);

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

        private static void Log(string message)
        {
            AssetUtils.Log("ModAssetBundleManager", message);
        }

        private static void Log(string message, params object[] parameters)
        {
            AssetUtils.Log("ModAssetBundleManager", message, parameters);
        }
    }
}
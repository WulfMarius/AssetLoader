using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace AssetLoader
{
    public class ModAssetBundleManager
    {
        private const string ASSET_NAME_LOCALIZATION = "localization";
        private const string PATH_PREFIX_ASSETS = "assets/";

        private static Dictionary<string, AssetBundle> knownAssetBundles = new Dictionary<string, AssetBundle>();
        private static Dictionary<string, AssetBundle> knownAssetNames = new Dictionary<string, AssetBundle>();
        private static Dictionary<string, string> knownAssetShortPaths = new Dictionary<string, string>();
        private static Dictionary<string, UIAtlas> knownSpriteAtlases = new Dictionary<string, UIAtlas>();

        public static AssetBundle GetAssetBundle(string relativePath)
        {
            AssetBundle result;
            knownAssetBundles.TryGetValue(relativePath, out result);
            return result;
        }

        public static bool IsKnownAsset(string name)
        {
            if (name == null)
            {
                return false;
            }

            return getFullAssetName(name) != null;
        }

        public static Object LoadAsset(string name)
        {
            string fullAssetName = getFullAssetName(name);

            AssetBundle assetBundle;
            if (knownAssetNames.TryGetValue(fullAssetName, out assetBundle))
            {
                return assetBundle.LoadAsset(fullAssetName);
            }

            throw new System.Exception("Unknown asset " + name + ". Did you forget to register an AssetBundle?");
        }

        internal static UIAtlas GetSpriteAtlas(string spriteName)
        {
            UIAtlas result = null;
            knownSpriteAtlases.TryGetValue(spriteName, out result);
            return result;
        }

        public static void LoadLocalization(Object asset)
        {
            TextAsset textAsset = asset as TextAsset;
            if (textAsset == null)
            {
                AssetUtils.Log("Asset called '{0}' is not a TextAsset as expected.", asset.name);
                return;
            }

            AssetUtils.Log("Processing asset '{0}' as localization.", asset.name);

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

                var key = values[0];
                if (Localization.dictionary.ContainsKey(key))
                {
                    Localization.dictionary[key] = translations;
                }
                else
                {
                    Localization.dictionary.Add(key, translations);
                }
            }
        }

        public static void LoadUiAtlas(Object asset)
        {
            GameObject gameObject = asset as GameObject;
            if (gameObject == null)
            {
                AssetUtils.Log("Asset called '{0}' is not a GameObject as expected.", asset.name);
                return;
            }

            UIAtlas uiAtlas = gameObject.GetComponent<UIAtlas>();
            if (uiAtlas == null)
            {
                AssetUtils.Log("Asset called '{0}' does not contain a UIAtlast as expected.", asset.name);
                return;
            }

            AssetUtils.Log("Processing asset '{0}' as UIAtlas.", asset.name);

            BetterList<string> sprites = uiAtlas.GetListOfSprites();
            foreach (var eachSprite in sprites)
            {
                if (knownSpriteAtlases.ContainsKey(eachSprite))
                {
                    Debug.LogWarning("Replacing definition of sprite '" + eachSprite + "' from atlas '" + knownSpriteAtlases[eachSprite].name + "' to '" + uiAtlas.name + "'.");
                    knownSpriteAtlases[eachSprite] = uiAtlas;
                }
                else
                {
                    knownSpriteAtlases.Add(eachSprite, uiAtlas);
                }
            }
        }

        public static void RegisterAssetBundle(string relativePath)
        {
            if (knownAssetBundles.ContainsKey(relativePath))
            {
                AssetUtils.Log("AssetBundle '{0}' has already been registered.", relativePath);
                return;
            }

            string modDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string fullPath = Path.Combine(modDirectory, relativePath);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("AssetBundle '" + relativePath + "' could not be found at '" + fullPath + "'.");
            }

            LoadAssetBundle(relativePath, fullPath);
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

        private static void LoadAssetBundle(string relativePath, string fullPath)
        {
            AssetUtils.Log("Loading AssetBundle from '{0}'.", fullPath);

            AssetBundle assetBundle = AssetBundle.LoadFromFile(fullPath);
            if (!assetBundle)
            {
                throw new System.Exception("Could not load AssetBundle from '" + fullPath + "'. Make sure the file was created with the correct version of Unity (should be 5.6.x).");
            }

            knownAssetBundles.Add(relativePath, assetBundle);

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Registered AssetBundle '");
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

                if (assetName.EndsWith("atlas", System.StringComparison.InvariantCultureIgnoreCase))
                {
                    Object asset = assetBundle.LoadAsset(eachAssetName);
                    LoadUiAtlas(asset);
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

            AssetUtils.Log(stringBuilder.ToString().Trim());
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
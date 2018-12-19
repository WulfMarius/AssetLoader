using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

using static AssetLoader.Implementation;

namespace AssetLoader
{
    public class ModAssetBundleManager
    {
        private const string ASSET_NAME_LOCALIZATION = "localization";
        private const string ASSET_NAME_PREFIX_GEAR = "gear_";
        private const string ASSET_NAME_SUFFIX = "atlas";
        private const string ASSET_PATH_SUFFIX_PREFAB = ".prefab";

        private static readonly string[] RESOURCE_FOLDER = { "assets/", "logimages/", "clothingpaperdoll/female/", "clothingpaperdoll/male/" };

        private static Dictionary<string, AssetBundle> knownAssetBundles = new Dictionary<string, AssetBundle>();
        private static Dictionary<string, string> knownAssetMappedNames = new Dictionary<string, string>();
        private static Dictionary<string, AssetBundle> knownAssetNames = new Dictionary<string, AssetBundle>();
        private static Dictionary<string, UIAtlas> knownSpriteAtlases = new Dictionary<string, UIAtlas>();

        public static AssetBundle GetAssetBundle(string relativePath)
        {
            knownAssetBundles.TryGetValue(relativePath, out AssetBundle result);
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

            if (knownAssetNames.TryGetValue(fullAssetName, out AssetBundle assetBundle))
            {
                return assetBundle.LoadAsset(fullAssetName);
            }

            throw new System.Exception("Unknown asset " + name + ". Did you forget to register an AssetBundle?");
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
                Log("Asset called '{0}' is not a GameObject as expected.", asset.name);
                return;
            }

            UIAtlas uiAtlas = gameObject.GetComponent<UIAtlas>();
            if (uiAtlas == null)
            {
                Log("Asset called '{0}' does not contain a UIAtlast as expected.", asset.name);
                return;
            }

            Log("Processing asset '{0}' as UIAtlas.", asset.name);

            BetterList<string> sprites = uiAtlas.GetListOfSprites();
            foreach (var eachSprite in sprites)
            {
                if (knownSpriteAtlases.ContainsKey(eachSprite))
                {
                    Log("Replacing definition of sprite '{0}' from atlas '{1}' to '{2}'.", eachSprite, knownSpriteAtlases[eachSprite].name, uiAtlas.name);
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
                Log("AssetBundle '{0}' has already been registered.", relativePath);
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

        internal static UIAtlas GetSpriteAtlas(string spriteName)
        {
            UIAtlas result = null;
            knownSpriteAtlases.TryGetValue(spriteName, out result);
            return result;
        }

        private static string getAssetMappedName(string assetPath, string assetName)
        {
            if (assetName.StartsWith(ASSET_NAME_PREFIX_GEAR) && assetPath.EndsWith(ASSET_PATH_SUFFIX_PREFAB))
            {
                return assetName;
            }

            string result = assetPath;

            result = StripResourceFolder(result);

            int index = result.LastIndexOf(assetName);
            if (index != -1)
            {
                result = result.Substring(0, index + assetName.Length);
            }

            return result;
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

        private static string getFullAssetName(string name)
        {
            string lowerCaseName = name.ToLowerInvariant();
            if (knownAssetNames.ContainsKey(lowerCaseName))
            {
                return lowerCaseName;
            }

            if (knownAssetMappedNames.ContainsKey(lowerCaseName))
            {
                return knownAssetMappedNames[lowerCaseName];
            }

            return null;
        }

        private static void LoadAssetBundle(string relativePath, string fullPath)
        {
            Log("Loading AssetBundle from '{0}'.", fullPath);

            AssetBundle assetBundle = AssetBundle.LoadFromFile(fullPath);
            if (!assetBundle)
            {
                throw new System.Exception("Could not load AssetBundle from '" + fullPath + "'. Make sure the file was created with the correct version of Unity (should be 2018.2.x).");
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

                if (assetName.EndsWith(ASSET_NAME_SUFFIX))
                {
                    Object asset = assetBundle.LoadAsset(eachAssetName);
                    LoadUiAtlas(asset);
                    continue;
                }

                if (knownAssetNames.ContainsKey(eachAssetName))
                {
                    Log("Duplicate asset name '{0}'.", eachAssetName);
                    continue;
                }

                knownAssetNames.Add(eachAssetName, assetBundle);

                string mappedName = getAssetMappedName(eachAssetName, assetName);
                knownAssetMappedNames.Add(mappedName, eachAssetName);

                stringBuilder.Append("  ");
                stringBuilder.Append(mappedName);
                stringBuilder.Append(" => ");
                stringBuilder.Append(eachAssetName);
                stringBuilder.Append("\n");
            }

            Log(stringBuilder.ToString().Trim());
        }

        private static string StripResourceFolder(string assetPath)
        {
            string result = assetPath;

            while (true)
            {
                string resourceFolder = RESOURCE_FOLDER.Where(eachResourceFolder => result.StartsWith(eachResourceFolder)).FirstOrDefault();
                if (resourceFolder == null)
                {
                    break;
                }

                result = result.Substring(resourceFolder.Length);
            }

            return result;
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
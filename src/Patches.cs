using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AssetLoader
{
    [HarmonyPatch()]
    internal class DefaultAssetBundleRef_LoadAsset_Texture2D
    {
        internal static bool Prefix(string name, ref Texture2D __result)
        {
            if (!ModAssetBundleManager.IsKnownAsset(name))
            {
                return true;
            }

            __result = ModAssetBundleManager.LoadAsset(name) as Texture2D;
            return __result == null;
        }

        internal static MethodBase TargetMethod()
        {
            MethodInfo[] methods = typeof(DefaultAssetBundleRef).GetMethods();
            foreach (MethodInfo eachMethod in methods)
            {
                if (eachMethod.Name == "LoadAsset" && eachMethod.GetGenericArguments().Length == 1)
                {
                    return eachMethod.MakeGenericMethod(typeof(Texture2D));
                }
            }

            Debug.LogWarning("Could not find target method for patch DefaultAssetBundleRef_LoadAsset_Texture2D.");

            // fallback is our own method, so harmony won't fail during load
            return typeof(DefaultAssetBundleRef_LoadAsset_Texture2D).GetMethod("Prefix");
        }
    }

    [HarmonyPatch(typeof(GameAudioManager), "LoadSoundBanks")]
    internal class GameAudioManager_LoadSoundBanksPath
    {
        internal static void Postfix()
        {
            ModSoundBankManager.RegisterPendingSoundBanks();
        }
    }

    // Hinterland load assets by calling Resources.Load which ignores external AssetBundles
    // so we need to patch Resources.Load to redirect specific calls to load from the AssetBundle instead
    [HarmonyPatch(typeof(Resources), "Load", new Type[] { typeof(string) })]
    internal class Resources_Load
    {
        internal static bool Prefix(string path, ref UnityEngine.Object __result)
        {
            if (!ModAssetBundleManager.IsKnownAsset(path))
            {
                return true;
            }

            __result = ModAssetBundleManager.LoadAsset(path);
            return false;
        }
    }

    internal class SaveAtlas : MonoBehaviour
    {
        public UIAtlas original;
    }

    [HarmonyPatch(typeof(UISprite), "set_spriteName")]
    internal class UISprite_set_spriteName
    {
        internal static void Postfix(UISprite __instance, string value)
        {
            UIAtlas atlas = AssetUtils.GetRequiredAtlas(__instance, value);
            if (__instance.atlas == atlas)
            {
                return;
            }

            AssetUtils.SaveOriginalAtlas(__instance);
            __instance.atlas = atlas;
        }
    }
}
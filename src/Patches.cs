using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace AssetLoader
{
    [HarmonyPatch(typeof(GameAudioManager), "LoadSoundBanks")]
    internal class GameAudioManager_LoadSoundBanksPath
    {
        public static void Postfix()
        {
            ModSoundBankManager.RegisterPendingSoundBanks();
        }
    }

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

    internal class SaveAtlas : MonoBehaviour
    {
        public UIAtlas original;
    }

    [HarmonyPatch(typeof(UISprite), "set_spriteName")]
    internal class UISprite_set_spriteName
    {
        public static void Postfix(UISprite __instance, string value)
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

    [HarmonyPatch(typeof(Utils), "GetInventoryIconTextureFromName")]
    internal class UtilsGetInventoryIconTextureFromNamePatch
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return AssetUtils.PatchResourceLoading(instructions);
        }
    }

    [HarmonyPatch(typeof(ClothingSlot), "SetPaperDollTexture")]
    internal class ClothingSlot_SetPaperDollTexture
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return AssetUtils.PatchResourceLoading(instructions);
        }
    }

    [HarmonyPatch(typeof(Panel_Log), "UpdateSkillsPage")]
    public class Panel_Log_UpdateSkillsPage
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return AssetUtils.PatchResourceLoading(instructions);
        }
    }
}
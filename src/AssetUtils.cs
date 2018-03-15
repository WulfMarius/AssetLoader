using Harmony;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace AssetLoader
{
    public class AssetUtils
    {
        internal static UIAtlas GetRequiredAtlas(UISprite sprite, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return sprite.atlas;
            }

            UIAtlas atlas = ModAssetBundleManager.GetSpriteAtlas(value);
            if (atlas != null)
            {
                return atlas;
            }

            SaveAtlas restoreAtlas = sprite.gameObject.GetComponent<SaveAtlas>();
            if (restoreAtlas != null)
            {
                return restoreAtlas.original;
            }

            return sprite.atlas;
        }

        internal static bool IsUnpatchableResourceLoading(CodeInstruction codeInstruction)
        {
            if (codeInstruction.opcode != OpCodes.Call)
            {
                return false;
            }

            MethodInfo methodInfo = codeInstruction.operand as MethodInfo;
            if (methodInfo == null)
            {
                return false;
            }

            return methodInfo.Name == "Load" && methodInfo.DeclaringType == typeof(Resources) && methodInfo.GetParameters().Length > 1;
        }

        internal static void Log(string message)
        {
            Debug.LogFormat("[AssetLoader]: {0}", message);
        }

        internal static void Log(string message, params object[] parameters)
        {
            string preformattedMessage = string.Format("[AssetLoader]: {0}", message);
            Debug.LogFormat(preformattedMessage, parameters);
        }

        internal static void MakeResourceLoadingPatchable(List<CodeInstruction> instructions, int offset)
        {
            instructions[offset - 2].opcode = OpCodes.Nop;
            instructions[offset - 1].opcode = OpCodes.Nop;

            var methodCall = instructions[offset];
            MethodInfo originalMethodInfo = (MethodInfo)methodCall.operand;
            MethodInfo newMethodInfo = AccessTools.Method(originalMethodInfo.DeclaringType, "Load", new System.Type[] { typeof(string) });

            instructions[offset].operand = newMethodInfo;
        }

        internal static IEnumerable<CodeInstruction> PatchResourceLoading(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codeInstructions = new List<CodeInstruction>(instructions);
            PatchResourceLoading(codeInstructions);
            return instructions;
        }


        internal static void PatchResourceLoading(List<CodeInstruction> codeInstructions)
        {
            for (int i = 0; i < codeInstructions.Count; i++)
            {
                if (AssetUtils.IsUnpatchableResourceLoading(codeInstructions[i]))
                {
                    AssetUtils.MakeResourceLoadingPatchable(codeInstructions, i);
                }
            }
        }

        internal static void SaveOriginalAtlas(UISprite sprite)
        {
            SaveAtlas restoreAtlas = sprite.gameObject.GetComponent<SaveAtlas>();
            if (restoreAtlas == null)
            {
                restoreAtlas = sprite.gameObject.AddComponent<SaveAtlas>();
                restoreAtlas.original = sprite.atlas;
            }
        }
    }
}
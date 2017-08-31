using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace AssetLoader
{
    public class ModSoundBankManager
    {
        private static List<string> pendingRelativePaths = new List<string>();

        internal static bool DelayLoadingSoundBanks = true;

        public static void RegisterSoundBank(string relativePath)
        {
            string modDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string soundBankPath = Path.Combine(modDirectory, relativePath);
            if (!File.Exists(soundBankPath))
            {
                throw new FileNotFoundException("Sound bank '" + relativePath + "' could not be found at '" + soundBankPath + "'.");
            }

            if (DelayLoadingSoundBanks)
            {
                Log("Adding sound bank '{0}' to the list of pending sound banks.", relativePath);
                pendingRelativePaths.Add(relativePath);
                return;
            }

            LoadSoundBank(soundBankPath);
        }

        internal static void RegisterPendingSoundBanks()
        {
            Log("Registering pending sound banks.");

            foreach (string eachPendingRelativePath in pendingRelativePaths)
            {
                RegisterSoundBank(eachPendingRelativePath);
            }

            pendingRelativePaths.Clear();
        }

        private static void LoadSoundBank(string soundBankPath)
        {
            Log("Loading mod sound bank from '{0}'.", soundBankPath);
            byte[] soundBankData = File.ReadAllBytes(soundBankPath);

            uint bankID;
            IntPtr soundBankDataPointer = ToIntPtr(soundBankData);
            AKRESULT result = AkSoundEngine.LoadBank(soundBankDataPointer, (uint)soundBankData.Length, out bankID);

            if (result != AKRESULT.AK_Success)
            {
                Log("Failed to load sound bank from '{0}'. Result was {1}.", soundBankPath, result);
            }
        }

        private static IntPtr ToIntPtr(byte[] data)
        {
            IntPtr result = Marshal.AllocHGlobal(data.Length);

            Marshal.Copy(data, 0, result, data.Length);

            return result;
        }

        private static void Log(string message)
        {
            AssetUtils.Log("ModSoundBankManager", message);
        }

        private static void Log(string message, params object[] parameters)
        {
            AssetUtils.Log("ModSoundBankManager", message, parameters);
        }
    }
}

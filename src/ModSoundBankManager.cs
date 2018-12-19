using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

using static AssetLoader.Implementation;

namespace AssetLoader
{
    public class ModSoundBankManager
    {
        private const int MEMORY_ALIGNMENT = 16;

        private static bool DelayLoadingSoundBanks = true;
        private static List<string> pendingPaths = new List<string>();

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
                pendingPaths.Add(soundBankPath);
                return;
            }

            LoadSoundBank(soundBankPath);
        }

        internal static void RegisterPendingSoundBanks()
        {
            Log("Registering pending sound banks.");
            DelayLoadingSoundBanks = false;

            foreach (string eachPendingPath in pendingPaths)
            {
                LoadSoundBank(eachPendingPath);
            }

            pendingPaths.Clear();
        }

        private static void LoadSoundBank(string soundBankPath)
        {
            Log("Loading mod sound bank from '{0}'.", soundBankPath);
            byte[] data = File.ReadAllBytes(soundBankPath);

            // allocate memory and copy file contents to aligned address
            IntPtr allocated = Marshal.AllocHGlobal(data.Length + MEMORY_ALIGNMENT - 1);
            IntPtr aligned = new IntPtr((allocated.ToInt64() + MEMORY_ALIGNMENT - 1) / MEMORY_ALIGNMENT * MEMORY_ALIGNMENT);
            Marshal.Copy(data, 0, aligned, data.Length);

            uint bankID;
            var result = AkSoundEngine.LoadBank(aligned, (uint)data.Length, out bankID);
            if (result != AKRESULT.AK_Success)
            {
                Log("Failed to load sound bank from '{0}'. Result was {1}.", soundBankPath, result);
                Marshal.FreeHGlobal(allocated);
            }
        }
    }
}
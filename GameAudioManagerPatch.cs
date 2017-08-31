using Harmony;

namespace AssetLoader
{
    [HarmonyPatch(typeof(GameAudioManager), "LoadSoundBanks")]
    class GameAudioManagerLoadSoundBanksPath
    {
        public static void Postfix()
        {
            ModSoundBankManager.DelayLoadingSoundBanks = false;
            ModSoundBankManager.RegisterPendingSoundBanks();
        }
    }
}

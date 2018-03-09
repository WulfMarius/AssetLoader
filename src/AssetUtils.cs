using UnityEngine;

namespace AssetLoader
{
    public class AssetUtils
    {
        internal static void Log(string message)
        {
            Debug.LogFormat("[AssetLoader]: {0}", message);
        }

        internal static void Log(string message, params object[] parameters)
        {
            string preformattedMessage = string.Format("[AssetLoader]: {0}", message);
            Debug.LogFormat(preformattedMessage, parameters);
        }
    }
}

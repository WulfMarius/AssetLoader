using UnityEngine;

namespace AssetLoader
{
    public class Implementation
    {
        public static void OnLoad()
        {
            AssetUtils.Log("Version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
        }
    }
}
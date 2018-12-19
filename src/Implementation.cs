using UnityEngine;
using System.Reflection;

namespace AssetLoader
{
    public class Implementation
    {
        private static string name;

        public static void OnLoad()
        {
            AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();
            name = assemblyName.Name;

            Log("Version " + assemblyName.Version);
        }

        internal static void Log(string message)
        {
            string preformattedMessage = string.Format("[0] {1}", name, message);
            Debug.LogFormat("[{0}] {1}", name, message);
        }

        internal static void Log(string message, params object[] parameters)
        {
            string preformattedMessage = string.Format("[{0}] {1}", name, message);
            Debug.LogFormat(preformattedMessage, parameters);
        }
    }
}
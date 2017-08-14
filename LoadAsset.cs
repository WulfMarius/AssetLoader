using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using UnityEngine;

namespace ModAsset
{
    public class LoadAsset
    {
        public static void OnLoad()
        {
            uConsole.RegisterCommand("prybar", new uConsole.DebugCommand(SpawnPrybar));
            uConsole.RegisterCommand("chair", new uConsole.DebugCommand(SpawnChair));
            uConsole.RegisterCommand("binoculars", new uConsole.DebugCommand(SpawnBinoculars));
        }

        private static void SpawnPrybar()
        {
            UnityEngine.Object prefab = Resources.Load("GEAR_Prybar");
            Debug.Log("Prefab: " + prefab);

            Transform playerTransform = GameManager.GetPlayerTransform();
            Vector3 targetPosition = playerTransform.position + playerTransform.forward * 2;

            instantiatePrefab((GameObject)prefab, targetPosition);
        }

        private static void SpawnBinoculars()
        {
            UnityEngine.Object prefab = Resources.Load(ModAssetBundleManager.MOD_PREFIX + "binoculars/assets/prefabs/MOD_GEAR_binoculars.prefab");
            Debug.Log("Prefab: " + prefab);

            Transform playerTransform = GameManager.GetPlayerTransform();
            Vector3 targetPosition = playerTransform.position + playerTransform.forward * 2;

            instantiatePrefab((GameObject)prefab, targetPosition);
        }

        private static void SpawnChair()
        {
            UnityEngine.Object prefab = Resources.Load(ModAssetBundleManager.MOD_PREFIX + "leather_chair/assets/prefabs/leather_chair.prefab");
            Debug.Log("Prefab: " + prefab);

            Transform playerTransform = GameManager.GetPlayerTransform();
            Vector3 targetPosition = playerTransform.position + playerTransform.forward * 2;

            instantiatePrefab((GameObject)prefab, targetPosition);
        }

        private static void instantiatePrefab(GameObject prefab, Vector3 targetPosition)
        {
            GameObject gameObject = UnityEngine.Object.Instantiate(prefab, targetPosition, Quaternion.identity);
            gameObject.name = prefab.name;

            Debug.Log("Created game object " + gameObject + ", layer " + gameObject.layer);
            foreach (Component eachComponent in gameObject.GetComponents<Component>())
            {
                Debug.Log("  with component " + eachComponent);
            }

            StickToGroundAndOrientOnSlope(gameObject.transform, gameObject.transform.position);
        }

        private static bool StickToGroundAndOrientOnSlope(Transform modifiedTransform, Vector3 desiredPosition)
        {
            RaycastHit hitInfo;
            if (!Physics.Raycast(desiredPosition, Vector3.down, out hitInfo, float.PositiveInfinity, Utils.m_PhysicalCollisionLayerMask))
            {
                return false;
            }

            modifiedTransform.position = hitInfo.point;
            modifiedTransform.rotation = Quaternion.identity;
            modifiedTransform.rotation = Utils.GetOrientationOnSlope(GameManager.GetPlayerManagerComponent().transform, hitInfo.normal);
            return true;
        }
    }
}

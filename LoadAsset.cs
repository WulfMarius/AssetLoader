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
            uConsole.RegisterCommand("chair", new uConsole.DebugCommand(SpawnChair));
            uConsole.RegisterCommand("binoculars", new uConsole.DebugCommand(SpawnBinoculars));
        }

        private static void SpawnBinoculars()
        {
            UnityEngine.Object prefab = Resources.Load(ModAssetBundleManager.MOD_PREFIX + "binoculars/assets/prefabs/binoculars.prefab");
            Debug.Log("Prefab: " + prefab);

            Transform playerTransform = GameManager.GetPlayerTransform();
            Vector3 targetPosition = playerTransform.position + playerTransform.forward * 2;

            GameObject leatherChair = UnityEngine.Object.Instantiate((GameObject)prefab, targetPosition, Quaternion.identity);
            StickToGroundAndOrientOnSlope(leatherChair.transform, leatherChair.transform.position);
        }

        private static void SpawnChair()
        {
            UnityEngine.Object prefab = Resources.Load(ModAssetBundleManager.MOD_PREFIX + "leather_chair/assets/prefabs/leather_chair.prefab");
            Debug.Log("Prefab: " + prefab);

            Transform playerTransform = GameManager.GetPlayerTransform();
            Vector3 targetPosition = playerTransform.position + playerTransform.forward * 2;

            GameObject leatherChair = UnityEngine.Object.Instantiate((GameObject)prefab, targetPosition, Quaternion.identity);
            StickToGroundAndOrientOnSlope(leatherChair.transform, leatherChair.transform.position);
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

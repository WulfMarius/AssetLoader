using UnityEngine;

namespace AssetLoader
{
    public class AssetUtils
    {
        public static GameObject instantiatePrefab(GameObject prefab, Vector3 targetPosition)
        {
            GameObject gameObject = UnityEngine.Object.Instantiate(prefab, targetPosition, Quaternion.identity);
            gameObject.name = prefab.name;

            StickToGroundAndOrientOnSlope(gameObject.transform, gameObject.transform.position);

            return gameObject;
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

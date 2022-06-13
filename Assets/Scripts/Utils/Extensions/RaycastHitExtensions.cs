using System.Runtime.CompilerServices;

namespace UnityEngine
{
    public static class RaycastHitExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ColliderName(this RaycastHit hit)
        {
            return hit.collider ? hit.collider.name : "NULL";
        }

        public static bool RecalculateNormalForMeshCollider(this RaycastHit hit, out Vector3 normal)
        {
            if (hit.collider && hit.collider is MeshCollider)
            {
                var collider = hit.collider as MeshCollider;
                var mesh = collider.sharedMesh;
                if (mesh.isReadable)
                {
                    var triangles = mesh.triangles;
                    var vertices = mesh.vertices;

                    var v0 = vertices[triangles[hit.triangleIndex * 3]];
                    var v1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
                    var v2 = vertices[triangles[hit.triangleIndex * 3 + 2]];

                    var n = Vector3.Cross(v1 - v0, v2 - v1).normalized;

                    normal = hit.transform.TransformDirection(n);
                    return true;
                }
            }

            normal = hit.normal;
            return false;
        }

        public static bool RecalculateNormalUsingRaycast(this RaycastHit hit, out Vector3 normal, int layerMask, QueryTriggerInteraction triggerQuery)
        {
            if (hit.collider != null)
            {
                var rayOrigin = hit.point + hit.normal * .01f; ;
                Physics.Raycast(rayOrigin, -hit.normal, out var rayHit, 0.011f, layerMask, triggerQuery);

                if (rayHit.collider == hit.collider)
                {
                    normal = rayHit.normal;
                    return true;
                }
            }

            normal = hit.normal;
            return false;
        }

        public static bool RecalculateNormal(this RaycastHit hit, out Vector3 normal, int layerMask, QueryTriggerInteraction triggerQuery)
        {
            if (hit.collider)
            {
                if (RecalculateNormalForMeshCollider(hit, out normal))
                {
                    return true;
                }

                if (RecalculateNormalUsingRaycast(hit, out normal, layerMask, triggerQuery))
                {
                    return true;
                }
            }

            normal = hit.normal;
            return false;
        }
    }
}
using System;
using UnityEngine;

public class CharacterCapsule : MonoBehaviour
{
    public const int DefaultRaycastLayerMask = Physics.DefaultRaycastLayers;
    public const QueryTriggerInteraction DefaultQueryTriggerInteraction = QueryTriggerInteraction.Ignore;
    public const float DefaultMoveThreshold = 0.001f;
    public const float DefaultSkinWidth = 0f;

    public CapsuleCollider CapsuleCollider { get => _CapsuleCollider; }
    [SerializeField] protected CapsuleCollider _CapsuleCollider;

    //////////////////////////////////////////////////////////////////
    /// UpdateLoop
    //////////////////////////////////////////////////////////////////

    public void Init(Character character)
    {
    }

    public void UpdateImpl()
    {
    }

    //////////////////////////////////////////////////////////////////
    /// Geometry
    //////////////////////////////////////////////////////////////////

    public bool IsSphereShaped
    {
        get
        {
            return GetWorldHeight <= GetWorldRadius * 2;
        }
    }

    /// Lengths (WorldSpace)
    public float GetWorldRadius
    {
        get
        {
            Vector3 worldScale = _CapsuleCollider.transform.lossyScale;
            return _CapsuleCollider.radius * Mathf.Max(worldScale.x, worldScale.z);
        }
    }

    public float GetWorldHeight
    {
        get
        {
            return _CapsuleCollider.height * _CapsuleCollider.transform.lossyScale.y;
        }
    }

    public float GetWorldCylinderHeight
    {
        get
        {
            float height = GetWorldHeight;
            float tradius = GetWorldRadius * 2;

            return height > tradius ? height - tradius : 0;
        }
    }

    public float GetWorldHalfCylinderHeight
    {
        get
        {
            return GetWorldCylinderHeight / 2;
        }
    }

    /// Volume (WorldSpace)
    public float GetWorldSphereVolume
    {
        get
        {
            return (float)Math.PI * (float)Math.Pow(GetWorldRadius, 2);
        }
    }

    public float GetWorldHalfSphereVolume
    {
        get
        {
            return GetWorldSphereVolume / 2;
        }
    }

    public float GetWorldCylinderVolume
    {
        get
        {
            return 2 * (float)Math.PI * GetWorldRadius * GetWorldCylinderHeight;
        }
    }

    public float GetWorldVolume
    {
        get
        {
            return GetWorldSphereVolume + GetWorldCylinderVolume;
        }
    }

    /// Positions (WorldSpace)
    public Vector3 GetWorldCenter
    {
        get
        {
            return _CapsuleCollider.transform.position + _CapsuleCollider.center;
        }
    }

    public Vector3 GetWorldTopSpherePosition
    {
        get
        {
            return GetWorldCenter + (GetWorldUpVector * GetWorldHalfCylinderHeight);
        }
    }

    public Vector3 GetWorldBaseSpherePosition
    {
        get
        {
            return GetWorldCenter + (-GetWorldUpVector * GetWorldHalfCylinderHeight);
        }
    }

    public Vector3 GetWorldTopPosition
    {
        get
        {
            return GetWorldCenter + (GetWorldUpVector * (GetWorldHalfCylinderHeight + GetWorldRadius));
        }
    }

    public Vector3 GetWorldBasePosition
    {
        get
        {
            return GetWorldCenter + (-GetWorldUpVector * (GetWorldHalfCylinderHeight + GetWorldRadius));
        }
    }

    /// Directions (WorldSpace)
    public Vector3 GetWorldForwardVector
    {
        get
        {
            return _CapsuleCollider.transform.forward;
        }
    }

    public Vector3 GetWorldRightVector
    {
        get
        {
            return _CapsuleCollider.transform.right;
        }
    }

    public Vector3 GetWorldUpVector
    {
        get
        {
            return _CapsuleCollider.transform.up;
        }
    }

    public void CalculateWorldGeometryValues(out Vector3 startSphere, out Vector3 endSphere, out float radius)
    {
        Vector3 worldCenter = _CapsuleCollider.transform.position + _CapsuleCollider.center;
        Vector3 worldScale = _CapsuleCollider.transform.lossyScale;

        float worldHeight = _CapsuleCollider.height * _CapsuleCollider.transform.lossyScale.y;
        float worldRadius = _CapsuleCollider.radius * Mathf.Max(worldScale.x, worldScale.z);
        float cylinderHeight = worldHeight - worldRadius - worldRadius;

        startSphere = worldCenter + (_CapsuleCollider.transform.up * (cylinderHeight / 2));
        endSphere = worldCenter + (-_CapsuleCollider.transform.up * (cylinderHeight / 2));
        radius = _CapsuleCollider.radius;
    }

    //////////////////////////////////////////////////////////////////
    /// Physics
    //////////////////////////////////////////////////////////////////
    
    public Vector3 CapsulePosition
    {
        get
        {
            return _CapsuleCollider.transform.position;
        }

        set
        {
            _CapsuleCollider.transform.position = value;
        }
    }

    public void MoveToPosition(Vector3 pos)
    {
        _CapsuleCollider.transform.position = pos;
    }

    public void MovePosition(Vector3 pos)
    {
        _CapsuleCollider.transform.position += pos;
    }

    public Collider[] CapsuleOverlap(Vector3 move, float skinWidth = DefaultSkinWidth, int layerMask = DefaultRaycastLayerMask, QueryTriggerInteraction queryTriggerInteraction = DefaultQueryTriggerInteraction)
    {
        CalculateWorldGeometryValues(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.OverlapCapsule(topSphere, baseSphere, radius + skinWidth, layerMask, queryTriggerInteraction);
    }

    public RaycastHit CapsuleCast(Vector3 move, float skinWidth = DefaultSkinWidth, int layerMask = DefaultRaycastLayerMask, QueryTriggerInteraction queryTriggerInteraction = DefaultQueryTriggerInteraction)
    {
        CalculateWorldGeometryValues(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        Physics.CapsuleCast(topSphere, baseSphere, radius + skinWidth, move.normalized, out RaycastHit hit, move.magnitude, layerMask, queryTriggerInteraction);
        return hit;
    }

    public RaycastHit[] CapsuleCastAll(Vector3 move, float skinWidth = DefaultSkinWidth, int layerMask = DefaultRaycastLayerMask, QueryTriggerInteraction queryTriggerInteraction = DefaultQueryTriggerInteraction)
    {
        CalculateWorldGeometryValues(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.CapsuleCastAll(topSphere, baseSphere, radius + skinWidth, move.normalized, move.magnitude, layerMask, queryTriggerInteraction);
    }

    public Collider[] TopSphereOverlap(Vector3 move, float skinWidth = DefaultSkinWidth, int layerMask = DefaultRaycastLayerMask, QueryTriggerInteraction queryTriggerInteraction = DefaultQueryTriggerInteraction)
    {
        CalculateWorldGeometryValues(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.OverlapSphere(topSphere, radius + skinWidth, layerMask, queryTriggerInteraction);
    }

    public RaycastHit TopSphereCast(Vector3 move, float skinWidth = DefaultSkinWidth, int layerMask = DefaultRaycastLayerMask, QueryTriggerInteraction queryTriggerInteraction = DefaultQueryTriggerInteraction)
    {
        CalculateWorldGeometryValues(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        Physics.SphereCast(topSphere, radius + skinWidth, move.normalized, out RaycastHit hit, move.magnitude, layerMask, queryTriggerInteraction);
        return hit;
    }

    public RaycastHit[] TopSphereCastAll(Vector3 move, float skinWidth = DefaultSkinWidth, int layerMask = DefaultRaycastLayerMask, QueryTriggerInteraction queryTriggerInteraction = DefaultQueryTriggerInteraction)
    {
        CalculateWorldGeometryValues(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.SphereCastAll(topSphere, radius + skinWidth, move.normalized, move.magnitude, layerMask, queryTriggerInteraction);
    }

    public Collider[] BaseSphereOverlap(Vector3 move, float skinWidth = DefaultSkinWidth, int layerMask = DefaultRaycastLayerMask, QueryTriggerInteraction queryTriggerInteraction = DefaultQueryTriggerInteraction)
    {
        CalculateWorldGeometryValues(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.OverlapSphere(baseSphere, radius + skinWidth, layerMask, queryTriggerInteraction);
    }

    public RaycastHit BaseSphereCast(Vector3 move, float skinWidth = DefaultSkinWidth, int layerMask = DefaultRaycastLayerMask, QueryTriggerInteraction queryTriggerInteraction = DefaultQueryTriggerInteraction)
    {
        CalculateWorldGeometryValues(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        Physics.SphereCast(baseSphere, radius + skinWidth, move.normalized, out RaycastHit hit, move.magnitude, layerMask, queryTriggerInteraction);
        return hit;
    }

    public RaycastHit[] BaseSphereCastAll(Vector3 move, float skinWidth = DefaultSkinWidth, int layerMask = DefaultRaycastLayerMask, QueryTriggerInteraction queryTriggerInteraction = DefaultQueryTriggerInteraction)
    {
        CalculateWorldGeometryValues(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.SphereCastAll(baseSphere, radius + skinWidth, move.normalized, move.magnitude, layerMask, queryTriggerInteraction);
    }

    public RaycastHit CapsuleMove(Vector3 move, float skinWidth = DefaultSkinWidth, float moveThreshold = DefaultMoveThreshold, int layerMask = DefaultRaycastLayerMask, QueryTriggerInteraction queryTriggerInteraction = DefaultQueryTriggerInteraction)
    {
        RaycastHit hit = CapsuleCast(move);
        if (hit.collider == null)
        {
            // no collision occurred, so we made the complete move
            MovePosition(move);
            return hit;
        }

        // move to the hit position
        Vector3 moveDir = move.normalized;
        if (hit.distance < moveThreshold)
        {
            return hit;
        }

        MovePosition(moveDir * hit.distance);
        return hit;
    }

    public RaycastHit CapsuleMoveNoHit(Vector3 move, float skinWidth = DefaultSkinWidth, float moveThreshold = DefaultMoveThreshold, int layerMask = DefaultRaycastLayerMask, QueryTriggerInteraction queryTriggerInteraction = DefaultQueryTriggerInteraction)
    {
        RaycastHit hit = CapsuleCast(move);
        if (hit.collider == null)
        {
            // no collision occurred, so we made the complete move
            MovePosition(move);
        }

        return hit;
    }

    //////////////////////////////////////////////////////////////////
    /// Gizmos
    //////////////////////////////////////////////////////////////////

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(GetWorldTopSpherePosition, GetWorldRadius);
        Gizmos.DrawWireSphere(GetWorldBaseSpherePosition, GetWorldRadius);
    }
}
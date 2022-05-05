using System;
using UnityEngine;

[Serializable]
public struct VirtualCapsule
{
    //////////////////////////////////////////////////////////////////
    // Variables
    //////////////////////////////////////////////////////////////////
    [SerializeField] public Vector3 Position;
    [SerializeField] public Quaternion Rotation;
    [SerializeField] public Vector3 Scale;
    [SerializeField] public Vector3 Center;
    [SerializeField] public int Direction;
    [SerializeField] public float Height;
    [SerializeField] public float Radius;
    [SerializeField] public float SkinWidth;
    [SerializeField] public LayerMask LayerMask;
    [SerializeField] public QueryTriggerInteraction TriggerQuery;

    //////////////////////////////////////////////////////////////////
    // Constructors
    //////////////////////////////////////////////////////////////////
    public VirtualCapsule(Vector3 position, Quaternion rotation)
    {
        Position = position;
        Rotation = rotation;
        Scale = Vector3.one;
        Center = Vector3.zero;
        Direction = 0;
        Height = 2;
        Radius = 0.5f;
        SkinWidth = 0f;
        LayerMask = Physics.DefaultRaycastLayers;
        TriggerQuery = QueryTriggerInteraction.Ignore;
    }

    public VirtualCapsule(ColliderCapsule colliderCapsule)
    {
        Position = colliderCapsule.Position;
        Rotation = colliderCapsule.Rotation;
        Scale = colliderCapsule.Scale;
        Center = colliderCapsule.Center;
        Direction = colliderCapsule.Direction;
        Height = colliderCapsule.Height;
        Radius = colliderCapsule.Radius;
        SkinWidth = colliderCapsule.SkinWidth;
        LayerMask = colliderCapsule.LayerMask;
        TriggerQuery = colliderCapsule.TriggerQuery;
    }

    //////////////////////////////////////////////////////////////////
    /// Geometry
    //////////////////////////////////////////////////////////////////

    /// Checks if Capsule is in Sphere shaped in Space
    public bool IsSphereShaped
    {
        get
        {
            return GetHeight <= GetRadius * 2;
        }
    }

    /// Lengths (Space && LocalSpace)
    public float GetRadius
    {
        get
        {
            return Radius * Mathf.Max(Scale.x, Scale.z);
        }
    }
    public float GetHeight
    {
        get
        {
            return Height * Scale.y;
        }
    }
    public float GetCylinderHeight
    {
        get
        {
            float height = GetHeight;
            float tradius = GetRadius * 2;

            return height > tradius ? height - tradius : 0;
        }
    }
    public float GetHalfCylinderHeight
    {
        get
        {
            return GetCylinderHeight / 2;
        }
    }

    /// Rotations (Space)
    public Quaternion GetRotation
    {
        get
        {
            return Rotation;
        }
    }
    public Vector3 GetRotationEuler
    {
        get
        {
            return Rotation.eulerAngles;
        }
    }

    public Vector3 GetForwardVector
    {
        get
        {
            return GetRotation * Vector3.forward;
        }
    }
    public Vector3 GetBackVector
    {
        get
        {
            return GetRotation * Vector3.back;
        }
    }
    public Vector3 GetLetVector
    {
        get
        {
            return GetRotation * Vector3.left;
        }
    }
    public Vector3 GetRightVector
    {
        get
        {
            return GetRotation * Vector3.right;
        }
    }
    public Vector3 GetUpVector
    {
        get
        {
            return GetRotation * Vector3.up;
        }
    }
    public Vector3 GetDownVector
    {
        get
        {
            return GetRotation * Vector3.down;
        }
    }

    /// Positions (Space && LocalSpace)
    public Vector3 GetPosition
    {
        get
        {
            return Position;
        }
    }
    public Vector3 GetCenter
    {
        get
        {
            return GetPosition + Center;
        }
    }
    public Vector3 GetTopSpherePosition
    {
        get
        {
            return GetCenter + (GetUpVector * GetHalfCylinderHeight);
        }
    }
    public Vector3 GetBaseSpherePosition
    {
        get
        {
            return GetCenter + (GetDownVector * GetHalfCylinderHeight);
        }
    }
    public Vector3 GetTopPosition
    {
        get
        {
            return GetCenter + (GetUpVector * (GetHalfCylinderHeight + GetRadius));
        }
    }
    public Vector3 GetBasePosition
    {
        get
        {
            return GetCenter + (GetDownVector * (GetHalfCylinderHeight + GetRadius));
        }
    }

    /// Volume (Space && LocalSpace)
    public float GetSphereVolume
    {
        get
        {
            return (float)Math.PI * (float)Math.Pow(GetRadius, 2);
        }
    }
    public float GetHalfSphereVolume
    {
        get
        {
            return GetSphereVolume / 2;
        }
    }
    public float GetCylinderVolume
    {
        get
        {
            return 2 * (float)Math.PI * GetRadius * GetCylinderHeight;
        }
    }
    public float GetVolume
    {
        get
        {
            return GetSphereVolume + GetCylinderVolume;
        }
    }

    /// Calculate Geometry (Space)
    public void CalculateGeometryValues(out Vector3 startSphere, out Vector3 endSphere, out float radius)
    {
        Vector3 worldCenter = GetPosition + GetCenter;
        Vector3 worldScale = Scale;

        float worldHeight = GetHeight * worldScale.y;
        float worldRadius = GetRadius * Mathf.Max(worldScale.x, worldScale.z) + SkinWidth;
        float cylinderHeight = worldHeight - worldRadius - worldRadius;

        startSphere = worldCenter + (GetUpVector * (cylinderHeight / 2));
        endSphere = worldCenter + (GetDownVector * (cylinderHeight / 2));
        radius = GetRadius * Mathf.Max(worldScale.x, worldScale.z);
    }

    //////////////////////////////////////////////////////////////////
    /// Physics
    //////////////////////////////////////////////////////////////////

    public void Move(Vector3 pos)
    {
        Position += pos;
    }

    public void Rotate(Vector3 rot)
    {
        Rotation = Rotation * Quaternion.Euler(rot);
    }

    public Collider[] CapsuleOverlap()
    {
        CalculateGeometryValues(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.OverlapCapsule(topSphere, baseSphere, radius, LayerMask, TriggerQuery);
    }

    public uint CapsuleOverlapNonAlloc(Collider[] colliders)
    {
        if (colliders == null || colliders.Length == 0)
            return 0;

        CalculateGeometryValues(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.OverlapCapsuleNonAlloc(topSphere, baseSphere, radius, colliders, LayerMask, TriggerQuery);
    }

    public RaycastHit CapsuleCast(Vector3 move)
    {
        CalculateGeometryValues(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        Physics.CapsuleCast(topSphere, baseSphere, radius, move.normalized, out RaycastHit hit, move.magnitude, LayerMask, TriggerQuery);
        return hit;
    }

    public RaycastHit[] CapsuleCastAll(Vector3 move)
    {
        CalculateGeometryValues(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.CapsuleCastAll(topSphere, baseSphere, radius, move.normalized, move.magnitude, LayerMask, TriggerQuery);
    }

    public uint CapsuleCastAllNonAlloc(RaycastHit[] hitResults, Vector3 move)
    {
        if (hitResults == null || hitResults.Length == 0)
            return 0;

        CalculateGeometryValues(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.CapsuleCastNonAlloc(topSphere, baseSphere, radius, move.normalized, hitResults, move.magnitude, LayerMask, TriggerQuery);
    }

    public Collider[] TopSphereOverlap()
    {
        CalculateGeometryValues(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.OverlapSphere(topSphere, radius, LayerMask, TriggerQuery);
    }

    public uint TopSphereOverlapNonAlloc(Collider[] colliders)
    {
        if (colliders == null || colliders.Length == 0)
            return 0;

        CalculateGeometryValues(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.OverlapSphereNonAlloc(topSphere, radius, colliders, LayerMask, TriggerQuery);
    }

    public RaycastHit TopSphereCast(Vector3 move)
    {
        CalculateGeometryValues(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        Physics.SphereCast(topSphere, radius, move.normalized, out RaycastHit hit, move.magnitude, LayerMask, TriggerQuery);
        return hit;
    }

    public RaycastHit[] TopSphereCastAll(Vector3 move)
    {
        CalculateGeometryValues(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.SphereCastAll(topSphere, radius, move.normalized, move.magnitude, LayerMask, TriggerQuery);
    }

    public uint TopSphereCastAllNonAlloc(RaycastHit[] hitResults, Vector3 move)
    {
        if (hitResults == null || hitResults.Length == 0)
            return 0;

        CalculateGeometryValues(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.SphereCastNonAlloc(topSphere, radius, move.normalized, hitResults, move.magnitude, LayerMask, TriggerQuery);
    }

    public Collider[] BaseSphereOverlap()
    {
        CalculateGeometryValues(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.OverlapSphere(baseSphere, radius, LayerMask, TriggerQuery);
    }

    public uint BaseSphereOverlapNonAlloc(Collider[] colliders)
    {
        if (colliders == null || colliders.Length == 0)
            return 0;

        CalculateGeometryValues(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.OverlapSphereNonAlloc(baseSphere, radius, colliders, LayerMask, TriggerQuery);
    }

    public RaycastHit BaseSphereCast(Vector3 move)
    {
        CalculateGeometryValues(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        Physics.SphereCast(baseSphere, radius, move.normalized, out RaycastHit hit, move.magnitude, LayerMask, TriggerQuery);
        return hit;
    }

    public RaycastHit[] BaseSphereCastAll(Vector3 move)
    {
        CalculateGeometryValues(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.SphereCastAll(baseSphere, radius, move.normalized, move.magnitude, LayerMask, TriggerQuery);
    }

    public uint BaseSphereCastAllNonAlloc(RaycastHit[] hitResults, Vector3 move)
    {
        if (hitResults == null || hitResults.Length == 0)
            return 0;

        CalculateGeometryValues(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.SphereCastNonAlloc(baseSphere, radius, move.normalized, hitResults, move.magnitude, LayerMask, TriggerQuery);
    }

    public RaycastHit CapsuleMove(Vector3 move, float moveThreshold)
    {
        RaycastHit hit = CapsuleCast(move);
        if (hit.collider == null)
        {
            // no collision occurred, so we made the complete move
            Move(move);
            return hit;
        }

        // move to the hit position
        Vector3 moveDir = move.normalized;
        if (hit.distance < moveThreshold)
        {
            return hit;
        }

        Move(moveDir.normalized * hit.distance);
        return hit;
    }

    public RaycastHit CapsuleMoveNoHit(Vector3 move, float moveThreshold)
    {
        RaycastHit hit = CapsuleCast(move);
        if (hit.collider == null)
        {
            // no collision occurred, so we made the complete move
            Move(move);
        }

        return hit;
    }

    //////////////////////////////////////////////////////////////////
    // Conversions
    //////////////////////////////////////////////////////////////////
    public void FromColliderCapsule(ColliderCapsule colliderCapsule)
    {
        Position = colliderCapsule.Position;
        Rotation = colliderCapsule.Rotation;
        Scale = colliderCapsule.Scale;
        Center = colliderCapsule.Center;
        Direction = colliderCapsule.Direction;
        Height = colliderCapsule.Height;
        Radius = colliderCapsule.Radius;
        SkinWidth = colliderCapsule.SkinWidth;
        LayerMask = colliderCapsule.LayerMask;
        TriggerQuery = colliderCapsule.TriggerQuery;
    }
}
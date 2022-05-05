using System;
using UnityEngine;

[Serializable]
public struct ColliderCapsule
{
    //////////////////////////////////////////////////////////////////
    // Variables
    //////////////////////////////////////////////////////////////////
    [SerializeField] public CapsuleCollider Capsule;
    [SerializeField] public float SkinWidth;
    [SerializeField] public LayerMask LayerMask;
    [SerializeField] public QueryTriggerInteraction TriggerQuery;

    public Vector3 Position
    {
        get => Capsule.transform.position;
        set => Capsule.transform.position = value;
    }
    public Quaternion Rotation
    {
        get => Capsule.transform.rotation;
        set => Capsule.transform.rotation = value;
    }
    public Vector3 Scale
    {
        get => Capsule.transform.lossyScale;
        set => Capsule.transform.SetWorldScale(value);
    }

    public Vector3 LocalPosition
    {
        get => Capsule.transform.localPosition;
        set => Capsule.transform.localPosition = value;
    }
    public Quaternion LocalRotation
    {
        get => Capsule.transform.localRotation;
        set => Capsule.transform.localRotation = value;
    }
    public Vector3 LocalScale
    {
        get => Capsule.transform.localScale;
        set => Capsule.transform.localScale = value;
    }

    public Vector3 Center
    {
        get => Capsule.center;
        set => Capsule.center = value;
    }
    public int Direction
    {
        get => Capsule.direction;
        set => Capsule.direction = value;
    }
    public float Height
    {
        get => Capsule.height;
        set => Capsule.height = value;
    }
    public float Radius
    {
        get => Capsule.radius;
        set => Capsule.radius = value;
    }

    //////////////////////////////////////////////////////////////////
    // Constructors
    //////////////////////////////////////////////////////////////////
    public ColliderCapsule(CapsuleCollider collider)
    {
        Capsule = collider;
        SkinWidth = 0f;
        LayerMask = Physics.DefaultRaycastLayers;
        TriggerQuery = QueryTriggerInteraction.Ignore;
    }

    public ColliderCapsule(CapsuleCollider collider, VirtualCapsule virtualCapsule)
    {
        Capsule = collider;
        SkinWidth = virtualCapsule.SkinWidth;
        LayerMask = virtualCapsule.LayerMask;
        TriggerQuery = virtualCapsule.TriggerQuery;

        Position = virtualCapsule.Position;
        Rotation = virtualCapsule.Rotation;
        Scale = virtualCapsule.Scale;
        Center = virtualCapsule.Center;
        Direction = virtualCapsule.Direction;
        Height = virtualCapsule.Height;
        Radius = virtualCapsule.Radius;
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
            return GetPosition + GetCenter;
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

    /// <summary>
    /// Compute the minimal translation required to separate the character from the collider.
    /// </summary>
    /// <param name="move">Minimal move required to separate the colliders apart.</param>
    /// <param name="collider">The collider to test.</param>
    /// <param name="colliderPosition">Position of the collider.</param>
    /// <param name="colliderRotation">Rotation of the collider.</param>
    /// <returns>True if found penetration.</returns>
    public bool ComputePenetration(out Vector3 move, Collider collider, Vector3 colliderPosition, Quaternion colliderRotation)
    {
        if (Capsule == null || collider == null || collider == Capsule)
        {
            // Ignore self
            move = Vector3.zero;
            return false;
        }

        float cacheRadius = Capsule.radius + SkinWidth;
        float cacheHeight = Capsule.height;

        Capsule.radius = cacheRadius + SkinWidth;
        Capsule.height = cacheHeight;

        // Note: Physics.ComputePenetration does not always return values when the colliders overlap.
        var result = Physics.ComputePenetration(Capsule, Capsule.transform.position,
            Capsule.transform.rotation, collider, colliderPosition, colliderRotation,
            out Vector3 direction, out float distance);

        Capsule.radius = cacheRadius;
        Capsule.height = cacheHeight;

        move = direction * distance;
        return result;
    }

    public Vector3 ResolvePenetration(float k_CollisionOffset = 0.001f)
    {
        Collider[] overlaps = CapsuleOverlap();
        if (overlaps.Length <= 0)
        {
            return Vector3.zero;
        }

        Vector3 moveOut = Vector3.zero;
        foreach (var collider in overlaps)
        {
            if (collider == null)
            {
                break;
            }

            if (ComputePenetration(out Vector3 capsuleMoveOut, collider, collider.transform.position, collider.transform.rotation))
            {
                moveOut += capsuleMoveOut + (capsuleMoveOut.normalized * k_CollisionOffset);
            }
        }

        Move(moveOut);
        return moveOut;
    }

    //////////////////////////////////////////////////////////////////
    // Conversions
    //////////////////////////////////////////////////////////////////
    public void FromVirtualCapsule(VirtualCapsule virtualCapsule)
    {
        Position = virtualCapsule.Position;
        Rotation = virtualCapsule.Rotation;
        Scale = virtualCapsule.Scale;
        Center = virtualCapsule.Center;
        Direction = virtualCapsule.Direction;
        Height = virtualCapsule.Height;
        Radius = virtualCapsule.Radius;
        SkinWidth = virtualCapsule.SkinWidth;
        LayerMask = virtualCapsule.LayerMask;
        TriggerQuery = virtualCapsule.TriggerQuery;
    }
}
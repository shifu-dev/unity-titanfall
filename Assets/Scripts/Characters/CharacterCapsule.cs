using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCapsule : MonoBehaviour
{
    //////////////////////////////////////////////////////////////////
    // Variables
    //////////////////////////////////////////////////////////////////
    [SerializeField] public CapsuleCollider CapsuleCollider = null;
    [SerializeField] public Vector3 Position = Vector3.zero;
    [SerializeField] public Quaternion Rotation = Quaternion.identity;
    [SerializeField] public Vector3 Scale = Vector3.one;
    [SerializeField] public Vector3 Center = Vector3.zero;
    [SerializeField] public int Direction = 1;
    [SerializeField] public float Height = 2;
    [SerializeField] public float Radius = 0.5f;
    [SerializeField] public float SkinWidth = 0.01f;
    [SerializeField] public LayerMask LayerMask = Physics.DefaultRaycastLayers;
    [SerializeField] public QueryTriggerInteraction TriggerQuery = QueryTriggerInteraction.Ignore;

    public CapsuleCollider Capsule => CapsuleCollider;
    public Vector3 LastPosition { get; protected set; }
    public Quaternion LastRotation { get; protected set; }
    public Vector3 Velocity { get; protected set; }
    public float Speed => Velocity.magnitude;

    //////////////////////////////////////////////////////////////////
    /// UpdateLoop
    //////////////////////////////////////////////////////////////////

    public void Init(Character character)
    {
        Position = transform.position;
        Rotation = CapsuleCollider.transform.rotation;
    }

    public void UpdateImpl()
    {
        PerformMove();
    }

    public void PerformMove()
    {
        // store previous values
        LastPosition = transform.position;
        LastRotation = CapsuleCollider.transform.rotation;

        // set new values
        transform.position = Position;
        CapsuleCollider.transform.rotation = Rotation;
        CapsuleCollider.transform.localScale = Scale;
        CapsuleCollider.direction = Direction;
        CapsuleCollider.center = Center;
        CapsuleCollider.height = Height;
        CapsuleCollider.radius = Radius;

        // calculate velocity
        Velocity = (Position - LastPosition) / Time.deltaTime;
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
    public void CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius)
    {
        InternalCalculateCapsuleGeometry(out topSphere, out baseSphere, out radius, 0f);
    }

    public void CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius)
    {
        InternalCalculateCapsuleGeometry(out topSphere, out baseSphere, out radius, SkinWidth);
    }

    protected virtual void InternalCalculateCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius, float skinWidth)
    {
        Vector3 worldCenter = GetCenter;
        Vector3 worldScale = Scale;

        float worldHeight = Height * worldScale.y;
        float worldRadius = Radius * Mathf.Max(worldScale.x, worldScale.z) + skinWidth;
        float cylinderHeight = worldHeight - worldRadius - worldRadius;

        topSphere = worldCenter + (GetUpVector * (cylinderHeight / 2));
        baseSphere = worldCenter + (GetDownVector * (cylinderHeight / 2));
        radius = worldRadius;
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

    //////////////////////////////////////////////////////////////////
    /// CapsuleOverlap

    public Collider[] SmallCapsuleOverlap()
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.OverlapCapsule(topSphere, baseSphere, radius, LayerMask, TriggerQuery);
    }

    public Collider[] BigCapsuleOverlap()
    {
        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.OverlapCapsule(topSphere, baseSphere, radius, LayerMask, TriggerQuery);
    }

    public uint CapsuleOverlap(out Collider[] smallCapsuleOverlaps, out Collider[] bigCapsuleOverlaps, bool filter = true)
    {
        smallCapsuleOverlaps = SmallCapsuleOverlap();
        bigCapsuleOverlaps = BigCapsuleOverlap();

        if (filter)
        {
            Collider[] smallCapsuleOverlapsArray = smallCapsuleOverlaps;
            List<Collider> bigCapsuleOverlapsList = new List<Collider>(bigCapsuleOverlaps);

            bigCapsuleOverlapsList.RemoveAll((Collider collider) =>
            {
                for (int i = 0; i < smallCapsuleOverlapsArray.Length; i++)
                {
                    if (smallCapsuleOverlapsArray[i] == collider)
                        return true;
                }

                return false;
            });

            bigCapsuleOverlaps = bigCapsuleOverlapsList.ToArray();
        }

        return (uint)smallCapsuleOverlaps.Length + (uint)bigCapsuleOverlaps.Length;
    }

    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// CapsuleOverlapNonAlloc

    public uint CapsuleOverlapNonAlloc(Collider[] colliders)
    {
        if (colliders == null || colliders.Length == 0)
            return 0;

        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.OverlapCapsuleNonAlloc(topSphere, baseSphere, radius, colliders, LayerMask, TriggerQuery);
    }

    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// CapsuleCast

    public RaycastHit SmallCapsuleCast(Vector3 move)
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        Physics.CapsuleCast(topSphere, baseSphere, radius, move.normalized, out RaycastHit hit, move.magnitude, LayerMask, TriggerQuery);
        return hit;
    }

    public RaycastHit BigCapsuleCast(Vector3 move)
    {
        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        Physics.CapsuleCast(topSphere, baseSphere, radius, move.normalized, out RaycastHit hit, move.magnitude, LayerMask, TriggerQuery);
        return hit;
    }

    public bool CapsuleCast(Vector3 move, out RaycastHit smallCapsuleHit, out RaycastHit bigCapsuleHit)
    {
        smallCapsuleHit = SmallCapsuleCast(move);
        bigCapsuleHit = BigCapsuleCast(move);

        return smallCapsuleHit.IsHit() || bigCapsuleHit.IsHit();
    }

    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// CapsuleCastAll

    public RaycastHit[] SmallCapsuleCastAll(Vector3 move)
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.CapsuleCastAll(topSphere, baseSphere, radius, move.normalized, move.magnitude, LayerMask, TriggerQuery);
    }

    public RaycastHit[] BigCapsuleCastAll(Vector3 move)
    {
        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.CapsuleCastAll(topSphere, baseSphere, radius, move.normalized, move.magnitude, LayerMask, TriggerQuery);
    }

    public uint CapsuleCastAll(Vector3 move, out RaycastHit[] smallCapsuleCasts, out RaycastHit[] bigCapsuleCasts)
    {
        smallCapsuleCasts = SmallCapsuleCastAll(move);
        bigCapsuleCasts = BigCapsuleCastAll(move);

        return (uint)smallCapsuleCasts.Length + (uint)bigCapsuleCasts.Length;
    }

    //////////////////////////////////////////////////////////////////

    public uint CapsuleCastAllNonAlloc(RaycastHit[] hitResults, Vector3 move)
    {
        if (hitResults == null || hitResults.Length == 0)
            return 0;

        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.CapsuleCastNonAlloc(topSphere, baseSphere, radius, move.normalized, hitResults, move.magnitude, LayerMask, TriggerQuery);
    }

    //////////////////////////////////////////////////////////////////

    public Collider[] TopSphereOverlap()
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.OverlapSphere(topSphere, radius, LayerMask, TriggerQuery);
    }

    public uint TopSphereOverlapNonAlloc(Collider[] colliders)
    {
        if (colliders == null || colliders.Length == 0)
            return 0;

        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.OverlapSphereNonAlloc(topSphere, radius, colliders, LayerMask, TriggerQuery);
    }

    public RaycastHit TopSphereCast(Vector3 move)
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        Physics.SphereCast(topSphere, radius, move.normalized, out RaycastHit hit, move.magnitude, LayerMask, TriggerQuery);
        return hit;
    }

    public RaycastHit[] TopSphereCastAll(Vector3 move)
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.SphereCastAll(topSphere, radius, move.normalized, move.magnitude, LayerMask, TriggerQuery);
    }

    public uint TopSphereCastAllNonAlloc(RaycastHit[] hitResults, Vector3 move)
    {
        if (hitResults == null || hitResults.Length == 0)
            return 0;

        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.SphereCastNonAlloc(topSphere, radius, move.normalized, hitResults, move.magnitude, LayerMask, TriggerQuery);
    }

    public Collider[] BaseSphereOverlap()
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.OverlapSphere(baseSphere, radius, LayerMask, TriggerQuery);
    }

    public uint BaseSphereOverlapNonAlloc(Collider[] colliders)
    {
        if (colliders == null || colliders.Length == 0)
            return 0;

        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.OverlapSphereNonAlloc(baseSphere, radius, colliders, LayerMask, TriggerQuery);
    }

    public RaycastHit BaseSphereCast(Vector3 move)
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        Physics.SphereCast(baseSphere, radius, move.normalized, out RaycastHit hit, move.magnitude, LayerMask, TriggerQuery);
        return hit;
    }

    public RaycastHit[] BaseSphereCastAll(Vector3 move)
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.SphereCastAll(baseSphere, radius, move.normalized, move.magnitude, LayerMask, TriggerQuery);
    }

    public uint BaseSphereCastAllNonAlloc(RaycastHit[] hitResults, Vector3 move)
    {
        if (hitResults == null || hitResults.Length == 0)
            return 0;

        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.SphereCastNonAlloc(baseSphere, radius, move.normalized, hitResults, move.magnitude, LayerMask, TriggerQuery);
    }

    public RaycastHit CapsuleMove(Vector3 move, float moveThreshold = 0.00001f)
    {
        if (move.magnitude == 0f || move.magnitude < moveThreshold)
            return new RaycastHit();

        RaycastHit hit = SmallCapsuleCast(move);
        if (hit.collider == null)
        {
            // no collision occurred, so we made the complete move
            Move(move);
            return hit;
        }

        // move to the hit position
        if (hit.distance < moveThreshold)
        {
            return hit;
        }

        Move(move.normalized * hit.distance);
        return hit;
    }

    public RaycastHit CapsuleMoveNoHit(Vector3 move, float moveThreshold = 0.001f)
    {
        RaycastHit hit = SmallCapsuleCast(move);
        if (hit.collider == null)
        {
            // no collision occurred, so we made the complete move
            Move(move);
        }

        return hit;
    }

    //////////////////////////////////////////////////////////////////
    /// Penetration
    //////////////////////////////////////////////////////////////////

    /// <summary>
    /// Compute the minimal translation required to separate the character from the collider.
    /// </summary>
    /// <param name="moveOut">Minimal move required to separate the colliders apart.</param>
    /// <param name="collider">The collider to test.</param>
    /// <param name="colliderPosition">Position of the collider.</param>
    /// <param name="colliderRotation">Rotation of the collider.</param>
    /// <returns>True if found penetration.</returns>
    public bool ComputePenetration(out Vector3 moveOut, Collider collider, Vector3 colliderPosition, Quaternion colliderRotation)
    {
        if (CapsuleCollider == null || collider == null || collider == CapsuleCollider)
        {
            // Ignore self
            moveOut = Vector3.zero;
            return false;
        }

        float cacheRadius = CapsuleCollider.radius + SkinWidth;
        float cacheHeight = CapsuleCollider.height;

        CapsuleCollider.radius = cacheRadius + SkinWidth;
        CapsuleCollider.height = cacheHeight;

        // Note: Physics.ComputePenetration does not always return values when the colliders overlap.
        var result = Physics.ComputePenetration(CapsuleCollider, CapsuleCollider.transform.position,
            CapsuleCollider.transform.rotation, collider, colliderPosition, colliderRotation,
            out Vector3 direction, out float distance);

        CapsuleCollider.radius = cacheRadius;
        CapsuleCollider.height = cacheHeight;

        moveOut = direction * distance;
        return result;
    }

    public Vector3 ResolvePenetration(float collisionOffset = 0.001f)
    {
        Collider[] overlaps = SmallCapsuleOverlap();
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
                moveOut += capsuleMoveOut + (capsuleMoveOut.normalized * collisionOffset);
            }
        }

        Move(moveOut);
        return moveOut;
    }

    //////////////////////////////////////////////////////////////////
    /// Gizmos
    //////////////////////////////////////////////////////////////////

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(GetTopSpherePosition, GetRadius);
        Gizmos.DrawSphere(GetBaseSpherePosition, GetRadius);
    }
}
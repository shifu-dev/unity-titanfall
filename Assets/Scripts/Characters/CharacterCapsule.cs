using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCapsule : MonoBehaviour
{
    //////////////////////////////////////////////////////////////////
    // Constants
    //////////////////////////////////////////////////////////////////
    public const float k_CollisionOffset = 0.001f;

    //////////////////////////////////////////////////////////////////
    // Variables
    //////////////////////////////////////////////////////////////////
    [SerializeField] protected CapsuleCollider m_CapsuleCollider = null;
    [SerializeField] protected Vector3 m_Position = Vector3.zero;
    [SerializeField] protected Quaternion m_LocalRotation = Quaternion.identity;
    [SerializeField] protected Vector3 m_LocalScale = Vector3.one;
    [SerializeField] protected Vector3 m_Center = Vector3.zero;
    [SerializeField] protected int m_Direction = 1;
    [SerializeField] protected float m_Height = 2;
    [SerializeField] protected float m_Radius = 0.5f;
    [SerializeField] protected float m_SkinWidth = 0.01f;
    [SerializeField] protected LayerMask m_LayerMask = Physics.DefaultRaycastLayers;
    [SerializeField] protected QueryTriggerInteraction m_TriggerQuery = QueryTriggerInteraction.Ignore;

    public CapsuleCollider Capsule => m_CapsuleCollider;
    public Vector3 LastPosition { get; protected set; }
    public Quaternion LastRotation { get; protected set; }
    public Vector3 Velocity { get; protected set; }
    public float Speed => Velocity.magnitude;

    //////////////////////////////////////////////////////////////////
    /// UpdateLoop
    //////////////////////////////////////////////////////////////////

    public void Init(Character character)
    {
        m_Position = transform.position;
        m_LocalRotation = m_CapsuleCollider.transform.rotation;
    }

    public void UpdateImpl()
    {
        PerformMove();
    }

    public void PerformMove()
    {
        // store previous values
        LastPosition = transform.position;
        LastRotation = m_CapsuleCollider.transform.rotation;

        // set new values
        transform.position = m_Position;
        m_CapsuleCollider.transform.rotation = m_LocalRotation;
        m_CapsuleCollider.transform.localScale = m_LocalScale;
        m_CapsuleCollider.direction = m_Direction;
        m_CapsuleCollider.center = m_Center;
        m_CapsuleCollider.height = m_Height;
        m_CapsuleCollider.radius = m_Radius;

        // calculate velocity
        Velocity = (m_Position - LastPosition) / Time.deltaTime;
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
            return m_Radius * Mathf.Max(m_LocalScale.x, m_LocalScale.z);
        }
    }
    public float GetHeight
    {
        get
        {
            return m_Height * m_LocalScale.y;
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
            return m_LocalRotation;
        }
    }
    public Vector3 GetRotationEuler
    {
        get
        {
            return m_LocalRotation.eulerAngles;
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
            return m_Position;
        }
    }
    public Vector3 GetCenter
    {
        get
        {
            return GetPosition + m_Center;
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
        InternalCalculateCapsuleGeometry(out topSphere, out baseSphere, out radius, m_SkinWidth);
    }

    protected virtual void InternalCalculateCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius, float skinWidth)
    {
        Vector3 worldCenter = GetCenter;
        Vector3 worldScale = m_LocalScale;

        float worldHeight = m_Height * worldScale.y;
        float worldRadius = m_Radius * Mathf.Max(worldScale.x, worldScale.z) + skinWidth;
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
        m_Position += pos;
    }

    public void Rotate(Vector3 rot)
    {
        m_LocalRotation = m_LocalRotation * Quaternion.Euler(rot);
    }

    //////////////////////////////////////////////////////////////////
    /// CapsuleOverlap

    public Collider[] SmallCapsuleOverlap()
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.OverlapCapsule(topSphere, baseSphere, radius, m_LayerMask, m_TriggerQuery);
    }

    public Collider[] BigCapsuleOverlap()
    {
        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.OverlapCapsule(topSphere, baseSphere, radius, m_LayerMask, m_TriggerQuery);
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
        return (uint)Physics.OverlapCapsuleNonAlloc(topSphere, baseSphere, radius, colliders, m_LayerMask, m_TriggerQuery);
    }

    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// CapsuleCast

    public RaycastHit SmallCapsuleCast(Vector3 move)
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        Physics.CapsuleCast(topSphere, baseSphere, radius, move.normalized, out RaycastHit hit, move.magnitude, m_LayerMask, m_TriggerQuery);
        return hit;
    }

    public RaycastHit BigCapsuleCast(Vector3 move)
    {
        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        Physics.CapsuleCast(topSphere, baseSphere, radius, move.normalized, out RaycastHit hit, move.magnitude, m_LayerMask, m_TriggerQuery);
        return hit;
    }

    public bool CapsuleCast(Vector3 move, out RaycastHit smallCapsuleHit, out RaycastHit bigCapsuleHit)
    {
        smallCapsuleHit = SmallCapsuleCast(move);
        bigCapsuleHit = BigCapsuleCast(move);

        return smallCapsuleHit.collider || bigCapsuleHit.collider;
    }

    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// CapsuleCastAll

    public RaycastHit[] SmallCapsuleCastAll(Vector3 move)
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.CapsuleCastAll(topSphere, baseSphere, radius, move.normalized, move.magnitude, m_LayerMask, m_TriggerQuery);
    }

    public RaycastHit[] BigCapsuleCastAll(Vector3 move)
    {
        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.CapsuleCastAll(topSphere, baseSphere, radius, move.normalized, move.magnitude, m_LayerMask, m_TriggerQuery);
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
        return (uint)Physics.CapsuleCastNonAlloc(topSphere, baseSphere, radius, move.normalized, hitResults, move.magnitude, m_LayerMask, m_TriggerQuery);
    }

    //////////////////////////////////////////////////////////////////

    public Collider[] TopSphereOverlap()
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.OverlapSphere(topSphere, radius, m_LayerMask, m_TriggerQuery);
    }

    public uint TopSphereOverlapNonAlloc(Collider[] colliders)
    {
        if (colliders == null || colliders.Length == 0)
            return 0;

        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.OverlapSphereNonAlloc(topSphere, radius, colliders, m_LayerMask, m_TriggerQuery);
    }

    public RaycastHit TopSphereCast(Vector3 move)
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        Physics.SphereCast(topSphere, radius, move.normalized, out RaycastHit hit, move.magnitude, m_LayerMask, m_TriggerQuery);
        return hit;
    }

    public RaycastHit[] TopSphereCastAll(Vector3 move)
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.SphereCastAll(topSphere, radius, move.normalized, move.magnitude, m_LayerMask, m_TriggerQuery);
    }

    public uint TopSphereCastAllNonAlloc(RaycastHit[] hitResults, Vector3 move)
    {
        if (hitResults == null || hitResults.Length == 0)
            return 0;

        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.SphereCastNonAlloc(topSphere, radius, move.normalized, hitResults, move.magnitude, m_LayerMask, m_TriggerQuery);
    }

    //////////////////////////////////////////////////////////////////
    /// BaseSphereOverlap

    public Collider[] SmallBaseSphereOverlap()
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.OverlapSphere(baseSphere, radius, m_LayerMask, m_TriggerQuery);
    }

    public Collider[] BigBaseSphereOverlap()
    {
        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.OverlapSphere(baseSphere, radius, m_LayerMask, m_TriggerQuery);
    }

    public uint BaseSphereOverlap(out Collider[] smallSphereOverlaps, out Collider[] bigSphereOverlaps)
    {
        smallSphereOverlaps = SmallBaseSphereOverlap();
        bigSphereOverlaps = BigBaseSphereOverlap();

        return (uint)smallSphereOverlaps.Length + (uint)bigSphereOverlaps.Length;
    }

    //////////////////////////////////////////////////////////////////

    public uint BaseSphereOverlapNonAlloc(Collider[] colliders)
    {
        if (colliders == null || colliders.Length == 0)
            return 0;

        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.OverlapSphereNonAlloc(baseSphere, radius, colliders, m_LayerMask, m_TriggerQuery);
    }

    //////////////////////////////////////////////////////////////////
    /// BaseSphereCast

    public RaycastHit SmallBaseSphereCast(Vector3 move)
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        Physics.SphereCast(baseSphere, radius, move.normalized, out RaycastHit hit, move.magnitude, m_LayerMask, m_TriggerQuery);
        return hit;
    }

    public RaycastHit BigBaseSphereCast(Vector3 move)
    {
        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        Physics.SphereCast(baseSphere, radius, move.normalized, out RaycastHit hit, move.magnitude, m_LayerMask, m_TriggerQuery);
        return hit;
    }

    public bool BaseSphereCast(Vector3 move, out RaycastHit smallSphereHit, out RaycastHit baseSphereHit)
    {
        smallSphereHit = SmallBaseSphereCast(move);
        baseSphereHit = BigBaseSphereCast(move);

        return smallSphereHit.collider || baseSphereHit.collider;
    }

    //////////////////////////////////////////////////////////////////

    public RaycastHit[] BaseSphereCastAll(Vector3 move)
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.SphereCastAll(baseSphere, radius, move.normalized, move.magnitude, m_LayerMask, m_TriggerQuery);
    }

    public uint BaseSphereCastAllNonAlloc(RaycastHit[] hitResults, Vector3 move)
    {
        if (hitResults == null || hitResults.Length == 0)
            return 0;

        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.SphereCastNonAlloc(baseSphere, radius, move.normalized, hitResults, move.magnitude, m_LayerMask, m_TriggerQuery);
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
    /// Compute the minimal translation required to separate the character from the collider (without SkinWidth).
    /// </summary>
    /// <param name="moveOut">Minimal move required to separate the colliders apart.</param>
    /// <param name="collider">The collider to test.</param>
    /// <param name="colliderPosition">Position of the collider.</param>
    /// <param name="colliderRotation">Rotation of the collider.</param>
    /// <returns>True if found penetration.</returns>
    public bool ComputePenetrationForSmallCapsule(out Vector3 moveOut, Collider collider, Vector3 colliderPosition, Quaternion colliderRotation)
    {
        return InternalComputePenetration(false, out moveOut, collider, colliderPosition, colliderRotation);
    }

    /// <summary>
    /// Compute the minimal translation required to separate the character from the collider (with SkinWidth).
    /// </summary>
    /// <param name="moveOut">Minimal move required to separate the colliders apart.</param>
    /// <param name="collider">The collider to test.</param>
    /// <param name="colliderPosition">Position of the collider.</param>
    /// <param name="colliderRotation">Rotation of the collider.</param>
    /// <returns>True if found penetration.</returns>
    public bool ComputePenetrationForBigCapsule(out Vector3 moveOut, Collider collider, Vector3 colliderPosition, Quaternion colliderRotation)
    {
        return InternalComputePenetration(true, out moveOut, collider, colliderPosition, colliderRotation);
    }

    protected bool InternalComputePenetration(bool bigCapsule, out Vector3 moveOut, Collider collider, Vector3 colliderPosition, Quaternion colliderRotation)
    {
        if (m_CapsuleCollider == null || collider == null || collider == m_CapsuleCollider)
        {
            moveOut = Vector3.zero;
            return false;
        }

        // store current values
        float cacheRadius = m_CapsuleCollider.radius;
        float cacheHeight = m_CapsuleCollider.height;
        Vector3 cacheCenter = m_CapsuleCollider.center;
        int cacheDirection = m_CapsuleCollider.direction;

        // set new values
        m_CapsuleCollider.radius = m_Radius;
        m_CapsuleCollider.height = m_Height;
        m_CapsuleCollider.center = m_Center;
        m_CapsuleCollider.direction = m_Direction;

        // Note: Physics.ComputePenetration does not always return values when the colliders overlap.
        var result = Physics.ComputePenetration(m_CapsuleCollider, m_Position, m_LocalRotation,
            collider, colliderPosition, colliderRotation, out Vector3 direction, out float distance);

        // restore previous values
        m_CapsuleCollider.radius = cacheRadius;
        m_CapsuleCollider.height = cacheHeight;
        m_CapsuleCollider.center = cacheCenter;
        m_CapsuleCollider.direction = cacheDirection;

        moveOut = direction * distance;
        return result;
    }

    public Vector3 ResolvePenetrationForSmallCapsule(float collisionOffset = k_CollisionOffset)
    {
        return InternalResolvePenetration(false, collisionOffset);
    }

    public Vector3 ResolvePenetrationForBigCapsule(float collisionOffset = k_CollisionOffset)
    {
        return InternalResolvePenetration(true, collisionOffset);
    }

    protected Vector3 InternalResolvePenetration(bool bigCapsule, float collisionOffset)
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
                continue;

            Vector3 capsuleMoveOut;
            bool computed = bigCapsule ? ComputePenetrationForBigCapsule(out capsuleMoveOut, collider, collider.transform.position, collider.transform.rotation)
                : ComputePenetrationForSmallCapsule(out capsuleMoveOut, collider, collider.transform.position, collider.transform.rotation);

            if (computed)
            {
                moveOut = moveOut + capsuleMoveOut + (capsuleMoveOut.normalized * collisionOffset);
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
        // Gizmos.DrawSphere(GetTopSpherePosition, GetRadius);
        // Gizmos.DrawSphere(GetBaseSpherePosition, GetRadius);
    }
}
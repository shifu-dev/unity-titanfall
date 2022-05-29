using System;
using UnityEngine;

public class CharacterCapsule : CharacterBehaviour
{
    //////////////////////////////////////////////////////////////////
    // Constants
    //////////////////////////////////////////////////////////////////
    public const float k_collisionOffset = 0f;

    //////////////////////////////////////////////////////////////////
    // Variables
    //////////////////////////////////////////////////////////////////
    [SerializeField, Space] protected CapsuleCollider m_collider;
    [NonSerialized] protected Vector3 m_position;
    [NonSerialized] protected Quaternion m_rotation;
    [NonSerialized] protected Vector3 m_scale;
    [SerializeField] protected Vector3 m_center;
    [SerializeField, Min(0)] protected float m_height;
    [SerializeField, Min(0)] protected float m_radius;
    [SerializeField] protected float m_skinWidth;
    [SerializeField] protected LayerMask m_layerMask;
    [SerializeField] protected QueryTriggerInteraction m_triggerQuery;

    new public CapsuleCollider collider => m_collider;
    public Vector3 lastPosition { get; protected set; }
    public Quaternion lastRotation { get; protected set; }
    public Vector3 velocity { get; protected set; }
    public float speed => velocity.magnitude;

    //////////////////////////////////////////////////////////////////
    /// Constructors
    //////////////////////////////////////////////////////////////////

    public CharacterCapsule()
    {
        m_collider = null;
        m_position = Vector3.zero;
        m_rotation = Quaternion.identity;
        m_scale = Vector3.one;
        m_center = Vector3.zero;
        m_height = 2;
        m_radius = 0.5f;
        m_skinWidth = 0.01f;
        m_layerMask = Physics.DefaultRaycastLayers;
        m_triggerQuery = QueryTriggerInteraction.Ignore;
    }

    //////////////////////////////////////////////////////////////////
    /// UpdateLoop
    //////////////////////////////////////////////////////////////////

    public override void OnInitCharacter(Character character, CharacterInitializer initializer)
    {
        base.OnInitCharacter(character, initializer);

        m_position = transform.position;
        m_rotation = m_collider.transform.rotation;
    }

    public override void OnUpdateCharacter()
    {
        PerformMove();
    }

    public void PerformMove()
    {
        if (m_collider == null)
            return;

        // store previous values
        lastPosition = transform.position;
        lastRotation = m_collider.transform.rotation;

        // set new values
        transform.position = m_position;
        m_collider.transform.rotation = m_rotation;
        m_collider.transform.localScale = m_scale;
        m_collider.center = m_center;
        m_collider.height = m_height;
        m_collider.radius = m_radius;

        // calculate velocity
        velocity = (m_position - lastPosition) / Time.deltaTime;
    }

    //////////////////////////////////////////////////////////////////
    /// Geometry
    //////////////////////////////////////////////////////////////////

    /// Checks if Capsule is in Sphere shaped in Space
    public bool isSphereShaped
    {
        get
        {
            return height <= radius * 2;
        }
    }

    /// Lengths (Space && LocalSpace)
    public float radius
    {
        get
        {
            Vector3 worldScale = scale;
            return m_radius * Mathf.Max(worldScale.x, worldScale.z);
        }
    }
    public float height
    {
        get
        {
            return m_height * scale.y;
        }
    }
    public float cylinderHeight
    {
        get
        {
            float height = this.height;
            float tradius = radius * 2;

            return height > tradius ? height - tradius : 0;
        }
    }
    public float halfCylinderHeight
    {
        get
        {
            return cylinderHeight / 2;
        }
    }

    /// Rotations (Space)
    public Quaternion rotation
    {
        get
        {
            return m_rotation;
        }
    }
    public Vector3 rotationEuler
    {
        get
        {
            return m_rotation.eulerAngles;
        }
    }

    public Vector3 forward
    {
        get
        {
            return rotation * Vector3.forward;
        }
    }
    public Vector3 backward
    {
        get
        {
            return rotation * Vector3.back;
        }
    }
    public Vector3 left
    {
        get
        {
            return rotation * Vector3.left;
        }
    }
    public Vector3 right
    {
        get
        {
            return rotation * Vector3.right;
        }
    }
    public Vector3 up
    {
        get
        {
            return rotation * Vector3.up;
        }
    }
    public Vector3 down
    {
        get
        {
            return rotation * Vector3.down;
        }
    }

    /// Positions (Space && LocalSpace)
    public Vector3 position
    {
        get
        {
            return m_position;
        }
    }
    public Vector3 center
    {
        get
        {
            return position + m_center;
        }
    }
    public Vector3 topSpherePosition
    {
        get
        {
            return center + (up * halfCylinderHeight);
        }
    }
    public Vector3 baseSpherePosition
    {
        get
        {
            return center + (down * halfCylinderHeight);
        }
    }
    public Vector3 topPosition
    {
        get
        {
            return center + (up * (halfCylinderHeight + radius));
        }
    }
    public Vector3 basePosition
    {
        get
        {
            return center + (down * (halfCylinderHeight + radius));
        }
    }
    public Vector3 scale
    {
        get
        {
            Vector3 worldScale = transform.lossyScale;
            worldScale = worldScale.DivideEach(transform.localScale);
            worldScale = worldScale.MultiplyEach(m_scale);
            return worldScale;
        }
    }

    /// Volume (Space && LocalSpace)
    public float sphereVolume
    {
        get
        {
            return (float)Math.PI * (float)Math.Pow(radius, 2);
        }
    }
    public float halfSphereVolume
    {
        get
        {
            return sphereVolume / 2;
        }
    }
    public float cylinderVolume
    {
        get
        {
            return 2 * (float)Math.PI * radius * cylinderHeight;
        }
    }
    public float volume
    {
        get
        {
            return sphereVolume + cylinderVolume;
        }
    }

    /// Calculate Geometry (Space)
    public void CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius)
    {
        InternalCalculateCapsuleGeometry(out topSphere, out baseSphere, out radius, 0f);
    }

    public void CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius)
    {
        InternalCalculateCapsuleGeometry(out topSphere, out baseSphere, out radius, m_skinWidth);
    }

    protected virtual void InternalCalculateCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius, float skinWidth)
    {
        Vector3 worldCenter = center;
        Vector3 worldScale = scale;

        float worldHeight = m_height * worldScale.y;
        float worldRadius = m_radius * Mathf.Max(worldScale.x, worldScale.z) + skinWidth;
        float cylinderHeight = worldHeight - worldRadius - worldRadius;

        topSphere = worldCenter + (up * (cylinderHeight / 2));
        baseSphere = worldCenter + (down * (cylinderHeight / 2));
        radius = worldRadius;
    }

    //////////////////////////////////////////////////////////////////
    /// Physics
    //////////////////////////////////////////////////////////////////

    public void Move(Vector3 move)
    {
        m_position += move;
    }

    public void Rotate(Vector3 rot)
    {
        m_rotation = m_rotation * Quaternion.Euler(rot);
    }

    #region Capsule Physics

    public Collider[] SmallCapsuleOverlap()
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.OverlapCapsule(topSphere, baseSphere, radius, m_layerMask, m_triggerQuery);
    }

    public Collider[] BigCapsuleOverlap()
    {
        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.OverlapCapsule(topSphere, baseSphere, radius, m_layerMask, m_triggerQuery);
    }

    public uint CapsuleOverlap(out Collider[] smallCapsuleOverlaps, out Collider[] bigCapsuleOverlaps)
    {
        smallCapsuleOverlaps = SmallCapsuleOverlap();
        bigCapsuleOverlaps = BigCapsuleOverlap();

        return (uint)smallCapsuleOverlaps.Length + (uint)bigCapsuleOverlaps.Length;
    }

    public uint CapsuleOverlap(out Collider[] smallCapsuleOverlaps, out Collider[] bigCapsuleOverlaps, out Collider[] onlyBigCapsuleOverlaps)
    {
        smallCapsuleOverlaps = SmallCapsuleOverlap();
        bigCapsuleOverlaps = BigCapsuleOverlap();
        onlyBigCapsuleOverlaps = ArrayExtensions.FilterUnique(smallCapsuleOverlaps, bigCapsuleOverlaps);

        return (uint)smallCapsuleOverlaps.Length + (uint)onlyBigCapsuleOverlaps.Length;
    }

    public uint SmallCapsuleOverlapNonAlloc(Collider[] colliders)
    {
        if (colliders == null || colliders.Length == 0)
            return 0;

        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.OverlapCapsuleNonAlloc(topSphere, baseSphere, radius, colliders, m_layerMask, m_triggerQuery);
    }

    public uint BigCapsuleOverlapNonAlloc(Collider[] colliders)
    {
        if (colliders == null || colliders.Length == 0)
            return 0;

        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.OverlapCapsuleNonAlloc(topSphere, baseSphere, radius, colliders, m_layerMask, m_triggerQuery);
    }

    public uint CapsuleOverlapNonAlloc(Collider[] smallCapsuleOverlaps, Collider[] bigCapsuleOverlaps)
    {
        return SmallCapsuleOverlapNonAlloc(smallCapsuleOverlaps) +
            BigCapsuleOverlapNonAlloc(bigCapsuleOverlaps);
    }

    public uint CapsuleOverlapNonAlloc(Collider[] smallCapsuleOverlaps, Collider[] bigCapsuleOverlaps, out Collider[] onlyBigCapsuleOverlaps)
    {
        uint count = SmallCapsuleOverlapNonAlloc(smallCapsuleOverlaps);
        BigCapsuleOverlapNonAlloc(bigCapsuleOverlaps);

        if (smallCapsuleOverlaps == null || bigCapsuleOverlaps == null)
        {
            onlyBigCapsuleOverlaps = null;
        }
        else
        {
            onlyBigCapsuleOverlaps = ArrayExtensions.FilterUnique(smallCapsuleOverlaps, bigCapsuleOverlaps);
        }

        count += onlyBigCapsuleOverlaps == null ? 0 : (uint)onlyBigCapsuleOverlaps.Length;
        return count;
    }

    public RaycastHit SmallCapsuleCast(Vector3 move)
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        Physics.CapsuleCast(topSphere, baseSphere, radius, move.normalized, out RaycastHit hitInfo, move.magnitude, m_layerMask, m_triggerQuery);
        return hitInfo;
    }

    public RaycastHit BigCapsuleCast(Vector3 move)
    {
        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        Physics.CapsuleCast(topSphere, baseSphere, radius, move.normalized, out RaycastHit hit, move.magnitude, m_layerMask, m_triggerQuery);
        return hit;
    }

    public bool CapsuleCast(Vector3 move, out RaycastHit smallCapsuleHit, out RaycastHit bigCapsuleHit)
    {
        smallCapsuleHit = SmallCapsuleCast(move);
        bigCapsuleHit = BigCapsuleCast(move);

        return smallCapsuleHit.collider || bigCapsuleHit.collider;
    }

    public RaycastHit[] SmallCapsuleCastAll(Vector3 move)
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.CapsuleCastAll(topSphere, baseSphere, radius, move.normalized, move.magnitude, m_layerMask, m_triggerQuery);
    }

    public RaycastHit[] BigCapsuleCastAll(Vector3 move)
    {
        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.CapsuleCastAll(topSphere, baseSphere, radius, move.normalized, move.magnitude, m_layerMask, m_triggerQuery);
    }

    public uint CapsuleCastAll(Vector3 move, out RaycastHit[] smallCapsuleCasts, out RaycastHit[] bigCapsuleCasts)
    {
        smallCapsuleCasts = SmallCapsuleCastAll(move);
        bigCapsuleCasts = BigCapsuleCastAll(move);

        return (uint)smallCapsuleCasts.Length + (uint)bigCapsuleCasts.Length;
    }

    public uint CapsuleCastAll(Vector3 move, out RaycastHit[] smallCapsuleCasts, out RaycastHit[] bigCapsuleCasts, out RaycastHit[] onlyBigCapsuleCasts)
    {
        smallCapsuleCasts = SmallCapsuleCastAll(move);
        bigCapsuleCasts = BigCapsuleCastAll(move);
        onlyBigCapsuleCasts = ArrayExtensions.FilterUnique(smallCapsuleCasts, bigCapsuleCasts);

        return (uint)smallCapsuleCasts.Length + (uint)bigCapsuleCasts.Length;
    }

    public uint SmallCapsuleCastNonAlloc(RaycastHit[] hits, Vector3 move)
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.CapsuleCastNonAlloc(topSphere, baseSphere, radius, move.normalized, hits, move.magnitude, m_layerMask, m_triggerQuery);
    }

    public uint BigCapsuleCastNonAlloc(RaycastHit[] hits, Vector3 move)
    {
        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.CapsuleCastNonAlloc(topSphere, baseSphere, radius, move.normalized, hits, move.magnitude, m_layerMask, m_triggerQuery);
    }

    public uint CapsuleCastNonAlloc(Vector3 move, RaycastHit[] smallCapsuleCasts, RaycastHit[] bigCapsuleCasts)
    {
        return SmallCapsuleCastNonAlloc(smallCapsuleCasts, move) +
            BigCapsuleCastNonAlloc(bigCapsuleCasts, move);
    }

    public uint CapsuleCastNonAlloc(Vector3 move, RaycastHit[] smallCapsuleCasts, RaycastHit[] bigCapsuleCasts, out RaycastHit[] onlyBigCapsuleCasts)
    {
        uint count = SmallCapsuleCastNonAlloc(smallCapsuleCasts, move);
        BigCapsuleCastNonAlloc(bigCapsuleCasts, move);

        if (smallCapsuleCasts == null || bigCapsuleCasts == null)
        {
            onlyBigCapsuleCasts = null;
        }
        else
        {
            onlyBigCapsuleCasts = ArrayExtensions.FilterUnique(smallCapsuleCasts, bigCapsuleCasts);
        }

        count += onlyBigCapsuleCasts == null ? 0 : (uint)onlyBigCapsuleCasts.Length;
        return count;
    }

    #endregion

    #region TopSphere Physics

    public Collider[] SmallTopSphereOverlap()
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.OverlapSphere(topSphere, radius, m_layerMask, m_triggerQuery);
    }

    public Collider[] BigTopSphereOverlap()
    {
        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.OverlapSphere(topSphere, radius, m_layerMask, m_triggerQuery);
    }

    public uint SmallTopSphereOverlapNonAlloc(Collider[] colliders)
    {
        if (colliders == null || colliders.Length == 0)
            return 0;

        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.OverlapSphereNonAlloc(topSphere, radius, colliders, m_layerMask, m_triggerQuery);
    }

    public uint BigTopSphereOverlapNonAlloc(Collider[] colliders)
    {
        if (colliders == null || colliders.Length == 0)
            return 0;

        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.OverlapSphereNonAlloc(topSphere, radius, colliders, m_layerMask, m_triggerQuery);
    }

    public RaycastHit SmallTopSphereCast(Vector3 move)
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        Physics.SphereCast(topSphere, radius, move.normalized, out RaycastHit hit, move.magnitude, m_layerMask, m_triggerQuery);
        return hit;
    }

    public RaycastHit BigTopSphereCast(Vector3 move)
    {
        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        Physics.SphereCast(topSphere, radius, move.normalized, out RaycastHit hit, move.magnitude, m_layerMask, m_triggerQuery);
        return hit;
    }

    public RaycastHit[] SmallTopSphereCastAll(Vector3 move)
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.SphereCastAll(topSphere, radius, move.normalized, move.magnitude, m_layerMask, m_triggerQuery);
    }

    public RaycastHit[] BigTopSphereCastAll(Vector3 move)
    {
        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.SphereCastAll(topSphere, radius, move.normalized, move.magnitude, m_layerMask, m_triggerQuery);
    }

    public uint SmallTopSphereCastNonAlloc(RaycastHit[] hitResults, Vector3 move)
    {
        if (hitResults == null || hitResults.Length == 0)
            return 0;

        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.SphereCastNonAlloc(topSphere, radius, move.normalized, hitResults, move.magnitude, m_layerMask, m_triggerQuery);
    }

    public uint BigTopSphereCastNonAlloc(RaycastHit[] hitResults, Vector3 move)
    {
        if (hitResults == null || hitResults.Length == 0)
            return 0;

        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.SphereCastNonAlloc(topSphere, radius, move.normalized, hitResults, move.magnitude, m_layerMask, m_triggerQuery);
    }

    #endregion

    #region BaseSphere Physics

    public Collider[] SmallBaseSphereOverlap()
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.OverlapSphere(baseSphere, radius, m_layerMask, m_triggerQuery);
    }

    public Collider[] BigBaseSphereOverlap()
    {
        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.OverlapSphere(baseSphere, radius, m_layerMask, m_triggerQuery);
    }

    public uint SmallBaseSphereOverlapNonAlloc(Collider[] colliders)
    {
        if (colliders == null || colliders.Length == 0)
            return 0;

        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.OverlapSphereNonAlloc(baseSphere, radius, colliders, m_layerMask, m_triggerQuery);
    }

    public uint BigBaseSphereOverlapNonAlloc(Collider[] colliders)
    {
        if (colliders == null || colliders.Length == 0)
            return 0;

        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.OverlapSphereNonAlloc(baseSphere, radius, colliders, m_layerMask, m_triggerQuery);
    }

    public RaycastHit SmallBaseSphereCast(Vector3 move)
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        Physics.SphereCast(baseSphere, radius, move.normalized, out RaycastHit hit, move.magnitude, m_layerMask, m_triggerQuery);
        return hit;
    }

    public RaycastHit BigBaseSphereCast(Vector3 move)
    {
        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        Physics.SphereCast(baseSphere, radius, move.normalized, out RaycastHit hit, move.magnitude, m_layerMask, m_triggerQuery);
        return hit;
    }

    public RaycastHit[] SmallBaseSphereCastAll(Vector3 move)
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.SphereCastAll(baseSphere, radius, move.normalized, move.magnitude, m_layerMask, m_triggerQuery);
    }

    public RaycastHit[] BigBaseSphereCastAll(Vector3 move)
    {
        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.SphereCastAll(baseSphere, radius, move.normalized, move.magnitude, m_layerMask, m_triggerQuery);
    }

    public uint SmallBaseSphereCastNonAlloc(RaycastHit[] hitResults, Vector3 move)
    {
        if (hitResults == null || hitResults.Length == 0)
            return 0;

        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.SphereCastNonAlloc(baseSphere, radius, move.normalized, hitResults, move.magnitude, m_layerMask, m_triggerQuery);
    }

    public uint BigBaseSphereCastNonAlloc(RaycastHit[] hitResults, Vector3 move)
    {
        if (hitResults == null || hitResults.Length == 0)
            return 0;

        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.SphereCastNonAlloc(baseSphere, radius, move.normalized, hitResults, move.magnitude, m_layerMask, m_triggerQuery);
    }

    #endregion

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
        if (m_collider == null || collider == null || collider == m_collider)
        {
            moveOut = Vector3.zero;
            return false;
        }

        // store current values
        float cacheRadius = m_collider.radius;
        float cacheHeight = m_collider.height;
        Vector3 cacheCenter = m_collider.center;
        int cacheDirection = m_collider.direction;

        // set new values
        m_collider.radius = m_radius;
        m_collider.height = m_height;
        m_collider.center = m_center;

        // Note: Physics.ComputePenetration does not always return values when the colliders overlap.
        var result = Physics.ComputePenetration(m_collider, m_position, m_rotation,
            collider, colliderPosition, colliderRotation, out Vector3 direction, out float distance);

        // restore previous values
        m_collider.radius = cacheRadius;
        m_collider.height = cacheHeight;
        m_collider.center = cacheCenter;
        m_collider.direction = cacheDirection;

        moveOut = direction * distance;
        return result;
    }

    public Vector3 ResolvePenetrationForSmallCapsule(float collisionOffset = k_collisionOffset)
    {
        return InternalResolvePenetration(false, collisionOffset);
    }

    public Vector3 ResolvePenetrationForBigCapsule(float collisionOffset = k_collisionOffset)
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

        // store current values
        float cacheRadius = m_collider.radius;
        float cacheHeight = m_collider.height;
        Vector3 cacheCenter = m_collider.center;

        // set new values
        m_collider.radius = m_radius;
        m_collider.height = m_height;
        m_collider.center = m_center;

        Vector3 finalMoveOut = Vector3.zero;
        foreach (var collider in overlaps)
        {
            if (collider == null || collider == m_collider)
            {
                continue;
            }

            bool computed = Physics.ComputePenetration(m_collider, m_position, m_rotation,
                collider, collider.transform.position, collider.transform.rotation, out Vector3 direction, out float distance);

            Vector3 moveOut = direction * distance;

            if (computed)
            {
                finalMoveOut += moveOut + (moveOut.normalized * collisionOffset);
            }
        }

        // restore previous values
        m_collider.radius = cacheRadius;
        m_collider.height = cacheHeight;
        m_collider.center = cacheCenter;

        Move(finalMoveOut);
        return finalMoveOut;
    }

    //////////////////////////////////////////////////////////////////
    /// Gizmos
    //////////////////////////////////////////////////////////////////
}
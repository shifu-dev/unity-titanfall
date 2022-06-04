using System;
using UnityEngine;

public class CharacterCapsule : CharacterBehaviour
{
    //////////////////////////////////////////////////////////////////
    // Variables
    //////////////////////////////////////////////////////////////////

    [SerializeField, Label("Collider")]
    public new CapsuleCollider collider;

    [SerializeField, Label("Center")]
    public Vector3 localCenter;

    [SerializeField, Label("Height"), Min(0)]
    public float localHeight;

    [SerializeField, Label("Radius"), Min(0)]
    public float localRadius;

    [SerializeField, Label("SkinWidth"), Min(0)]
    public float skinWidth;

    [SerializeField, Label("LayerMask")]
    public LayerMask layerMask;

    [SerializeField, Label("Trigger Query")]
    public QueryTriggerInteraction triggerQuery;

    public Vector3 localPosition { get; set; }
    public Quaternion localRotation { get; set; }
    public Vector3 localScale { get; set; }
    public Vector3 lastLocalPosition { get; protected set; }
    public Quaternion lastLocalRotation { get; protected set; }
    public Vector3 lastLocalScale { get; protected set; }
    public Vector3 velocity { get; protected set; }
    public float speed => velocity.magnitude;

    public CharacterCapsule()
    {
        collider = null;
        localPosition = Vector3.zero;
        localRotation = Quaternion.identity;
        localScale = Vector3.one;
        localCenter = Vector3.zero;
        localHeight = 2;
        localRadius = 0.5f;
        skinWidth = 0.01f;
        layerMask = Physics.DefaultRaycastLayers;
        triggerQuery = QueryTriggerInteraction.Ignore;
    }

    //////////////////////////////////////////////////////////////////
    /// UpdateLoop
    //////////////////////////////////////////////////////////////////

    public override void OnInitCharacter(Character character, CharacterInitializer initializer)
    {
        base.OnInitCharacter(character, initializer);

        localPosition = collider.transform.localPosition;
        localRotation = collider.transform.localRotation;
        localScale = collider.transform.localScale;

        lastLocalPosition = localPosition;
        lastLocalRotation = localRotation;
        lastLocalScale = localScale;
    }

    public override void OnUpdateCharacter()
    {
        PerformMove();
    }

    public void PerformMove()
    {
        if (collider == null)
            return;

        // store previous values
        lastLocalPosition = collider.transform.localPosition;
        lastLocalRotation = collider.transform.localRotation;

        // set new values
        collider.transform.localPosition = localPosition;
        collider.transform.localRotation = localRotation;
        collider.transform.localScale = localScale;
        collider.center = localCenter;
        collider.height = localHeight;
        collider.radius = localRadius;

        // calculate velocity
        velocity = (localPosition - lastLocalPosition) / Time.deltaTime;
    }

    //////////////////////////////////////////////////////////////////
    /// Geometry
    //////////////////////////////////////////////////////////////////

    /// Checks if Capsule is in Sphere shaped in WorldSpace
    public bool isSphereShaped
    {
        get => height <= diameter;
    }

    //////////////////////////////////////////////////////////////////
    /// Lengths (WorldSpace & LocalSpace)
    //////////////////////////////////////////////////////////////////
    public float radius
    {
        get
        {
            Vector3 worldScale = scale;
            return localRadius * Math.Max(worldScale.x, worldScale.z);
        }
    }
    public float diameter
    {
        get => radius * 2;
    }
    public float height
    {
        get => Math.Max(localHeight * scale.y, diameter);
    }
    public float cylinderHeight
    {
        get => height - diameter;
    }
    public float halfCylinderHeight
    {
        get => cylinderHeight / 2;
    }

    //////////////////////////////////////////////////////////////////
    /// Rotations (WorldSpace & LocalSpace)
    //////////////////////////////////////////////////////////////////
    public Quaternion rotation
    {
        get => localRotation;
        set => localRotation = value;
    }
    public Vector3 rotationEuler
    {
        get => rotation.eulerAngles;
    }
    public Vector3 forward
    {
        get => rotation * Vector3.forward;
    }
    public Vector3 backward
    {
        get => rotation * Vector3.back;
    }
    public Vector3 left
    {
        get => rotation * Vector3.left;
    }
    public Vector3 right
    {
        get => rotation * Vector3.right;
    }
    public Vector3 up
    {
        get => rotation * Vector3.up;
    }
    public Vector3 down
    {
        get => rotation * Vector3.down;
    }

    //////////////////////////////////////////////////////////////////
    /// Positions (WorldSpace & LocalSpace)
    //////////////////////////////////////////////////////////////////
    public Vector3 position
    {
        get
        {
            return collider.transform.position -
                collider.transform.localPosition + localPosition;
        }
        set
        {
            localPosition = value - collider.transform.position;
        }
    }
    public Vector3 center
    {
        get => position + localCenter;
    }
    public Vector3 topSpherePosition
    {
        get => center + (up * halfCylinderHeight);
    }
    public Vector3 baseSpherePosition
    {
        get => center + (down * halfCylinderHeight);
    }
    public Vector3 topPosition
    {
        get => center + (up * (halfCylinderHeight + radius));
    }
    public Vector3 basePosition
    {
        get => center + (down * (halfCylinderHeight + radius));
    }
    public Vector3 scale
    {
        get
        {
            Vector3 worldScale = transform.lossyScale;
            worldScale = worldScale.DivideEach(transform.localScale);
            worldScale = worldScale.MultiplyEach(this.localScale);
            return worldScale;
        }
    }

    //////////////////////////////////////////////////////////////////
    /// Volume (WorldSpace)
    //////////////////////////////////////////////////////////////////
    public float sphereVolume
    {
        get
        {
            return (float)Math.PI * (float)Math.Pow(radius, 2);
        }
    }
    public float halfSphereVolume
    {
        get => sphereVolume / 2;
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
        get => sphereVolume + cylinderVolume;
    }

    //////////////////////////////////////////////////////////////////
    /// Geometry
    //////////////////////////////////////////////////////////////////

    public Vector3 GetPositionInVolume(Vector3 pos, bool scale = true, bool clamp = true)
    {
        if (scale)
        {
            pos.MultiplyEach(this.scale);
        }

        pos += center;

        if (clamp)
        {
            pos = ClampPositionInsideVolume(pos);
        }

        return pos;
    }

    public Vector3 ClampRelativePositionInsideVolume(Vector3 pos)
    {
        return ClampPositionInsideVolume(this.center + pos);
    }

    public Vector3 ClampPositionInsideVolume(Vector3 pos)
    {
        var center = this.center;
        CalculateSmallCapsuleGeometry(out var topSphere, out var baseSphere, out var radius);

        var topSphere_to_pos = pos - topSphere;
        if (Vector3.Angle(up, topSphere_to_pos) <= 90)
        {
            if (topSphere_to_pos.magnitude > radius)
            {
                pos += -topSphere_to_pos.normalized * (topSphere_to_pos.magnitude - radius);
            }

            return pos;
        }

        var baseSphere_to_pos = pos - baseSphere;
        if (Vector3.Angle(down, pos - baseSphere) <= 90)
        {
            if (baseSphere_to_pos.magnitude > radius)
            {
                pos += -baseSphere_to_pos.normalized * (baseSphere_to_pos.magnitude - radius);
            }

            return pos;
        }

        var center_to_pos_projected = Vector3.ProjectOnPlane(pos - center, up);
        if (center_to_pos_projected.magnitude > radius)
        {
            var length = center_to_pos_projected.magnitude - radius;
            var correction = -center_to_pos_projected.normalized * length;
            pos += correction;
        }

        return pos;
    }

    public void CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius)
    {
        InternalCalculateCapsuleGeometry(out topSphere, out baseSphere, out radius, 0f);
    }

    public void CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius)
    {
        InternalCalculateCapsuleGeometry(out topSphere, out baseSphere, out radius, skinWidth);
    }

    protected virtual void InternalCalculateCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius, float skinWidth)
    {
        InternalCalculateCapsuleGeometry(out var scale, out var center, out topSphere, out baseSphere, out var cylinderHeight, out radius, skinWidth);
    }

    protected virtual void InternalCalculateCapsuleGeometry(out Vector3 scale, out Vector3 center, out Vector3 topSphere, out Vector3 baseSphere, out float cylinderHeight, out float radius, float skinWidth)
    {
        scale = this.scale;
        radius = localRadius * Mathf.Max(scale.x, scale.z) + skinWidth;

        float worldHeight = localHeight * scale.y;
        cylinderHeight = worldHeight - radius - radius;

        center = this.center;
        topSphere = center + (up * (cylinderHeight / 2));
        baseSphere = center + (down * (cylinderHeight / 2));
    }

    //////////////////////////////////////////////////////////////////
    /// Physics
    //////////////////////////////////////////////////////////////////

    public void Move(Vector3 move)
    {
        localPosition += move;
    }

    public void Rotate(Vector3 rot)
    {
        localRotation = localRotation * Quaternion.Euler(rot);
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
    /// Capsule Physics
    //////////////////////////////////////////////////////////////////

    public Collider[] SmallCapsuleOverlap()
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.OverlapCapsule(topSphere, baseSphere, radius, layerMask, triggerQuery);
    }

    public Collider[] BigCapsuleOverlap()
    {
        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.OverlapCapsule(topSphere, baseSphere, radius, layerMask, triggerQuery);
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
        return (uint)Physics.OverlapCapsuleNonAlloc(topSphere, baseSphere, radius, colliders, layerMask, triggerQuery);
    }

    public uint BigCapsuleOverlapNonAlloc(Collider[] colliders)
    {
        if (colliders == null || colliders.Length == 0)
            return 0;

        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.OverlapCapsuleNonAlloc(topSphere, baseSphere, radius, colliders, layerMask, triggerQuery);
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
        Physics.CapsuleCast(topSphere, baseSphere, radius, move.normalized, out RaycastHit hitInfo, move.magnitude, layerMask, triggerQuery);
        return hitInfo;
    }

    public RaycastHit BigCapsuleCast(Vector3 move)
    {
        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        Physics.CapsuleCast(topSphere, baseSphere, radius, move.normalized, out RaycastHit hit, move.magnitude, layerMask, triggerQuery);
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
        return Physics.CapsuleCastAll(topSphere, baseSphere, radius, move.normalized, move.magnitude, layerMask, triggerQuery);
    }

    public RaycastHit[] BigCapsuleCastAll(Vector3 move)
    {
        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.CapsuleCastAll(topSphere, baseSphere, radius, move.normalized, move.magnitude, layerMask, triggerQuery);
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
        return (uint)Physics.CapsuleCastNonAlloc(topSphere, baseSphere, radius, move.normalized, hits, move.magnitude, layerMask, triggerQuery);
    }

    public uint BigCapsuleCastNonAlloc(RaycastHit[] hits, Vector3 move)
    {
        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.CapsuleCastNonAlloc(topSphere, baseSphere, radius, move.normalized, hits, move.magnitude, layerMask, triggerQuery);
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

    //////////////////////////////////////////////////////////////////
    /// TopSphere Physics
    //////////////////////////////////////////////////////////////////

    public Collider[] SmallTopSphereOverlap()
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.OverlapSphere(topSphere, radius, layerMask, triggerQuery);
    }

    public Collider[] BigTopSphereOverlap()
    {
        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.OverlapSphere(topSphere, radius, layerMask, triggerQuery);
    }

    public uint SmallTopSphereOverlapNonAlloc(Collider[] colliders)
    {
        if (colliders == null || colliders.Length == 0)
            return 0;

        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.OverlapSphereNonAlloc(topSphere, radius, colliders, layerMask, triggerQuery);
    }

    public uint BigTopSphereOverlapNonAlloc(Collider[] colliders)
    {
        if (colliders == null || colliders.Length == 0)
            return 0;

        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.OverlapSphereNonAlloc(topSphere, radius, colliders, layerMask, triggerQuery);
    }

    public RaycastHit SmallTopSphereCast(Vector3 move)
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        Physics.SphereCast(topSphere, radius, move.normalized, out RaycastHit hit, move.magnitude, layerMask, triggerQuery);
        return hit;
    }

    public RaycastHit BigTopSphereCast(Vector3 move)
    {
        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        Physics.SphereCast(topSphere, radius, move.normalized, out RaycastHit hit, move.magnitude, layerMask, triggerQuery);
        return hit;
    }

    public RaycastHit[] SmallTopSphereCastAll(Vector3 move)
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.SphereCastAll(topSphere, radius, move.normalized, move.magnitude, layerMask, triggerQuery);
    }

    public RaycastHit[] BigTopSphereCastAll(Vector3 move)
    {
        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.SphereCastAll(topSphere, radius, move.normalized, move.magnitude, layerMask, triggerQuery);
    }

    public uint SmallTopSphereCastNonAlloc(RaycastHit[] hitResults, Vector3 move)
    {
        if (hitResults == null || hitResults.Length == 0)
            return 0;

        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.SphereCastNonAlloc(topSphere, radius, move.normalized, hitResults, move.magnitude, layerMask, triggerQuery);
    }

    public uint BigTopSphereCastNonAlloc(RaycastHit[] hitResults, Vector3 move)
    {
        if (hitResults == null || hitResults.Length == 0)
            return 0;

        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.SphereCastNonAlloc(topSphere, radius, move.normalized, hitResults, move.magnitude, layerMask, triggerQuery);
    }

    //////////////////////////////////////////////////////////////////
    /// BaseSphere Physics
    //////////////////////////////////////////////////////////////////

    public Collider[] SmallBaseSphereOverlap()
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.OverlapSphere(baseSphere, radius, layerMask, triggerQuery);
    }

    public Collider[] BigBaseSphereOverlap()
    {
        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.OverlapSphere(baseSphere, radius, layerMask, triggerQuery);
    }

    public uint SmallBaseSphereOverlapNonAlloc(Collider[] colliders)
    {
        if (colliders == null || colliders.Length == 0)
            return 0;

        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.OverlapSphereNonAlloc(baseSphere, radius, colliders, layerMask, triggerQuery);
    }

    public uint BigBaseSphereOverlapNonAlloc(Collider[] colliders)
    {
        if (colliders == null || colliders.Length == 0)
            return 0;

        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.OverlapSphereNonAlloc(baseSphere, radius, colliders, layerMask, triggerQuery);
    }

    public RaycastHit SmallBaseSphereCast(Vector3 move)
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        Physics.SphereCast(baseSphere, radius, move.normalized, out RaycastHit hit, move.magnitude, layerMask, triggerQuery);
        return hit;
    }

    public RaycastHit BigBaseSphereCast(Vector3 move)
    {
        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        Physics.SphereCast(baseSphere, radius, move.normalized, out RaycastHit hit, move.magnitude, layerMask, triggerQuery);
        return hit;
    }

    public RaycastHit[] SmallBaseSphereCastAll(Vector3 move)
    {
        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.SphereCastAll(baseSphere, radius, move.normalized, move.magnitude, layerMask, triggerQuery);
    }

    public RaycastHit[] BigBaseSphereCastAll(Vector3 move)
    {
        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.SphereCastAll(baseSphere, radius, move.normalized, move.magnitude, layerMask, triggerQuery);
    }

    public uint SmallBaseSphereCastNonAlloc(RaycastHit[] hitResults, Vector3 move)
    {
        if (hitResults == null || hitResults.Length == 0)
            return 0;

        CalculateSmallCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.SphereCastNonAlloc(baseSphere, radius, move.normalized, hitResults, move.magnitude, layerMask, triggerQuery);
    }

    public uint BigBaseSphereCastNonAlloc(RaycastHit[] hitResults, Vector3 move)
    {
        if (hitResults == null || hitResults.Length == 0)
            return 0;

        CalculateBigCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.SphereCastNonAlloc(baseSphere, radius, move.normalized, hitResults, move.magnitude, layerMask, triggerQuery);
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
        if (collider == null || collider == null || this.collider == collider)
        {
            moveOut = Vector3.zero;
            return false;
        }

        // store current values
        float cacheRadius = this.collider.radius;
        float cacheHeight = this.collider.height;
        Vector3 cacheCenter = this.collider.center;
        int cacheDirection = this.collider.direction;

        // set new values
        this.collider.radius = localRadius;
        this.collider.height = localHeight;
        this.collider.center = localCenter;

        // Note: Physics.ComputePenetration does not always return values when the colliders overlap.
        var result = Physics.ComputePenetration(collider, localPosition, localRotation,
            collider, colliderPosition, colliderRotation, out Vector3 direction, out float distance);

        // restore previous values
        this.collider.radius = cacheRadius;
        this.collider.height = cacheHeight;
        this.collider.center = cacheCenter;
        this.collider.direction = cacheDirection;

        moveOut = direction * distance;
        return result;
    }

    public Vector3 ResolvePenetrationForSmallCapsule(float collisionOffset)
    {
        return InternalResolvePenetration(false, collisionOffset);
    }

    public Vector3 ResolvePenetrationForBigCapsule(float collisionOffset)
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
        float cacheRadius = collider.radius;
        float cacheHeight = collider.height;
        Vector3 cacheCenter = collider.center;

        // set new values
        collider.radius = localRadius;
        collider.height = localHeight;
        collider.center = localCenter;
        var thisCollider = this.collider;
        var thisPosition = this.position;
        var thisRotation = this.rotation;

        Vector3 finalMoveOut = Vector3.zero;
        foreach (var collider in overlaps)
        {
            if (collider == null || thisCollider == collider)
            {
                continue;
            }

            bool computed = Physics.ComputePenetration(thisCollider, thisPosition, thisRotation,
                collider, collider.transform.position, collider.transform.rotation, out Vector3 direction, out float distance);

            Vector3 moveOut = direction * distance;

            if (computed)
            {
                finalMoveOut += moveOut + (moveOut.normalized * collisionOffset);
            }
        }

        // restore previous values
        collider.radius = cacheRadius;
        collider.height = cacheHeight;
        collider.center = cacheCenter;

        Move(finalMoveOut);
        return finalMoveOut;
    }
}
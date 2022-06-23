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

    public float localSkinWidth
    {
        get => skinWidth * (localRadius / radius);
    }

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

        lastLocalPosition = Vector3.zero;
        lastLocalRotation = Quaternion.identity;
        lastLocalScale = Vector3.one;
        velocity = Vector3.zero;
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

        velocity = Vector3.zero;
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
        velocity = localPosition - lastLocalPosition;
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
        InternalCalculateCapsuleGeometry(out var scale, out var center, out topSphere, out baseSphere, out var height, out radius, skinWidth);
    }

    protected virtual void InternalCalculateCapsuleGeometry(out Vector3 scale, out Vector3 center, out Vector3 topSphere, out Vector3 baseSphere, out float height, out float radius, float skinWidth)
    {
        scale = this.scale;

        radius = localRadius * Mathf.Max(scale.x, scale.z);

        height = Math.Max(localHeight * scale.y, radius * 2f);

        float cylinderHeight = height - radius - radius;
        var halfCylinderHeight = cylinderHeight * .5f;

        center = this.center;

        topSphere = center + up * halfCylinderHeight;

        baseSphere = center + down * halfCylinderHeight;

        radius += skinWidth;
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

    public void Teleport(Vector3 pos, Quaternion rot)
    {
        lastLocalPosition = pos;
        localPosition = pos;

        lastLocalRotation = rot;
        localRotation = rot;
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

    public bool CapsuleCast(Vector3 move, out RaycastHit smallHit, out RaycastHit bigHit)
    {
        smallHit = SmallCapsuleCast(move);
        bigHit = skinWidth <= 0f ? smallHit : BigCapsuleCast(move);

        return smallHit.collider || bigHit.collider;
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

    public bool ComputePenetrationForSmallCapsule(out Vector3 moveOut, Collider collider, Vector3 colliderPosition, Quaternion colliderRotation)
    {
        return InternalComputePenetration(false, out moveOut, collider, colliderPosition, colliderRotation);
    }

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

        var thisCollider = this.collider;

        // store current values
        float cacheRadius = thisCollider.radius;
        float cacheHeight = thisCollider.height;
        Vector3 cacheCenter = thisCollider.center;
        int cacheDirection = thisCollider.direction;

        // set new values
        thisCollider.radius = bigCapsule ? localRadius + localSkinWidth : localRadius;
        thisCollider.height = localHeight;
        thisCollider.center = localCenter;

        // Note: Physics.ComputePenetration does not always return values when the colliders overlap.
        var result = Physics.ComputePenetration(thisCollider, this.position, this.rotation,
            collider, colliderPosition, colliderRotation, out Vector3 direction, out float distance);

        // restore previous values
        thisCollider.radius = cacheRadius;
        thisCollider.height = cacheHeight;
        thisCollider.center = cacheCenter;
        thisCollider.direction = cacheDirection;

        moveOut = direction * distance;
        return result;
    }

    public bool ResolvePenetrationInfoForSmallCapsule(out Vector3 moveOut, float collisionOffset = 0f)
    {
        return InternalResolvePenetrationInfo(false, out moveOut, collisionOffset);
    }

    public bool ResolvePenetrationInfoForBigCapsule(out Vector3 moveOut, float collisionOffset = 0f)
    {
        return InternalResolvePenetrationInfo(true, out moveOut, collisionOffset);
    }

    protected bool InternalResolvePenetrationInfo(bool bigCapsule, out Vector3 moveOut, float collisionOffset)
    {
        moveOut = Vector3.zero;

        Collider[] overlaps = SmallCapsuleOverlap();
        if (overlaps.Length <= 0)
        {
            return false;
        }

        var thisCollider = this.collider;
        var thisPosition = this.position;
        var thisRotation = this.rotation;

        // store current values
        float cacheRadius = thisCollider.radius;
        float cacheHeight = thisCollider.height;
        Vector3 cacheCenter = thisCollider.center;

        // set new values
        thisCollider.radius = bigCapsule ? localRadius + localSkinWidth : localRadius;
        thisCollider.height = localHeight;
        thisCollider.center = localCenter;

        bool didCompute = false;
        foreach (var collider in overlaps)
        {
            if (collider == null || thisCollider == collider)
            {
                continue;
            }

            bool computed = Physics.ComputePenetration(thisCollider, thisPosition, thisRotation,
                collider, collider.transform.position, collider.transform.rotation,
                out Vector3 direction, out float distance);

            if (computed)
            {
                didCompute = true;
                moveOut += direction * (distance + collisionOffset);
            }
        }

        // restore previous values
        thisCollider.radius = cacheRadius;
        thisCollider.height = cacheHeight;
        thisCollider.center = cacheCenter;

        return didCompute;
    }

    public bool ResolvePenetrationInfoForSmallCapsuleNonAlloc(Collider[] overlaps, out Vector3 moveOut, float collisionOffset = 0f)
    {
        return InternalResolvePenetrationInfoNonAlloc(false, overlaps, out moveOut, collisionOffset);
    }

    public bool ResolvePenetrationInfoForBigCapsuleNonAlloc(Collider[] overlaps, out Vector3 moveOut, float collisionOffset = 0f)
    {
        return InternalResolvePenetrationInfoNonAlloc(true, overlaps, out moveOut, collisionOffset);
    }

    protected bool InternalResolvePenetrationInfoNonAlloc(bool bigCapsule, Collider[] overlaps, out Vector3 moveOut, float collisionOffset)
    {
        moveOut = Vector3.zero;

        if (overlaps == null)
        {
            return false;
        }

        uint overlapCount = SmallCapsuleOverlapNonAlloc(overlaps);
        if (overlapCount <= 0)
        {
            return false;
        }

        var thisCollider = this.collider;
        var thisPosition = this.position;
        var thisRotation = this.rotation;

        // store current values
        float cacheRadius = thisCollider.radius;
        float cacheHeight = thisCollider.height;
        Vector3 cacheCenter = thisCollider.center;

        // set new values
        thisCollider.radius = bigCapsule ? localRadius + localSkinWidth : localRadius;
        thisCollider.height = localHeight;
        thisCollider.center = localCenter;

        bool didCompute = false;
        for (uint i = 0; i < overlapCount; i++)
        {
            var collider = overlaps[i];
            if (collider == null || thisCollider == collider)
            {
                continue;
            }

            bool computed = Physics.ComputePenetration(thisCollider, thisPosition, thisRotation,
                collider, collider.transform.position, collider.transform.rotation,
                out Vector3 direction, out float distance);

            if (computed)
            {
                didCompute = true;
                moveOut += direction * (distance + collisionOffset);
            }
        }

        // restore previous values
        thisCollider.radius = cacheRadius;
        thisCollider.height = cacheHeight;
        thisCollider.center = cacheCenter;

        return didCompute;
    }

    public Vector3 ResolvePenetrationForSmallCapsule(float collisionOffset = 0f)
    {
        return InternalResolvePenetration(false, collisionOffset);
    }

    public Vector3 ResolvePenetrationForBigCapsule(float collisionOffset = 0f)
    {
        return InternalResolvePenetration(true, collisionOffset);
    }

    protected Vector3 InternalResolvePenetration(bool bigCapsule, float collisionOffset)
    {
        Vector3 moveOut = Vector3.zero;

        Collider[] overlaps = bigCapsule ? BigCapsuleOverlap() : SmallCapsuleOverlap();
        if (overlaps.Length <= 0)
        {
            return moveOut;
        }

        var thisCollider = this.collider;
        var thisPosition = this.position;
        var thisRotation = this.rotation;

        // store current values
        float cacheRadius = thisCollider.radius;
        float cacheHeight = thisCollider.height;
        Vector3 cacheCenter = thisCollider.center;

        // set new values
        thisCollider.radius = bigCapsule ? localRadius + localSkinWidth : localRadius;
        thisCollider.height = localHeight;
        thisCollider.center = localCenter;

        foreach (var collider in overlaps)
        {
            if (collider == null || thisCollider == collider)
            {
                continue;
            }

            bool computed = Physics.ComputePenetration(thisCollider, thisPosition, thisRotation,
                collider, collider.transform.position, collider.transform.rotation,
                out Vector3 direction, out float distance);

            if (computed)
            {
                moveOut += direction * (distance + collisionOffset);
            }
        }

        // restore previous values
        thisCollider.radius = cacheRadius;
        thisCollider.height = cacheHeight;
        thisCollider.center = cacheCenter;

        localPosition += moveOut;
        return moveOut;
    }

    //////////////////////////////////////////////////////////////////
    /// Movement Physics
    //////////////////////////////////////////////////////////////////

    public Vector3 SweepMove(Vector3 move)
    {
        return SweepMove(move, out RaycastHit hit);
    }

    public Vector3 SweepMove(Vector3 move, out RaycastHit hit)
    {
        hit = SmallCapsuleCast(move);
        if (hit.collider != null)
        {
            move = move.normalized * hit.distance;
        }

        localPosition += move;
        return move;
    }

    public bool SweepMoveOnSurface(out Vector3 move, Vector3 originalMove, Vector3 remainingMove, RaycastHit hit, float slopeUpAngle, bool maintainVelocity)
    {
        if (slopeUpAngle <= 0 || hit.collider == null || remainingMove == Vector3.zero)
        {
            move = Vector3.zero;
            return false;
        }

        Vector3 moveVectorLeft = (Quaternion.Euler(0, -90, 0) * remainingMove).normalized;
        Vector3 obstacleForward = Vector3.ProjectOnPlane(-hit.normal, moveVectorLeft).normalized;
        float slopeAngle = 90f - Vector3.SignedAngle(remainingMove.normalized, obstacleForward, -moveVectorLeft);
        slopeAngle = Math.Max(slopeAngle, 0);

        if (slopeAngle > slopeUpAngle)
        {
            move = Vector3.zero;
            return false;
        }

        Vector3 slopeMove = Vector3.ProjectOnPlane(originalMove.normalized * remainingMove.magnitude, hit.normal);
        if (maintainVelocity)
        {
            slopeMove = slopeMove.normalized * remainingMove.magnitude;
        }

        move = slopeMove;
        return true;
    }

    public Vector3 SweepMoveAlongSurface(Vector3 originalMove, Vector3 remainingMove, RaycastHit hit, bool maintainVelocity)
    {
        if (hit.collider == null || remainingMove == Vector3.zero)
            return Vector3.zero;

        Vector3 slideMove = Vector3.ProjectOnPlane(originalMove.normalized * remainingMove.magnitude, hit.normal);
        if (maintainVelocity)
        {
            slideMove = slideMove.normalized * remainingMove.magnitude;
        }

        return slideMove;
    }

    //////////////////////////////////////////////////////////////////
    /// Debug
    //////////////////////////////////////////////////////////////////

    protected virtual void OnDrawGizmos()
    {
        CalculateSmallCapsuleGeometry(out var smallTopSphere, out var smallBaseSphere, out var smallRadius);
        GizmosExtensions.DrawWireCapsule(rotation, smallTopSphere, smallBaseSphere, smallRadius, Color.cyan);

        CalculateBigCapsuleGeometry(out var bigTopSphere, out var bigBaseSphere, out var bigRadius);
        GizmosExtensions.DrawWireCapsule(rotation, bigTopSphere, bigBaseSphere, bigRadius, Color.green);
    }
}
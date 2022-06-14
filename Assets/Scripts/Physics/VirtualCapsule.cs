using System;
using UnityEngine;

public struct VirtualCapsule
{
    //////////////////////////////////////////////////////////////////
    /// Physics
    //////////////////////////////////////////////////////////////////

    public CapsuleCollider collider { get; set; }

    public LayerMask layerMask { get; set; }
    public QueryTriggerInteraction triggerQuery { get; set; }

    public Vector3 velocity { get; }
    public float speed => velocity.magnitude;

    //////////////////////////////////////////////////////////////////
    /// Positions (WorldSpace & LocalSpace)
    //////////////////////////////////////////////////////////////////
    public Vector3 position { get; set; }
    public Vector3 localCenter { get; set; }
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

    //////////////////////////////////////////////////////////////////
    /// Rotations (WorldSpace & LocalSpace)
    //////////////////////////////////////////////////////////////////
    public Quaternion rotation { get; set; }
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
    /// Lengths (WorldSpace & LocalSpace)
    //////////////////////////////////////////////////////////////////

    public Vector3 scale { get; set; }
    public float localRadius { get; set; }
    public float radius
    {
        get
        {
            Vector3 worldScale = this.scale;
            return localRadius * Math.Max(worldScale.x, worldScale.z);
        }
    }
    public float diameter
    {
        get => radius * 2;
    }
    public float localHeight { get; set; }
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

    /// Checks if Capsule is in Sphere shaped in WorldSpace
    public bool isSphereShaped
    {
        get => height <= diameter;
    }

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
        CalculateCapsuleGeometry(out var topSphere, out var baseSphere, out var radius);

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

    public void CalculateCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius)
    {
        CalculateCapsuleGeometry(out var scale, out var center, out topSphere, out baseSphere, out var cylinderHeight, out radius);
    }

    public void CalculateCapsuleGeometry(out Vector3 scale, out Vector3 center, out Vector3 topSphere, out Vector3 baseSphere, out float cylinderHeight, out float radius)
    {
        scale = this.scale;
        radius = localRadius * Mathf.Max(scale.x, scale.z);

        float worldHeight = localHeight * scale.y;
        cylinderHeight = worldHeight - radius - radius;

        center = this.center;
        topSphere = center + (up * (cylinderHeight / 2));
        baseSphere = center + (down * (cylinderHeight / 2));
    }

    //////////////////////////////////////////////////////////////////
    /// Capsule Physics
    //////////////////////////////////////////////////////////////////

    public Collider[] CapsuleOverlap()
    {
        CalculateCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.OverlapCapsule(topSphere, baseSphere, radius, layerMask, triggerQuery);
    }

    public uint CapsuleOverlapNonAlloc(Collider[] colliders)
    {
        if (colliders == null || colliders.Length == 0)
            return 0;

        CalculateCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.OverlapCapsuleNonAlloc(topSphere, baseSphere, radius, colliders, layerMask, triggerQuery);
    }

    public RaycastHit CapsuleCast(Vector3 move)
    {
        CalculateCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        Physics.CapsuleCast(topSphere, baseSphere, radius, move.normalized, out RaycastHit hitInfo, move.magnitude, layerMask, triggerQuery);
        return hitInfo;
    }

    public RaycastHit[] CapsuleCastAll(Vector3 move)
    {
        CalculateCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.CapsuleCastAll(topSphere, baseSphere, radius, move.normalized, move.magnitude, layerMask, triggerQuery);
    }

    public uint CapsuleCastNonAlloc(RaycastHit[] hits, Vector3 move)
    {
        CalculateCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.CapsuleCastNonAlloc(topSphere, baseSphere, radius, move.normalized, hits, move.magnitude, layerMask, triggerQuery);
    }

    //////////////////////////////////////////////////////////////////
    /// TopSphere Physics
    //////////////////////////////////////////////////////////////////

    public Collider[] TopSphereOverlap()
    {
        CalculateCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.OverlapSphere(topSphere, radius, layerMask, triggerQuery);
    }

    public uint TopSphereOverlapNonAlloc(Collider[] colliders)
    {
        if (colliders == null || colliders.Length == 0)
            return 0;

        CalculateCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.OverlapSphereNonAlloc(topSphere, radius, colliders, layerMask, triggerQuery);
    }

    public RaycastHit TopSphereCast(Vector3 move)
    {
        CalculateCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        Physics.SphereCast(topSphere, radius, move.normalized, out RaycastHit hit, move.magnitude, layerMask, triggerQuery);
        return hit;
    }

    public RaycastHit[] TopSphereCastAll(Vector3 move)
    {
        CalculateCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.SphereCastAll(topSphere, radius, move.normalized, move.magnitude, layerMask, triggerQuery);
    }

    public uint TopSphereCastNonAlloc(RaycastHit[] hitResults, Vector3 move)
    {
        if (hitResults == null || hitResults.Length == 0)
            return 0;

        CalculateCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.SphereCastNonAlloc(topSphere, radius, move.normalized, hitResults, move.magnitude, layerMask, triggerQuery);
    }

    //////////////////////////////////////////////////////////////////
    /// BaseSphere Physics
    //////////////////////////////////////////////////////////////////

    public Collider[] BaseSphereOverlap()
    {
        CalculateCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.OverlapSphere(baseSphere, radius, layerMask, triggerQuery);
    }

    public uint BaseSphereOverlapNonAlloc(Collider[] colliders)
    {
        if (colliders == null || colliders.Length == 0)
            return 0;

        CalculateCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.OverlapSphereNonAlloc(baseSphere, radius, colliders, layerMask, triggerQuery);
    }

    public RaycastHit BaseSphereCast(Vector3 move)
    {
        CalculateCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        Physics.SphereCast(baseSphere, radius, move.normalized, out RaycastHit hit, move.magnitude, layerMask, triggerQuery);
        return hit;
    }

    public RaycastHit[] BaseSphereCastAll(Vector3 move)
    {
        CalculateCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return Physics.SphereCastAll(baseSphere, radius, move.normalized, move.magnitude, layerMask, triggerQuery);
    }

    public uint BaseSphereCastNonAlloc(RaycastHit[] hitResults, Vector3 move)
    {
        if (hitResults == null || hitResults.Length == 0)
            return 0;

        CalculateCapsuleGeometry(out Vector3 topSphere, out Vector3 baseSphere, out float radius);
        return (uint)Physics.SphereCastNonAlloc(baseSphere, radius, move.normalized, hitResults, move.magnitude, layerMask, triggerQuery);
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
        hit = CapsuleCast(move);
        if (hit.collider != null)
        {
            move = move.normalized * hit.distance;
        }

        position += move;
        return move;
    }

    //////////////////////////////////////////////////////////////////
    /// Penetration
    //////////////////////////////////////////////////////////////////

    public bool ComputePenetration(out Vector3 moveOut, Collider collider, Vector3 colliderPosition, Quaternion colliderRotation)
    {
        moveOut = Vector3.zero;
        var thisCollider = this.collider;

        if (thisCollider == null || collider == null || thisCollider == collider)
        {
            return false;
        }

        // store current values
        float cacheRadius = thisCollider.radius;
        float cacheHeight = thisCollider.height;
        Vector3 cacheCenter = thisCollider.center;
        int cacheDirection = thisCollider.direction;

        // set new values
        thisCollider.radius = this.localRadius;
        thisCollider.height = this.localHeight;
        thisCollider.center = this.localCenter;

        // Note: Physics.ComputePenetration does not always return values when the colliders overlap.
        var result = Physics.ComputePenetration(thisCollider, this.position, this.rotation,
            collider, colliderPosition, colliderRotation, out Vector3 direction, out float distance);

        // restore previous values
        thisCollider.radius = cacheRadius;
        thisCollider.height = cacheHeight;
        thisCollider.center = cacheCenter;
        thisCollider.direction = cacheDirection;

        if (result)
        {
            moveOut = direction * distance;
            return true;
        }

        return false;
    }

    public bool ResolvePenetrationInfo(out Vector3 moveOut, float collisionOffset = 0f)
    {
        moveOut = Vector3.zero;

        Collider[] overlaps = CapsuleOverlap();
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
        thisCollider.radius = localRadius;
        thisCollider.height = localHeight;
        thisCollider.center = localCenter;

        foreach (var otherCollider in overlaps)
        {
            if (otherCollider == null || thisCollider == otherCollider)
            {
                continue;
            }

            bool computed = Physics.ComputePenetration(thisCollider, thisPosition, thisRotation,
                otherCollider, otherCollider.transform.position, otherCollider.transform.rotation,
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

        return true;
    }

    public Vector3 ResolvePenetration(float collisionOffset = 0f)
    {
        if (ResolvePenetrationInfo(out var moveOut, collisionOffset))
        {
            position += moveOut;
            return moveOut;
        }

        return Vector3.zero;
    }
}
using System;
using UnityEngine;

[Serializable]
public struct VirtualCapsule
{
    public const float DEFAULT_COLLISION_OFFSET = 0f;
    public const int DEFAULT_LAYER_MASK = Physics.DefaultRaycastLayers;
    public const QueryTriggerInteraction DEFAULT_TRIGGER_QUERY = QueryTriggerInteraction.Ignore;

    private struct TempColliderUser : IDisposable
    {
        public TempColliderUser(VirtualCapsule capsule, CapsuleCollider collider)
        {
            // cache values
            mCollider = collider;
            mRadius = mCollider.radius;
            mHeight = mCollider.height;

            // write values to CapsuleCollider
            Vector3 worldScale = collider.transform.lossyScale;
            worldScale.x = Mathf.Abs(worldScale.x);
            worldScale.y = Mathf.Abs(worldScale.y);
            worldScale.z = Mathf.Abs(worldScale.z);

            mCollider.radius = capsule.mRadius / Mathf.Max(worldScale.x, worldScale.z);
            mCollider.height = capsule.mHeight / worldScale.y;
        }

        public void Dispose()
        {
            // rewrite cached values
            mCollider.radius = mRadius;
            mCollider.height = mHeight;
        }

        private CapsuleCollider mCollider;
        private float mRadius;
        private float mHeight;
    }

    [SerializeField] private Vector3 mPosition;
    [SerializeField] private Quaternion mRotation;
    [SerializeField] private float mHeight;
    [SerializeField] private float mRadius;

    // cached values for performance
    private Vector3 mTopSphere;
    private Vector3 mBaseSphere;
    private Vector3 mDirUp;

    private LayerMask mLayerMask;
    private QueryTriggerInteraction mQueryTrigger;

    public VirtualCapsule(int layerMask = DEFAULT_LAYER_MASK,
        QueryTriggerInteraction queryTrigger = DEFAULT_TRIGGER_QUERY)
    {
        mPosition = Vector3.zero;
        mRotation = Quaternion.identity;
        mHeight = 2f;
        mRadius = 0.5f;

        mLayerMask = layerMask;
        mQueryTrigger = queryTrigger;

        mTopSphere = default;
        mBaseSphere = default;
        mDirUp = default;
    }

    public VirtualCapsule(CapsuleCollider collider, int layerMask = DEFAULT_LAYER_MASK,
        QueryTriggerInteraction queryTrigger = DEFAULT_TRIGGER_QUERY)
        : this(layerMask, queryTrigger)
    {
        ReadValuesFrom(collider);
    }

    public VirtualCapsule(CharacterController controller, int layerMask = DEFAULT_LAYER_MASK,
        QueryTriggerInteraction queryTrigger = DEFAULT_TRIGGER_QUERY)
        : this(layerMask, queryTrigger)
    {
        ReadValuesFrom(controller);
    }

    private void InternalUpdateCache()
    {
        float length = Mathf.Max(0, (mHeight / 2f) - mRadius);
        mTopSphere = mPosition + (mDirUp * length);
        mBaseSphere = mPosition + (mDirUp * -length);
    }

    public LayerMask layerMask
    {
        get => mLayerMask;
        set => mLayerMask = value;
    }
    public QueryTriggerInteraction queryTrigger
    {
        get => mQueryTrigger;
        set => mQueryTrigger = value;
    }

    public Vector3 position
    {
        get => mPosition;
        set
        {
            mPosition = value;
            InternalUpdateCache();
        }
    }
    public Vector3 topSpherePos
    {
        get => mTopSphere;
    }
    public Vector3 baseSpherePos
    {
        get => mBaseSphere;
    }
    public Vector3 topPos
    {
        get => mTopSphere + (mDirUp * mRadius);
    }
    public Vector3 basePos
    {
        get => mBaseSphere + (mDirUp * -mRadius);
    }

    public Quaternion rotation
    {
        get => mRotation;
        set
        {
            mRotation = value;
            mDirUp = mRotation * Vector3.up;
            InternalUpdateCache();
        }
    }
    public Vector3 forward => mRotation * Vector3.forward;
    public Vector3 backward => mRotation * Vector3.back;
    public Vector3 left => mRotation * Vector3.left;
    public Vector3 right => mRotation * Vector3.right;
    public Vector3 up => mRotation * Vector3.up;
    public Vector3 down => mRotation * Vector3.down;

    public float radius
    {
        get => mRadius;
        set
        {
            mRadius = value;
            InternalUpdateCache();
        }
    }
    public float diameter
    {
        get => mRadius * 2f;
    }
    public float height
    {
        get => mHeight;
        set
        {
            mHeight = value;
            InternalUpdateCache();
        }
    }
    public float cylinderHeight
    {
        get => Mathf.Max(0, mHeight - (mRadius * 2f));
    }

    public float sphereVolume
    {
        get => Mathf.PI * mRadius * mRadius;
    }
    public float cylinderVolume
    {
        get => 2 * Mathf.PI * mRadius * cylinderHeight;
    }
    public float volume
    {
        get => sphereVolume + cylinderVolume;
    }

    public bool isSphereShaped
    {
        get => mHeight <= mRadius * 2f;
    }

    // Reads the values form CapsuleCollider and applies to this VirtualCapsule.
    // After this operation VirtualCapsule will exactly imitate CapsuleCollider
    // This does not affects layerMask and queryTrigger.
    public void ReadValuesFrom(in CapsuleCollider collider)
    {
        if (collider is null)
        {
            throw new NullReferenceException("cannot read values from NULL collider");
        }

        Vector3 worldScale = collider.transform.lossyScale;

        Vector3 positionOffset = collider.transform.rotation *
            Vector3.Scale(collider.center, worldScale);

        // abs worldScale values, to scale collider correctly in reverse direction
        worldScale.x = Mathf.Abs(worldScale.x);
        worldScale.y = Mathf.Abs(worldScale.y);
        worldScale.z = Mathf.Abs(worldScale.z);

        // converts the collider.direction to rotation offset,
        // 0 -> X, 1 -> Y, 2 -> z
        Quaternion rotationOffset = Quaternion.identity;
        switch (collider.direction)
        {
            case 0:
                rotationOffset = Quaternion.Euler(Vector3.forward * 90f);
                float tmpZ = worldScale.z;
                worldScale.z = worldScale.y;
                worldScale.y = worldScale.x;
                worldScale.x = tmpZ;
                break;

            case 1:
                rotationOffset = Quaternion.Euler(Vector3.zero);
                break;

            case 2:
                rotationOffset = Quaternion.Euler(Vector3.right * 90f);
                float tmpY = worldScale.y;
                worldScale.y = worldScale.z;
                worldScale.z = worldScale.x;
                worldScale.x = tmpY;
                break;

            default: break;
        }

        // apply the values
        mRotation = collider.transform.rotation * rotationOffset;
        mPosition = collider.transform.position + positionOffset;
        mRadius = collider.radius * Mathf.Max(worldScale.x, worldScale.z);
        mHeight = collider.height * worldScale.y;

        // cache values
        mDirUp = mRotation * Vector3.up;
        InternalUpdateCache();
    }

    // Writes the values of VirtualCapsule to CapsuleCollider.
    // After this operation CapsuleCollider will exactly imitate VirtualCapsule
    // This method does not changes CapsuleCollider.Center and CapsuleCollider.Direction
    public void WriteValuesTo(CapsuleCollider collider)
    {
        if (collider is null)
        {
            throw new NullReferenceException("cannot write value to NULL collider");
        }

        Vector3 worldScale = collider.transform.lossyScale;

        Vector3 positionOffset = collider.transform.rotation *
            Vector3.Scale(collider.center, worldScale);

        // abs worldScale values, to scale collider correctly in reverse direction
        worldScale.x = Mathf.Abs(worldScale.x);
        worldScale.y = Mathf.Abs(worldScale.y);
        worldScale.z = Mathf.Abs(worldScale.z);

        // converts the collider.direction to rotation offset,
        // 0 -> X, 1 -> Y, 2 -> z
        Quaternion rotationOffset = Quaternion.identity;
        switch (collider.direction)
        {
            case 0:
                rotationOffset = Quaternion.Euler(Vector3.forward * -90f);
                float tmpZ = worldScale.z;
                worldScale.z = worldScale.y;
                worldScale.y = worldScale.x;
                worldScale.x = tmpZ;
                break;

            case 1:
                rotationOffset = Quaternion.Euler(Vector3.zero);
                break;

            case 2:
                rotationOffset = Quaternion.Euler(Vector3.right * -90f);
                float tmpY = worldScale.y;
                worldScale.y = worldScale.z;
                worldScale.z = worldScale.x;
                worldScale.x = tmpY;
                break;

            default: break;
        }

        // apply the values
        collider.transform.rotation = mRotation * rotationOffset;
        collider.transform.position = mPosition - positionOffset;
        collider.radius = mRadius / Mathf.Max(worldScale.x, worldScale.z);
        collider.height = mHeight / worldScale.y;
    }

    // Reads the values form CharacterController and applies to this VirtualCapsule.
    // After this operation VirtualCapsule will exactly imitate CharacterController
    // This does not affects layerMask and queryTrigger.
    public void ReadValuesFrom(in CharacterController controller)
    {
        if (controller is null)
        {
            throw new NullReferenceException("cannot read values from NULL controller");
        }

        Vector3 worldScale = controller.transform.lossyScale;

        Vector3 positionOffset = controller.transform.rotation *
            Vector3.Scale(controller.center, worldScale);

        // abs worldScale values, to scale collider correctly in reverse direction
        worldScale.x = Mathf.Abs(worldScale.x);
        worldScale.y = Mathf.Abs(worldScale.y);
        worldScale.z = Mathf.Abs(worldScale.z);

        // apply the values
        mRotation = controller.transform.rotation;
        mPosition = controller.transform.position + positionOffset;
        mRadius = controller.radius * Mathf.Max(worldScale.x, worldScale.z);
        mHeight = controller.height * worldScale.y;

        // cache values
        mDirUp = mRotation * Vector3.up;
        InternalUpdateCache();
    }

    // Writes the values of VirtualCapsule to CharacterController.
    // After this operation CharacterController will exactly imitate VirtualCapsule
    // This method does not changes CharacterController.Center
    public void WriteValuesTo(CharacterController controller)
    {
        if (controller is null)
        {
            throw new NullReferenceException("cannot write value to NULL controller");
        }

        Vector3 worldScale = controller.transform.lossyScale;

        Vector3 positionOffset = controller.transform.rotation *
            Vector3.Scale(controller.center, worldScale);

        // abs worldScale values, to scale collider correctly in reverse direction
        worldScale.x = Mathf.Abs(worldScale.x);
        worldScale.y = Mathf.Abs(worldScale.y);
        worldScale.z = Mathf.Abs(worldScale.z);

        // apply the values
        controller.transform.rotation = mRotation;
        controller.transform.position = mPosition - positionOffset;
        controller.radius = mRadius / Mathf.Max(worldScale.x, worldScale.z);
        controller.height = mHeight / worldScale.y;
    }

    // Given a point [pos] in worldSpace, returns a point 
    // which is clamped inside or on the surface of this Capsule
    public Vector3 ClampPositionInsideVolume(Vector3 pos)
    {
        var topSphereToPos = pos - mTopSphere;
        if (Vector3.Angle(mDirUp, topSphereToPos) <= 90)
        {
            if (topSphereToPos.magnitude > radius)
            {
                pos += -topSphereToPos.normalized * (topSphereToPos.magnitude - radius);
            }

            return pos;
        }

        var baseSphereToPos = pos - mBaseSphere;
        if (Vector3.Angle(-mDirUp, baseSphereToPos) <= 90)
        {
            if (baseSphereToPos.magnitude > radius)
            {
                pos += -baseSphereToPos.normalized * (baseSphereToPos.magnitude - radius);
            }

            return pos;
        }

        var centerToPosProjected = Vector3.ProjectOnPlane(pos, up);
        if (centerToPosProjected.magnitude > radius)
        {
            var length = centerToPosProjected.magnitude - radius;
            var correction = -centerToPosProjected.normalized * length;
            pos += correction;
        }

        return pos;
    }

    public Vector3 ClampPositionInsideVolumeRelative(Vector3 pos)
    {
        return ClampPositionInsideVolume(mPosition + pos);
    }

    // Returns count of overlaps
    public int CapsuleOverlap(out Collider[] colliders)
    {
        colliders = Physics.OverlapCapsule(mTopSphere, mBaseSphere, mRadius, mLayerMask, mQueryTrigger);
        return colliders.Length;
    }

    // Returns count of overlaps
    public int CapsuleOverlapNonAlloc(Collider[] colliders)
    {
        if (colliders is null || colliders.Length == 0)
            return 0;

        return Physics.OverlapCapsuleNonAlloc(mTopSphere, mBaseSphere, mRadius, colliders, mLayerMask, mQueryTrigger);
    }

    // Returns true if cast hit something
    public bool CapsuleCast(Vector3 move, out RaycastHit hit)
    {
        return Physics.CapsuleCast(mTopSphere, mBaseSphere, mRadius, move.normalized, out hit, move.magnitude, mLayerMask, mQueryTrigger);
    }

    // Returns count of hits
    public int CapsuleCastAll(Vector3 move, out RaycastHit[] hits)
    {
        hits = Physics.CapsuleCastAll(mTopSphere, mBaseSphere, mRadius, move.normalized, move.magnitude, mLayerMask, mQueryTrigger);
        return hits.Length;
    }

    // Returns count of hits
    public int CapsuleCastNonAlloc(Vector3 move, RaycastHit[] hits)
    {
        return Physics.CapsuleCastNonAlloc(mTopSphere, mBaseSphere, mRadius, move.normalized, hits, move.magnitude, mLayerMask, mQueryTrigger);
    }


    // Returns count of overlaps
    public int TopSphereOverlap(out Collider[] colliders)
    {
        colliders = Physics.OverlapSphere(mTopSphere, mRadius, mLayerMask, mQueryTrigger);
        return colliders.Length;
    }

    // Returns count of overlaps
    public int TopSphereOverlapNonAlloc(Collider[] colliders)
    {
        if (colliders is null || colliders.Length == 0)
            return 0;

        return Physics.OverlapSphereNonAlloc(mTopSphere, mRadius, colliders, mLayerMask, mQueryTrigger);
    }

    // Returns true if cast hit something
    public bool TopSphereCast(Vector3 move, out RaycastHit hit)
    {
        return Physics.SphereCast(mTopSphere, mRadius, move.normalized, out hit, move.magnitude, mLayerMask, mQueryTrigger);
    }

    // Returns count of hits
    public int TopSphereCastAll(Vector3 move, out RaycastHit[] hits)
    {
        hits = Physics.SphereCastAll(mTopSphere, mRadius, move.normalized, move.magnitude, mLayerMask, mQueryTrigger);
        return hits.Length;
    }

    // Returns count of hits
    public int TopSphereCastNonAlloc(RaycastHit[] hitResults, Vector3 move)
    {
        if (hitResults is null || hitResults.Length == 0)
            return 0;

        return Physics.SphereCastNonAlloc(mTopSphere, mRadius, move.normalized, hitResults, move.magnitude, mLayerMask, mQueryTrigger);
    }


    // Returns count of overlaps
    public int BaseSphereOverlap(out Collider[] colliders)
    {
        colliders = Physics.OverlapSphere(mBaseSphere, mRadius, mLayerMask, mQueryTrigger);
        return colliders.Length;
    }

    // Returns count of overlaps
    public int BaseSphereOverlapNonAlloc(Collider[] colliders)
    {
        if (colliders is null || colliders.Length == 0)
            return 0;

        return Physics.OverlapSphereNonAlloc(mBaseSphere, mRadius, colliders, mLayerMask, mQueryTrigger);
    }

    // Returns true if cast hit something
    public bool BaseSphereCast(Vector3 move, out RaycastHit hit)
    {
        return Physics.SphereCast(mBaseSphere, mRadius, move.normalized, out hit, move.magnitude, mLayerMask, mQueryTrigger);
    }

    // Returns count of hits
    public int BaseSphereCastAll(Vector3 move, out RaycastHit[] hits)
    {
        hits = Physics.SphereCastAll(mBaseSphere, mRadius, move.normalized, move.magnitude, mLayerMask, mQueryTrigger);
        return hits.Length;
    }

    // Returns count of hits
    public int BaseSphereCastNonAlloc(RaycastHit[] hitResults, Vector3 move)
    {
        if (hitResults is null || hitResults.Length == 0)
            return 0;

        return Physics.SphereCastNonAlloc(mBaseSphere, mRadius, move.normalized, hitResults, move.magnitude, mLayerMask, mQueryTrigger);
    }


    // Moves the Capsule, stops on hit
    // Returns the delta move
    public Vector3 SweepMove(Vector3 move)
    {
        return SweepMove(move, out RaycastHit hit);
    }

    // Moves the Capsule, stops on hit
    // Returns the delta move
    // outs the RaycastHit that stopped the move
    // RaycastHit.collider is null, if no collision occurred an completed the move
    public Vector3 SweepMove(Vector3 move, out RaycastHit hit)
    {
        if (CapsuleCast(move, out hit))
        {
            move = move.normalized * hit.distance;
        }

        position += move;
        return move;
    }


    // Calculates the delta move to resolve penetration with the given collider
    // thisCollider: CapsuleCollider to be used for calculation
    // NOTE: values of thisCollider are changed, but reverted back after the operation
    // Returns true if calculation was successfull
    // If calculation was unsuccessful, moveOut will have value of Vector3.zero
    // collisionOffset: offset to apply when resolving penetration,
    //                  this will result in Capsule moving [ResolveValue + CollisionOffset] away from the penetrating collider
    public bool ComputePenetration(CapsuleCollider thisCollider, out Vector3 moveOut,
        CapsuleCollider collider, Vector3 colliderPosition, Quaternion colliderRotation,
        float collisionOffset = DEFAULT_COLLISION_OFFSET)
    {
        moveOut = Vector3.zero;
        if (thisCollider is null || collider is null || thisCollider == collider)
        {
            return false;
        }

        using (var temp = new TempColliderUser(this, thisCollider))
        {
            // Note: Physics.ComputePenetration does not always return values when the colliders overlap.
            bool result = Physics.ComputePenetration(thisCollider, mPosition, mRotation,
                collider, colliderPosition, colliderRotation, out Vector3 direction, out float distance);

            if (result)
            {
                moveOut += direction * (distance + collisionOffset);
                return true;
            }

            return false;
        }
    }

    // Performs overlap scan and calculates to Vector3 to resolve penetration
    // NOTE: values of thisCollider are changed, but reverted back after the operation
    // collisionOffset: offset to apply when resolving penetration,
    //                  this will result in Capsule moving [ResolveValue + CollisionOffset] away from the penetrating collider
    public bool ResolvePenetrationInfo(CapsuleCollider thisCollider, out Vector3 moveOut,
        float collisionOffset = DEFAULT_COLLISION_OFFSET)
    {
        moveOut = Vector3.zero;

        if (CapsuleOverlap(out Collider[] overlaps) == 0)
        {
            return false;
        }

        using (var temp = new TempColliderUser(this, thisCollider))
        {
            foreach (var otherCollider in overlaps)
            {
                if (otherCollider is null || thisCollider == otherCollider)
                {
                    continue;
                }

                bool computed = Physics.ComputePenetration(thisCollider, mPosition, mRotation,
                    otherCollider, otherCollider.transform.position, otherCollider.transform.rotation,
                    out Vector3 direction, out float distance);

                if (computed)
                {
                    moveOut += direction * (distance + collisionOffset);
                }
            }
        }

        return true;
    }

    // Calls ResolvePenetrationInfo and applies the output to resolve the penetration
    // collisionOffset: offset to apply when resolving penetration,
    //                  this will result in Capsule moving [ResolveValue + CollisionOffset] away from the penetrating collider
    public Vector3 ResolvePenetration(CapsuleCollider thisCollider, float collisionOffset = DEFAULT_COLLISION_OFFSET)
    {
        if (ResolvePenetrationInfo(thisCollider, out Vector3 moveOut, collisionOffset))
        {
            mPosition += moveOut;
            return moveOut;
        }

        return Vector3.zero;
    }

#if UNITY_EDITOR

    public void DrawGizmo(Color color = default)
    {
        GizmosExtensions.DrawWireCapsule(mRotation, mTopSphere, mBaseSphere, mRadius, color);
    }

#endif
}
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
            _collider = collider;
            _radius = _collider.radius;
            _height = _collider.height;

            // write values to CapsuleCollider
            Vector3 worldScale = collider.transform.lossyScale;
            worldScale.x = Mathf.Abs(worldScale.x);
            worldScale.y = Mathf.Abs(worldScale.y);
            worldScale.z = Mathf.Abs(worldScale.z);

            _collider.radius = capsule._radius / Mathf.Max(worldScale.x, worldScale.z);
            _collider.height = capsule._height / worldScale.y;
        }

        public void Dispose()
        {
            // rewrite cached values
            _collider.radius = _radius;
            _collider.height = _height;
        }

        private CapsuleCollider _collider;
        private float _radius;
        private float _height;
    }

    [SerializeField] private Vector3 _position;
    [SerializeField] private Quaternion _rotation;
    [SerializeField] private float _height;
    [SerializeField] private float _radius;

    // cached values for performance
    private Vector3 _topSphere;
    private Vector3 _baseSphere;
    private Vector3 _up;

    private LayerMask _layerMask;
    private QueryTriggerInteraction _queryTrigger;

    public VirtualCapsule(int layerMask = DEFAULT_LAYER_MASK,
        QueryTriggerInteraction queryTrigger = DEFAULT_TRIGGER_QUERY)
    {
        _position = Vector3.zero;
        _rotation = Quaternion.identity;
        _height = 2f;
        _radius = 0.5f;

        _layerMask = layerMask;
        _queryTrigger = queryTrigger;

        _topSphere = default;
        _baseSphere = default;
        _up = default;
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
        float length = Mathf.Max(0, (_height / 2f) - _radius);
        _topSphere = _position + (_up * length);
        _baseSphere = _position + (_up * -length);
    }

    public LayerMask layerMask
    {
        get => _layerMask;
        set => _layerMask = value;
    }
    public QueryTriggerInteraction queryTrigger
    {
        get => _queryTrigger;
        set => _queryTrigger = value;
    }

    public Vector3 position
    {
        get => _position;
        set
        {
            _position = value;
            InternalUpdateCache();
        }
    }
    public Vector3 topSpherePos
    {
        get => _topSphere;
    }
    public Vector3 baseSpherePos
    {
        get => _baseSphere;
    }
    public Vector3 topPos
    {
        get => _topSphere + (_up * _radius);
    }
    public Vector3 basePos
    {
        get => _baseSphere + (_up * -_radius);
    }

    public Quaternion rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            _up = _rotation * Vector3.up;
            InternalUpdateCache();
        }
    }
    public Vector3 forward => _rotation * Vector3.forward;
    public Vector3 backward => _rotation * Vector3.back;
    public Vector3 left => _rotation * Vector3.left;
    public Vector3 right => _rotation * Vector3.right;
    public Vector3 up => _rotation * Vector3.up;
    public Vector3 down => _rotation * Vector3.down;

    public float radius
    {
        get => _radius;
        set
        {
            _radius = value;
            InternalUpdateCache();
        }
    }
    public float diameter
    {
        get => _radius * 2f;
    }
    public float height
    {
        get => _height;
        set
        {
            _height = value;
            InternalUpdateCache();
        }
    }
    public float cylinderHeight
    {
        get => Mathf.Max(0, _height - (_radius * 2f));
    }

    public float sphereVolume
    {
        get => Mathf.PI * _radius * _radius;
    }
    public float cylinderVolume
    {
        get => 2 * Mathf.PI * _radius * cylinderHeight;
    }
    public float volume
    {
        get => sphereVolume + cylinderVolume;
    }

    public bool isSphereShaped
    {
        get => _height <= _radius * 2f;
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
        _rotation = collider.transform.rotation * rotationOffset;
        _position = collider.transform.position + positionOffset;
        _radius = collider.radius * Mathf.Max(worldScale.x, worldScale.z);
        _height = collider.height * worldScale.y;

        // cache values
        _up = _rotation * Vector3.up;
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
        collider.transform.rotation = _rotation * rotationOffset;
        collider.transform.position = _position - positionOffset;
        collider.radius = _radius / Mathf.Max(worldScale.x, worldScale.z);
        collider.height = _height / worldScale.y;
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
        _rotation = controller.transform.rotation;
        _position = controller.transform.position + positionOffset;
        _radius = controller.radius * Mathf.Max(worldScale.x, worldScale.z);
        _height = controller.height * worldScale.y;

        // cache values
        _up = _rotation * Vector3.up;
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
        controller.transform.rotation = _rotation;
        controller.transform.position = _position - positionOffset;
        controller.radius = _radius / Mathf.Max(worldScale.x, worldScale.z);
        controller.height = _height / worldScale.y;
    }

    // Given a point [pos] in worldSpace, returns a point 
    // which is clamped inside or on the surface of this Capsule
    public Vector3 ClampPositionInsideVolume(Vector3 pos)
    {
        var topSphereToPos = pos - _topSphere;
        if (Vector3.Angle(_up, topSphereToPos) <= 90)
        {
            if (topSphereToPos.magnitude > radius)
            {
                pos += -topSphereToPos.normalized * (topSphereToPos.magnitude - radius);
            }

            return pos;
        }

        var baseSphereToPos = pos - _baseSphere;
        if (Vector3.Angle(-_up, baseSphereToPos) <= 90)
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
        return ClampPositionInsideVolume(_position + pos);
    }

    // Returns count of overlaps
    public int CapsuleOverlap(out Collider[] colliders)
    {
        colliders = Physics.OverlapCapsule(_topSphere, _baseSphere, _radius, _layerMask, _queryTrigger);
        return colliders.Length;
    }

    // Returns count of overlaps
    public int CapsuleOverlapNonAlloc(Collider[] colliders)
    {
        if (colliders is null || colliders.Length == 0)
            return 0;

        return Physics.OverlapCapsuleNonAlloc(_topSphere, _baseSphere, _radius, colliders, _layerMask, _queryTrigger);
    }

    // Returns true if cast hit something
    public bool CapsuleCast(Vector3 move, out RaycastHit hit)
    {
        return Physics.CapsuleCast(_topSphere, _baseSphere, _radius, move.normalized, out hit, move.magnitude, _layerMask, _queryTrigger);
    }

    // Returns count of hits
    public int CapsuleCastAll(Vector3 move, out RaycastHit[] hits)
    {
        hits = Physics.CapsuleCastAll(_topSphere, _baseSphere, _radius, move.normalized, move.magnitude, _layerMask, _queryTrigger);
        return hits.Length;
    }

    // Returns count of hits
    public int CapsuleCastNonAlloc(Vector3 move, RaycastHit[] hits)
    {
        return Physics.CapsuleCastNonAlloc(_topSphere, _baseSphere, _radius, move.normalized, hits, move.magnitude, _layerMask, _queryTrigger);
    }


    // Returns count of overlaps
    public int TopSphereOverlap(out Collider[] colliders)
    {
        colliders = Physics.OverlapSphere(_topSphere, _radius, _layerMask, _queryTrigger);
        return colliders.Length;
    }

    // Returns count of overlaps
    public int TopSphereOverlapNonAlloc(Collider[] colliders)
    {
        if (colliders is null || colliders.Length == 0)
            return 0;

        return Physics.OverlapSphereNonAlloc(_topSphere, _radius, colliders, _layerMask, _queryTrigger);
    }

    // Returns true if cast hit something
    public bool TopSphereCast(Vector3 move, out RaycastHit hit)
    {
        return Physics.SphereCast(_topSphere, _radius, move.normalized, out hit, move.magnitude, _layerMask, _queryTrigger);
    }

    // Returns count of hits
    public int TopSphereCastAll(Vector3 move, out RaycastHit[] hits)
    {
        hits = Physics.SphereCastAll(_topSphere, _radius, move.normalized, move.magnitude, _layerMask, _queryTrigger);
        return hits.Length;
    }

    // Returns count of hits
    public int TopSphereCastNonAlloc(RaycastHit[] hitResults, Vector3 move)
    {
        if (hitResults is null || hitResults.Length == 0)
            return 0;

        return Physics.SphereCastNonAlloc(_topSphere, _radius, move.normalized, hitResults, move.magnitude, _layerMask, _queryTrigger);
    }


    // Returns count of overlaps
    public int BaseSphereOverlap(out Collider[] colliders)
    {
        colliders = Physics.OverlapSphere(_baseSphere, _radius, _layerMask, _queryTrigger);
        return colliders.Length;
    }

    // Returns count of overlaps
    public int BaseSphereOverlapNonAlloc(Collider[] colliders)
    {
        if (colliders is null || colliders.Length == 0)
            return 0;

        return Physics.OverlapSphereNonAlloc(_baseSphere, _radius, colliders, _layerMask, _queryTrigger);
    }

    // Returns true if cast hit something
    public bool BaseSphereCast(Vector3 move, out RaycastHit hit)
    {
        return Physics.SphereCast(_baseSphere, _radius, move.normalized, out hit, move.magnitude, _layerMask, _queryTrigger);
    }

    // Returns count of hits
    public int BaseSphereCastAll(Vector3 move, out RaycastHit[] hits)
    {
        hits = Physics.SphereCastAll(_baseSphere, _radius, move.normalized, move.magnitude, _layerMask, _queryTrigger);
        return hits.Length;
    }

    // Returns count of hits
    public int BaseSphereCastNonAlloc(RaycastHit[] hitResults, Vector3 move)
    {
        if (hitResults is null || hitResults.Length == 0)
            return 0;

        return Physics.SphereCastNonAlloc(_baseSphere, _radius, move.normalized, hitResults, move.magnitude, _layerMask, _queryTrigger);
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
            bool result = Physics.ComputePenetration(thisCollider, _position, _rotation,
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

                bool computed = Physics.ComputePenetration(thisCollider, _position, _rotation,
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
            _position += moveOut;
            return moveOut;
        }

        return Vector3.zero;
    }

#if UNITY_EDITOR

    public void DrawGizmo(Color color = default)
    {
        GizmosExtensions.DrawWireCapsule(_rotation, _topSphere, _baseSphere, _radius, color);
    }

#endif
}
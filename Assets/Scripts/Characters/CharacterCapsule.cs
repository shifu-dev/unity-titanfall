using System;
using UnityEngine;

public class CharacterCapsule : MonoBehaviour
{
    public CapsuleCollider CapsuleCollider { get => _CapsuleCollider; }
    [SerializeField] protected CapsuleCollider _CapsuleCollider;

    public bool IsSphereShaped
    {
        get
        {
            return GetWorldHeight <= GetWorldRadius * 2;
        }
    }

    /// Lengths (WorldSpace)
    public float GetWorldRadius
    {
        get
        {
            Vector3 worldScale = _CapsuleCollider.transform.lossyScale;
            return _CapsuleCollider.radius * Mathf.Max(worldScale.x, worldScale.z);
        }
    }

    public float GetWorldHeight
    {
        get
        {
            return _CapsuleCollider.height * _CapsuleCollider.transform.lossyScale.y;
        }
    }

    public float GetWorldCylinderHeight
    {
        get
        {
            float height = GetWorldHeight;
            float tradius = GetWorldRadius * 2;

            return height > tradius ? height - tradius : 0;
        }
    }

    public float GetWorldHalfCylinderHeight
    {
        get
        {
            return GetWorldCylinderHeight / 2;
        }
    }

    /// Volume (WorldSpace)
    public float GetWorldSphereVolume
    {
        get
        {
            return (float)Math.PI * (float)Math.Pow(GetWorldRadius, 2);
        }
    }

    public float GetWorldHalfSphereVolume
    {
        get
        {
            return GetWorldSphereVolume / 2;
        }
    }

    public float GetWorldCylinderVolume
    {
        get
        {
            return 2 * (float)Math.PI * GetWorldRadius * GetWorldCylinderHeight;
        }
    }

    public float GetWorldVolume
    {
        get
        {
            return GetWorldSphereVolume + GetWorldCylinderVolume;
        }
    }

    /// Positions (WorldSpace)
    public Vector3 GetWorldCenter
    {
        get
        {
            return _CapsuleCollider.transform.position + _CapsuleCollider.center;
        }
    }

    public Vector3 GetWorldStartSpherePosition
    {
        get
        {
            return GetWorldCenter + (GetWorldUpVector * GetWorldHalfCylinderHeight);
        }
    }

    public Vector3 GetWorldEndSpherePosition
    {
        get
        {
            return GetWorldCenter + (-GetWorldUpVector * GetWorldHalfCylinderHeight);
        }
    }

    public Vector3 GetWorldTopPosition
    {
        get
        {
            return GetWorldCenter + (GetWorldUpVector * (GetWorldHalfCylinderHeight + GetWorldRadius));
        }
    }

    public Vector3 GetWorldBasePosition
    {
        get
        {
            return GetWorldCenter + (-GetWorldUpVector * (GetWorldHalfCylinderHeight + GetWorldRadius));
        }
    }

    /// Directions (WorldSpace)
    public Vector3 GetWorldForwardVector
    {
        get
        {
            return _CapsuleCollider.transform.forward;
        }
    }

    public Vector3 GetWorldRightVector
    {
        get
        {
            return _CapsuleCollider.transform.right;
        }
    }

    public Vector3 GetWorldUpVector
    {
        get
        {
            return _CapsuleCollider.transform.up;
        }
    }

    //////////////////////////////////////////////////////////////////
    /// UpdateLoop
    //////////////////////////////////////////////////////////////////

    public void Init(Character character)
    {
    }

    public void UpdateImpl()
    {
    }

    public void OnMoved()
    {
    }

    public void OnRotated()
    {
    }

    public void OnResized()
    {
    }

    public void OnScaled()
    {
    }

    //////////////////////////////////////////////////////////////////
    /// Others
    //////////////////////////////////////////////////////////////////

    public void MoveToPosition(Vector3 pos)
    {
        _CapsuleCollider.transform.position = pos;
    }

    public void MovePosition(Vector3 pos)
    {
        _CapsuleCollider.transform.position += pos;
    }

    public void CalculateWorldGeometryValues(out Vector3 startSphere, out Vector3 endSphere, out float radius)
    {
        Vector3 worldCenter = _CapsuleCollider.transform.position + _CapsuleCollider.center;
        Vector3 worldScale = _CapsuleCollider.transform.lossyScale;

        float worldHeight = _CapsuleCollider.height * _CapsuleCollider.transform.lossyScale.y;
        float worldRadius = _CapsuleCollider.radius * Mathf.Max(worldScale.x, worldScale.z);
        float cylinderHeight = worldHeight - worldRadius - worldRadius;

        startSphere = worldCenter + (_CapsuleCollider.transform.up * (cylinderHeight / 2));
        endSphere = worldCenter + (-_CapsuleCollider.transform.up * (cylinderHeight / 2));
        radius = _CapsuleCollider.radius;
    }

    public RaycastHit SweepTest(Vector3 deltaMove)
    {
        CalculateWorldGeometryValues(out Vector3 point1, out Vector3 point2, out float radius);
        Physics.CapsuleCast(point1, point2, radius, deltaMove.normalized, out RaycastHit rayhit, deltaMove.magnitude, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        return rayhit;
    }

    public RaycastHit[] SweepTestAll(Vector3 deltaMove)
    {
        CalculateWorldGeometryValues(out Vector3 point1, out Vector3 point2, out float radius);
        return Physics.CapsuleCastAll(point1, point2, radius, deltaMove.normalized, deltaMove.magnitude, Physics.DefaultRaycastLayers);
    }

    public RaycastHit SweepMove(Vector3 deltaMove, float moveThreshold = 0.0001f)
    {
        RaycastHit hit = SweepTest(deltaMove);
        if (hit.collider == null)
        {
            // no collision occurred, so we made the complete move
            Debug.DrawLine(GetWorldCenter, GetWorldCenter + deltaMove.normalized * 10, Color.red);
            MovePosition(deltaMove);
            return hit;
        }

        // move to the hit position
        // Vector3 deltaMoved = deltaMove.normalized * hit.distance;
        // Vector3 moveDir = Vector3.ProjectOnPlane((hit.point - deltaMove), GetWorldUpVector).normalized;
        Vector3 moveDir = deltaMove.normalized;
        Debug.DrawLine(GetWorldCenter, GetWorldCenter + moveDir * 10, Color.red);
        Vector3 deltaMoved = moveDir * hit.distance;
        // Debug.Log("HitPoint : " + hit.point + " | Distance: " + hit.distance + " | DeltaMove : " + deltaMoved + " (" + deltaMoved.magnitude + ")");

        if (deltaMoved.magnitude < moveThreshold)
        {
            return hit;
        }

        MovePosition(deltaMoved);
        return hit;
    }

    public RaycastHit SweepMoveIfNoHit(Vector3 deltaMove)
    {
        RaycastHit rayhit = SweepTest(deltaMove);
        if (rayhit.collider == null)
        {
            // no collision occurred, so we made the complete move
            MovePosition(deltaMove);
        }

        return rayhit;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(GetWorldStartSpherePosition, GetWorldRadius);
        Gizmos.DrawWireSphere(GetWorldEndSpherePosition, GetWorldRadius);
    }
}
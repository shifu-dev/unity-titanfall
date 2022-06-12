using System;
using UnityEngine;

[Serializable]
public class ColliderCapsuleDeprecated : VirtualCapsuleDeprecated
{
    public Vector3 localPosition
    {
        get => collider.transform.localPosition;
        set => collider.transform.localPosition = value;
    }
    public override Vector3 position
    {
        get => collider.transform.position;
        set => collider.transform.position = value;
    }
    public Quaternion localRotation
    {
        get => collider.transform.localRotation;
        set => collider.transform.localRotation = value;
    }
    public override Quaternion rotation
    {
        get => collider.transform.rotation;
        set => collider.transform.rotation = value;
    }
    public Vector3 localScale
    {
        get => collider.transform.localScale;
        set => collider.transform.localScale = value;
    }
    public override Vector3 scale
    {
        get => collider.transform.lossyScale;
        set => collider.transform.SetWorldScale(value);
    }
    public override Vector3 localCenter
    {
        get => collider.center;
        set => collider.center = value;
    }
    public override float localHeight
    {
        get => collider.height;
        set => collider.height = value;
    }
    public override float localRadius
    {
        get => collider.radius;
        set => collider.radius = value;
    }

    //////////////////////////////////////////////////////////////////
    /// Penetration
    //////////////////////////////////////////////////////////////////

    public override bool ComputePenetration(out Vector3 moveOut, Collider collider, Vector3 colliderPosition, Quaternion colliderRotation)
    {
        moveOut = Vector3.zero;

        if (collider == null || this.collider == null || this.collider == collider)
        {
            return false;
        }

        // Note: Physics.ComputePenetration does not always return values when the colliders overlap.
        var result = Physics.ComputePenetration(this.collider, this.position, this.rotation,
            collider, colliderPosition, colliderRotation, out Vector3 direction, out float distance);

        if (result)
        {
            moveOut = direction * distance;
            return true;
        }

        return false;
    }

    public override bool ResolvePenetrationInfo(out Vector3 moveOut, float collisionOffset = 0f)
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

        return true;
    }

    public override Vector3 ResolvePenetration(float collisionOffset = 0f)
    {
        if (ResolvePenetrationInfo(out var moveOut, collisionOffset))
        {
            position += moveOut;
            return moveOut;
        }

        return Vector3.zero;
    }
}
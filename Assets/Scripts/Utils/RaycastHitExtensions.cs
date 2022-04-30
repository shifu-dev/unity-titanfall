using UnityEngine;

public static class RaycastHitExtensions
{
    public static bool IsHit(this RaycastHit hit)
    {
        return hit.collider != null;
    }

    public static void Clear(this RaycastHit hit)
    {
    }
}
using UnityEngine;

public static class RaycastHitExtensions
{
    /// <summary>
    /// checks if this represents a valid hit
    /// </summary>
    /// <param name="hit"></param>
    /// <returns>returns true if collider != null</returns>
    public static bool IsHit(this RaycastHit hit)
    {
        return hit.collider != null;
    }

    public static void Clear(this RaycastHit hit)
    {
    }
}
namespace UnityEngine
{
    public static class TransformExtensions
    {
        public static void SetWorldScale(this Transform transform, Vector3 globalScale)
        {
            Vector3 worldScale = transform.lossyScale;
            transform.localScale = new Vector3(globalScale.x / worldScale.x,
                globalScale.y / worldScale.y, globalScale.z / worldScale.z);
        }
    }
}
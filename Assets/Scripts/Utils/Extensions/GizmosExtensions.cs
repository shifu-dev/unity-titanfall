using UnityEditor;

namespace UnityEngine
{
    public static class GizmosExtensions
    {
        public static void DrawWireCapsule(Vector3 pos, Quaternion rot, float height, float radius, Color color = default(Color))
        {
            if (color != default(Color))
            {
                Handles.color = color;
            }

            Matrix4x4 angleMatrix = Matrix4x4.TRS(pos, rot, Handles.matrix.lossyScale);
            using (new Handles.DrawingScope(angleMatrix))
            {
                var pointOffset = (height - (radius * 2)) / 2;

                //draw sideways
                Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.left, Vector3.back, -180, radius);
                Handles.DrawLine(new Vector3(0, pointOffset, -radius), new Vector3(0, -pointOffset, -radius));
                Handles.DrawLine(new Vector3(0, pointOffset, radius), new Vector3(0, -pointOffset, radius));
                Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.left, Vector3.back, 180, radius);
                //draw frontways
                Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.back, Vector3.left, 180, radius);
                Handles.DrawLine(new Vector3(-radius, pointOffset, 0), new Vector3(-radius, -pointOffset, 0));
                Handles.DrawLine(new Vector3(radius, pointOffset, 0), new Vector3(radius, -pointOffset, 0));
                Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.back, Vector3.left, -180, radius);
                //draw center
                Handles.DrawWireDisc(Vector3.up * pointOffset, Vector3.up, radius);
                Handles.DrawWireDisc(Vector3.down * pointOffset, Vector3.up, radius);

            }
        }

        public static void DrawWireCapsule(Quaternion rot, Vector3 upper, Vector3 lower, float radius, Color color)
        {
            var prevColor = Handles.color;
            Handles.color = color;

            Debug.Log(rot.eulerAngles);
            var space = Matrix4x4.TRS(Vector3.zero, rot, Handles.matrix.lossyScale);
            using (new Handles.DrawingScope(space))
            {
                var offsetX = new Vector3(radius, 0f, 0f);
                var offsetZ = new Vector3(0f, 0f, radius);

                //draw frontways
                Handles.DrawWireArc(upper, Vector3.back, Vector3.left, 180, radius);
                Handles.DrawLine(lower + offsetX, upper + offsetX);
                Handles.DrawLine(lower - offsetX, upper - offsetX);
                Handles.DrawWireArc(lower, Vector3.back, Vector3.left, -180, radius);

                //draw sideways
                Handles.DrawWireArc(upper, Vector3.left, Vector3.back, -180, radius);
                Handles.DrawLine(lower + offsetZ, upper + offsetZ);
                Handles.DrawLine(lower - offsetZ, upper - offsetZ);
                Handles.DrawWireArc(lower, Vector3.left, Vector3.back, 180, radius);

                //draw center
                Handles.DrawWireDisc(upper, Vector3.up, radius);
                Handles.DrawWireDisc(lower, Vector3.up, radius);
            }

            Handles.color = prevColor;
        }
    }
}
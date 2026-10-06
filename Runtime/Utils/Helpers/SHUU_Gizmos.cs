#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace SHUU.Utils.Helpers
{
    public static class SHUU_Gizmos
    {
        #region Colors
        public static readonly Color Blue = new Color(0.35f, 0.85f, 1f);
        public static readonly Color Orange = new Color(1f, 0.6f, 0.25f);
        public static readonly Color Green = new Color(0.45f, 0.95f, 0.5f);
        public static readonly Color Red = new Color(1f, 0.4f, 0.4f);
        public static readonly Color Yellow = new Color(1f, 0.9f, 0.4f);
        public static readonly Color Purple = new Color(0.75f, 0.55f, 1f);
        public static readonly Color Grey = new Color(0.7f, 0.7f, 0.7f);


        public static Color WithAlpha(this Color color, float alpha)
        {
            color.a = alpha;

            return color;
        }
        #endregion



        #region Sizes
        public static float Size(Vector3 position, float size) => HandleUtility.GetHandleSize(position) * size;
        #endregion



        #region Shapes
        public static void Line(Vector3 from, Vector3 to, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawLine(from, to);
        }

        public static void Sphere(Vector3 center, float radius, Color color)
        {
            if (radius <= 0f) return;

            Gizmos.color = color;
            Gizmos.DrawWireSphere(center, radius);
        }

        public static void Cross(Vector3 center, float size, Color color)
        {
            Line(center - Vector3.right * size, center + Vector3.right * size, color);
            Line(center - Vector3.up * size, center + Vector3.up * size, color);
            Line(center - Vector3.forward * size, center + Vector3.forward * size, color);
        }

        public static void Beam(Vector3 origin, Vector3 direction, float length, Color color, bool head = true, float headSize = 0.04f)
        {
            Vector3 end = origin + direction.normalized * length;

            Line(origin, end, color);
            if (head) Sphere(end, Size(end, headSize), color);
        }

        public static void Head(Vector3 tip, Vector3 direction, Color color)
        {
            if (direction.sqrMagnitude < 1e-10f) return;

            Vector3 forward = direction.normalized;
            Perpendiculars(forward, out Vector3 side, out Vector3 other);

            float length = Size(tip, 0.1f);
            float width = length * 0.4f;
            Vector3 back = tip - forward * length;

            Line(tip, back + side * width, color);
            Line(tip, back - side * width, color);
            Line(tip, back + other * width, color);
            Line(tip, back - other * width, color);
        }

        public static void Perpendiculars(Vector3 forward, out Vector3 side, out Vector3 other)
        {
            side = Vector3.Cross(forward, Mathf.Abs(forward.y) > 0.95f ? Vector3.right : Vector3.up).normalized;
            other = Vector3.Cross(forward, side);
        }

        public static void Box(Vector3 center, Quaternion rotation, Vector3 halfExtents, Color color)
        {
            Matrix4x4 previous = Gizmos.matrix;

            Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
            Gizmos.color = color;
            Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);

            Gizmos.matrix = previous;
        }

        public static void Capsule(Vector3 a, Vector3 b, float radius, Color color)
        {
            Sphere(a, radius, color);
            Sphere(b, radius, color);

            Vector3 axis = b - a;

            if (axis.sqrMagnitude < 1e-10f) return;

            Perpendiculars(axis.normalized, out Vector3 side, out Vector3 other);

            Line(a + side * radius, b + side * radius, color);
            Line(a - side * radius, b - side * radius, color);
            Line(a + other * radius, b + other * radius, color);
            Line(a - other * radius, b - other * radius, color);
        }

        public static void Arrow(Vector3 from, Vector3 to, Color color)
        {
            Line(from, to, color);
            Head(to, to - from, color);
        }

        public static void Arc(Vector3 center, Vector3 axis, Vector3 from, float degrees, Color color, int segments = 24)
        {
            Vector3 previous = center + from;
            Vector3 direction = Vector3.zero;

            for (int i = 1; i <= segments; i++)
            {
                Vector3 next = center + Quaternion.AngleAxis(degrees * i / segments, axis) * from;

                Line(previous, next, color);

                direction = next - previous;
                previous = next;
            }

            Head(previous, direction, color);
        }
        #endregion
    }
}
#endif

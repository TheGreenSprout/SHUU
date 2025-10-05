using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SHUU.Utils.UI
{
    [DisallowMultipleComponent]
    public class SHUU_Gradient : BaseMeshEffect
    {
        [SerializeField] public int subdivisions = 6;


        [SerializeReference] public GradientType gradientType = null;




        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || gradientType == null || !enabled) return;


            gradientType.graphic = graphic;

            gradientType.ModifyMesh_Call(vh, subdivisions);
        }




        #region Gradient Types
        [Serializable]
        public abstract class GradientType {
            [System.NonSerialized] public Graphic graphic;



            public virtual void ModifyMesh_Call(VertexHelper vh, int subdivisions)
            {
                UIGradientUtils.EnsureSubdivisions(vh, graphic.rectTransform.rect, GetEffectiveSubdivs(subdivisions));
                ModifyMesh(vh);
            }


            public virtual void ModifyMesh(VertexHelper vh) { }

            public abstract Vector2Int GetEffectiveSubdivs(int subdivisions);
        }


        [Serializable]
        public class UIGradient : GradientType {
            public Color m_color1 = Color.white;
            public Color m_color2 = Color.white;

            [Range(-180f, 180f)] public float m_angle = 0f;
            [Range(-1f, 1f)] public float m_strength = 0f;
            [Range(0f, 1f)] public float m_midpoint = 0.5f;
            [Range(0.1f, 1f)] public float m_length = 1f;

            public bool m_ignoreRatio = true;

            public override void ModifyMesh(VertexHelper vh)
            {
                Rect rect = graphic.rectTransform.rect;

                // Gradient direction (0° = top→bottom)
                Vector2 dir = UIGradientUtils.RotationDir(m_angle - 90f).normalized;

                // Calculate projection range using rect corners
                Vector2[] corners = new Vector2[4] {
                    new Vector2(rect.xMin, rect.yMin),
                    new Vector2(rect.xMin, rect.yMax),
                    new Vector2(rect.xMax, rect.yMin),
                    new Vector2(rect.xMax, rect.yMax)
                };

                float minProj = float.MaxValue;
                float maxProj = float.MinValue;
                for (int i = 0; i < 4; i++)
                {
                    float proj = Vector2.Dot(corners[i], dir);
                    if (proj < minProj) minProj = proj;
                    if (proj > maxProj) maxProj = proj;
                }

                float projRange = maxProj - minProj;

                UIVertex vertex = default;
                for (int i = 0; i < vh.currentVertCount; i++)
                {
                    vh.PopulateUIVertex(ref vertex, i);

                    // Project position along axis, normalize to 0–1
                    float proj = Vector2.Dot((Vector2)vertex.position, dir);
                    float t = (proj - minProj) / projRange;

                    // Apply length + midpoint
                    float halfLength = m_length * 0.5f;
                    float start = m_midpoint - halfLength;
                    float end = m_midpoint + halfLength;

                    if (t <= start) t = 0f;
                    else if (t >= end) t = 1f;
                    else t = (t - start) / (end - start);

                    // Apply strength bias
                    t = Mathf.Lerp(t, m_strength > 0 ? 1f : 0f, Mathf.Abs(m_strength));

                    // Apply color
                    vertex.color *= Color.Lerp(m_color1, m_color2, t);
                    vh.SetUIVertex(vertex, i);
                }
            }
            
            public override Vector2Int GetEffectiveSubdivs(int subdivisions)
            {
                int effective = Mathf.CeilToInt(subdivisions / Mathf.Max(0.01f, m_length));

                return new Vector2Int(effective, effective);
            }
        }
        [Serializable]
        public class UICornersGradient : GradientType {
            public Color m_topLeftColor = Color.white;
            public Color m_topRightColor = Color.white;
            public Color m_bottomRightColor = Color.white;
            public Color m_bottomLeftColor = Color.white;

            [Range(-1f, 1f)] public float m_strengthX = 0f;
            [Range(-1f, 1f)] public float m_strengthY = 0f;
            [Range(0f, 1f)] public float m_midpointX = 0.5f;
            [Range(0f, 1f)] public float m_midpointY = 0.5f;
            [Range(0.1f, 1f)] public float m_lengthX = 1f;
            [Range(0.1f, 1f)] public float m_lengthY = 1f;

            public override void ModifyMesh(VertexHelper vh)
            {
                Vector2 m_strength = new Vector2(m_strengthX, m_strengthY);
                Vector2 m_midpoint = new Vector2(m_midpointX, m_midpointY);
                Vector2 m_length = new Vector2(m_lengthX, m_lengthY);


                Rect rect = graphic.rectTransform.rect;
                var localPositionMatrix = UIGradientUtils.LocalPositionMatrix(rect, Vector2.right);

                UIVertex vertex = default;
                for (int i = 0; i < vh.currentVertCount; i++) {
                    vh.PopulateUIVertex(ref vertex, i);

                    // Normalized local position
                    Vector2 norm = localPositionMatrix * vertex.position;

                    // --- Apply length + midpoint per axis ---
                    norm.x = ApplyLengthMidpoint(norm.x, m_midpoint.x, m_length.x);
                    norm.y = ApplyLengthMidpoint(norm.y, m_midpoint.y, m_length.y);

                    // --- Apply strength biases ---
                    float bx = Mathf.Lerp(norm.x, m_strength.x > 0 ? 1f : 0f, Mathf.Abs(m_strength.x));
                    float by = Mathf.Lerp(norm.y, m_strength.y > 0 ? 1f : 0f, Mathf.Abs(m_strength.y));

                    // Bilinear blend
                    Color c = UIGradientUtils.Bilerp(
                        m_bottomLeftColor,
                        m_bottomRightColor,
                        m_topLeftColor,
                        m_topRightColor,
                        new Vector2(bx, by)
                    );

                    vertex.color *= c;
                    vh.SetUIVertex(vertex, i);
                }
            }
            private float ApplyLengthMidpoint(float t, float midpoint, float length) {
                float half = length * 0.5f;
                float start = midpoint - half;
                float end   = midpoint + half;

                if (t <= start) return 0f;
                if (t >= end)   return 1f;
                return (t - start) / (end - start);
            }

            public override Vector2Int GetEffectiveSubdivs(int subdivisions)
            {
                int effectiveX = Mathf.CeilToInt(subdivisions / Mathf.Max(0.01f, m_lengthX));
                int effectiveY = Mathf.CeilToInt(subdivisions / Mathf.Max(0.01f, m_lengthY));


                return new Vector2Int(effectiveX, effectiveY);
            }
        }

        [Serializable]
        public class UITextGradient : GradientType {
            public Color m_color1 = Color.white;
            public Color m_color2 = Color.white;
            [Range(-180f, 180f)] public float m_angle = 0f;
            [Range(-1f, 1f)] public float m_strength = 0f;
            [Range(0f, 1f)] public float m_midpoint = 0.5f;
            [Range(0.1f, 1f)] public float m_length = 1f;

            public override void ModifyMesh(VertexHelper vh)
            {
                Rect rect = graphic.rectTransform.rect;

                Vector2 dir = UIGradientUtils.RotationDir(m_angle - 90f).normalized;

                // Projection range from corners
                Vector2[] corners = new Vector2[4] {
                    new Vector2(rect.xMin, rect.yMin),
                    new Vector2(rect.xMin, rect.yMax),
                    new Vector2(rect.xMax, rect.yMin),
                    new Vector2(rect.xMax, rect.yMax)
                };

                float minProj = float.MaxValue;
                float maxProj = float.MinValue;
                for (int i = 0; i < 4; i++) {
                    float proj = Vector2.Dot(corners[i], dir);
                    if (proj < minProj) minProj = proj;
                    if (proj > maxProj) maxProj = proj;
                }

                float projRange = maxProj - minProj;

                UIVertex vertex = default;
                for (int i = 0; i < vh.currentVertCount; i++) {
                    vh.PopulateUIVertex(ref vertex, i);

                    float proj = Vector2.Dot((Vector2)vertex.position, dir);
                    float t = (proj - minProj) / projRange;

                    // --- Length + Midpoint logic ---
                    float halfLength = m_length * 0.5f;
                    float start = m_midpoint - halfLength;
                    float end = m_midpoint + halfLength;

                    if (t <= start) t = 0f;
                    else if (t >= end) t = 1f;
                    else t = (t - start) / (end - start);

                    // Strength bias
                    t = Mathf.Lerp(t, m_strength > 0 ? 1f : 0f, Mathf.Abs(m_strength));

                    // Apply color
                    vertex.color *= Color.Lerp(m_color1, m_color2, t);
                    vh.SetUIVertex(vertex, i);
                }
            }
            
            public override Vector2Int GetEffectiveSubdivs(int subdivisions)
            {
                return Vector2Int.zero;
            }
        }
        [Serializable]
        public class UITextCornersGradient : GradientType {
            public Color m_topLeftColor = Color.white;
            public Color m_topRightColor = Color.white;
            public Color m_bottomRightColor = Color.white;
            public Color m_bottomLeftColor = Color.white;

            [Range(-1f, 1f)] public float m_strengthX = 0f;
            [Range(-1f, 1f)] public float m_strengthY = 0f;
            [Range(0f, 1f)] public float m_midpointX = 0.5f;
            [Range(0f, 1f)] public float m_midpointY = 0.5f;
            [Range(0.1f, 1f)] public float m_lengthX = 1f;
            [Range(0.1f, 1f)] public float m_lengthY = 1f;

            public override void ModifyMesh(VertexHelper vh)
            {
                Vector2 m_strength = new Vector2(m_strengthX, m_strengthY);
                Vector2 m_midpoint = new Vector2(m_midpointX, m_midpointY);
                Vector2 m_length = new Vector2(m_lengthX, m_lengthY);


                Rect rect = graphic.rectTransform.rect;
                Vector2 rectMin = rect.min;
                Vector2 rectSize = rect.size;

                UIVertex vertex = default;
                for (int i = 0; i < vh.currentVertCount; i++) {
                    vh.PopulateUIVertex(ref vertex, i);

                    // Normalized [0–1] position
                    Vector2 norm = new Vector2(
                        (vertex.position.x - rectMin.x) / rectSize.x,
                        (vertex.position.y - rectMin.y) / rectSize.y
                    );

                    // --- Apply length + midpoint per axis ---
                    norm.x = ApplyLengthMidpoint(norm.x, m_midpoint.x, m_length.x);
                    norm.y = ApplyLengthMidpoint(norm.y, m_midpoint.y, m_length.y);

                    // --- Apply strength biases ---
                    float bx = Mathf.Lerp(norm.x, m_strength.x > 0 ? 1f : 0f, Mathf.Abs(m_strength.y));
                    float by = Mathf.Lerp(norm.y, m_strength.y > 0 ? 1f : 0f, Mathf.Abs(m_strength.y));

                    // Bilinear color blend
                    Color c = UIGradientUtils.Bilerp(
                        m_bottomLeftColor,
                        m_bottomRightColor,
                        m_topLeftColor,
                        m_topRightColor,
                        new Vector2(bx, by)
                    );

                    vertex.color *= c;
                    vh.SetUIVertex(vertex, i);
                }
            }
            private float ApplyLengthMidpoint(float t, float midpoint, float length) {
                float half = length * 0.5f;
                float start = midpoint - half;
                float end   = midpoint + half;

                if (t <= start) return 0f;
                if (t >= end)   return 1f;
                return (t - start) / (end - start);
            }

            public override Vector2Int GetEffectiveSubdivs(int subdivisions)
            {
                return Vector2Int.zero;
            }
        }
        #endregion



        #region Gradient Utils
        private class UIGradientUtils
        {
            public struct Matrix2x3
            {
                public float m00, m01, m02, m10, m11, m12;
                public Matrix2x3(float m00, float m01, float m02, float m10, float m11, float m12)
                {
                    this.m00 = m00;
                    this.m01 = m01;
                    this.m02 = m02;
                    this.m10 = m10;
                    this.m11 = m11;
                    this.m12 = m12;
                }

                public static Vector2 operator *(Matrix2x3 m, Vector2 v)
                {
                    float x = (m.m00 * v.x) - (m.m01 * v.y) + m.m02;
                    float y = (m.m10 * v.x) + (m.m11 * v.y) + m.m12;
                    return new Vector2(x, y);
                }
            }

            public static Matrix2x3 LocalPositionMatrix(Rect rect, Vector2 dir)
            {
                float cos = dir.x;
                float sin = dir.y;
                Vector2 rectMin = rect.min;
                Vector2 rectSize = rect.size;
                float c = 0.5f;
                float ax = rectMin.x / rectSize.x + c;
                float ay = rectMin.y / rectSize.y + c;
                float m00 = cos / rectSize.x;
                float m01 = sin / rectSize.y;
                float m02 = -(ax * cos - ay * sin - c);
                float m10 = sin / rectSize.x;
                float m11 = cos / rectSize.y;
                float m12 = -(ax * sin + ay * cos - c);
                return new Matrix2x3(m00, m01, m02, m10, m11, m12);
            }

            static Vector2[] ms_verticesPositions = new Vector2[] { Vector2.up, Vector2.one, Vector2.right, Vector2.zero };
            public static Vector2[] VerticePositions
            {
                get { return ms_verticesPositions; }
            }

            public static Vector2 RotationDir(float angle)
            {
                float angleRad = angle * Mathf.Deg2Rad;
                float cos = Mathf.Cos(angleRad);
                float sin = Mathf.Sin(angleRad);
                return new Vector2(cos, sin);
            }

            public static Vector2 CompensateAspectRatio(Rect rect, Vector2 dir)
            {
                float ratio = rect.height / rect.width;
                dir.x *= ratio;
                return dir.normalized;
            }

            public static float InverseLerp(float a, float b, float v)
            {
                return a != b ? (v - a) / (b - a) : 0f;
            }

            public static Color Bilerp(Color a1, Color a2, Color b1, Color b2, Vector2 t)
            {
                Color a = Color.LerpUnclamped(a1, a2, t.x);
                Color b = Color.LerpUnclamped(b1, b2, t.x);
                return Color.LerpUnclamped(a, b, t.y);
            }

            public static void Lerp(UIVertex a, UIVertex b, float t, ref UIVertex c)
            {
                c.position = Vector3.LerpUnclamped(a.position, b.position, t);
                c.normal = Vector3.LerpUnclamped(a.normal, b.normal, t);
                c.color = Color32.LerpUnclamped(a.color, b.color, t);
                c.tangent = Vector3.LerpUnclamped(a.tangent, b.tangent, t);
                c.uv0 = Vector3.LerpUnclamped(a.uv0, b.uv0, t);
                c.uv1 = Vector3.LerpUnclamped(a.uv1, b.uv1, t);
                // c.uv2 = Vector3.LerpUnclamped(a.uv2, b.uv2, t);
                // c.uv3 = Vector3.LerpUnclamped(a.uv3, b.uv3, t);		
            }


            public static void EnsureSubdivisions(VertexHelper vh, Rect rect, Vector2Int subdivisions)
            {
                if (subdivisions.x <= 1 && subdivisions.y <= 1) return;

                var verts = new List<UIVertex>();
                vh.GetUIVertexStream(verts);
                vh.Clear();

                var gridVerts = new List<UIVertex>();

                // Build grid of vertices
                for (int y = 0; y <= subdivisions.y; y++)
                {
                    float fy = (float)y / subdivisions.y;
                    for (int x = 0; x <= subdivisions.x; x++)
                    {
                        float fx = (float)x / subdivisions.x;

                        var v = new UIVertex();
                        v.position = new Vector3(rect.xMin + fx * rect.width, rect.yMin + fy * rect.height);
                        v.uv0 = new Vector2(fx, fy);
                        v.color = Color.white;
                        gridVerts.Add(v);
                    }
                }

                // Build quads into triangles
                for (int y = 0; y < subdivisions.y; y++)
                {
                    for (int x = 0; x < subdivisions.x; x++)
                    {
                        int i0 = y * (subdivisions.x + 1) + x;
                        int i1 = i0 + 1;
                        int i2 = i0 + (subdivisions.x + 1);
                        int i3 = i2 + 1;

                        vh.AddVert(gridVerts[i0]);
                        vh.AddVert(gridVerts[i1]);
                        vh.AddVert(gridVerts[i2]);
                        vh.AddVert(gridVerts[i3]);

                        int baseIndex = vh.currentVertCount - 4;
                        vh.AddTriangle(baseIndex, baseIndex + 2, baseIndex + 1);
                        vh.AddTriangle(baseIndex + 1, baseIndex + 2, baseIndex + 3);
                    }
                }
            }
        }
        #endregion
    }
}

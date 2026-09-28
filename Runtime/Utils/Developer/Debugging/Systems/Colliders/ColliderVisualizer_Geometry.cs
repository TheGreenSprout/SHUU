/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



using System.Collections.Generic;
using UnityEngine;

namespace SHUU.Utils.Developer.Debugging.Systems
{
    internal static class ColliderVisualizer_Geometry
    {
        private const float SizeFraction = 0.0006f;

        private static Vector3 PushedVertex(Vector3[] verts, Vector3[] normals, bool hasNormals, float push, int index)
            => hasNormals ? verts[index] + normals[index] * push : verts[index];


        #region Wire geometry
        internal static void BuildWire(Collider col, List<Vector3> v, List<int> idx)
        {
            if (col is BoxCollider box) DrawBox(box, v, idx);
            else if (col is SphereCollider s) DrawSphere(s, v, idx);
            else if (col is CapsuleCollider cap) DrawCapsule(cap, v, idx);
            else if (col is MeshCollider mc) DrawMeshCollider(mc, v, idx);
        }

        private static void DrawBox(BoxCollider box, List<Vector3> v, List<int> idx)
        {
            Transform t = box.transform;
            Matrix4x4 m = Matrix4x4.TRS(t.TransformPoint(box.center), t.rotation, Vector3.Scale(box.size * (1f + SizeFraction), t.lossyScale));

            int b = v.Count;
            v.Add(m.MultiplyPoint3x4(new(-.5f, -.5f, -.5f)));
            v.Add(m.MultiplyPoint3x4(new(.5f, -.5f, -.5f)));
            v.Add(m.MultiplyPoint3x4(new(.5f, -.5f, .5f)));
            v.Add(m.MultiplyPoint3x4(new(-.5f, -.5f, .5f)));
            v.Add(m.MultiplyPoint3x4(new(-.5f, .5f, -.5f)));
            v.Add(m.MultiplyPoint3x4(new(.5f, .5f, -.5f)));
            v.Add(m.MultiplyPoint3x4(new(.5f, .5f, .5f)));
            v.Add(m.MultiplyPoint3x4(new(-.5f, .5f, .5f)));

            int[] edges = { 0,1, 1,2, 2,3, 3,0, 4,5, 5,6, 6,7, 7,4, 0,4, 1,5, 2,6, 3,7 };
            foreach (int e in edges) idx.Add(b + e);
        }

        private static void DrawSphere(SphereCollider s, List<Vector3> v, List<int> idx, int seg = 16)
        {
            Transform t = s.transform;
            Vector3 c = t.TransformPoint(s.center);
            float r = s.radius * Mathf.Max(t.lossyScale.x, t.lossyScale.y, t.lossyScale.z) * (1f + SizeFraction);

            DrawCircle(c, t.right, t.up, r, seg, v, idx);
            DrawCircle(c, t.up, t.forward, r, seg, v, idx);
            DrawCircle(c, t.forward, t.right, r, seg, v, idx);
        }

        private static void DrawCapsule(CapsuleCollider c, List<Vector3> v, List<int> idx, int seg = 12, int lat = 8)
        {
            Transform t = c.transform;
            Vector3 center = t.TransformPoint(c.center);

            Vector3 axis = c.direction == 0 ? t.right : c.direction == 1 ? t.up : t.forward;

            Vector3 orthoA = Mathf.Abs(Vector3.Dot(axis, t.up)) < 0.99f ? Vector3.Cross(axis, t.up).normalized : Vector3.Cross(axis, t.right).normalized;
            Vector3 orthoB = Vector3.Cross(axis, orthoA).normalized;

            float scale = Mathf.Max(t.lossyScale.x, t.lossyScale.y, t.lossyScale.z);
            float r = c.radius * scale * (1f + SizeFraction);
            float h = Mathf.Max(0, c.height * scale - 2 * c.radius * scale);

            Vector3 top = center + axis * h * 0.5f;
            Vector3 bottom = center - axis * h * 0.5f;

            for (int i = 0; i < seg; i++)
            {
                float a0 = i * Mathf.PI * 2f / seg;
                float a1 = (i + 1) * Mathf.PI * 2f / seg;
                Vector3 o0 = Mathf.Cos(a0) * orthoA * r + Mathf.Sin(a0) * orthoB * r;
                Vector3 o1 = Mathf.Cos(a1) * orthoA * r + Mathf.Sin(a1) * orthoB * r;

                int b = v.Count;
                v.Add(top + o0);
                v.Add(bottom + o0);
                v.Add(top + o0);
                v.Add(top + o1);
                v.Add(bottom + o0);
                v.Add(bottom + o1);
                for (int j = 0; j < 6; j++) idx.Add(b + j);
            }

            AddHemisphereWire(top, axis, orthoA, orthoB, r, true, lat, seg, v, idx);
            AddHemisphereWire(bottom, axis, orthoA, orthoB, r, false, lat, seg, v, idx);
        }

        private static void DrawMeshCollider(MeshCollider mc, List<Vector3> v, List<int> idx)
        {
            if (!mc.sharedMesh) return;

            Mesh mesh = mc.sharedMesh;
            Transform t = mc.transform;
            Vector3[] verts = mesh.vertices;
            int[] tris = mesh.triangles;

            Vector3[] normals = mesh.normals;
            bool hasNormals = normals != null && normals.Length == verts.Length;
            float push = mesh.bounds.extents.magnitude * SizeFraction;

            for (int i = 0; i < tris.Length; i += 3)
            {
                int b = v.Count;
                v.Add(t.TransformPoint(PushedVertex(verts, normals, hasNormals, push, tris[i])));
                v.Add(t.TransformPoint(PushedVertex(verts, normals, hasNormals, push, tris[i + 1])));
                v.Add(t.TransformPoint(PushedVertex(verts, normals, hasNormals, push, tris[i + 2])));
                idx.Add(b);
                idx.Add(b + 1);
                idx.Add(b + 1);
                idx.Add(b + 2);
                idx.Add(b + 2);
                idx.Add(b);
            }
        }

        private static void AddHemisphereWire( Vector3 center, Vector3 axis, Vector3 orthoA, Vector3 orthoB, float r, bool top, int lat, int seg, List<Vector3> v, List<int> idx)
        {
            float dir = top ? 1f : -1f;

            for (int y = 0; y < lat; y++)
            {
                float a0 = y / (float)lat * Mathf.PI * 0.5f;
                float a1 = (y + 1) / (float)lat * Mathf.PI * 0.5f;
                float s0 = Mathf.Sin(a0), s1 = Mathf.Sin(a1);
                float c0 = Mathf.Cos(a0), c1 = Mathf.Cos(a1);

                for (int i = 0; i < seg; i++)
                {
                    float u0 = i * Mathf.PI * 2f / seg;
                    float u1 = (i + 1) * Mathf.PI * 2f / seg;

                    Vector3 p00 = center + axis * (c0 * r * dir) + (Mathf.Cos(u0) * orthoA + Mathf.Sin(u0) * orthoB) * s0 * r;
                    Vector3 p01 = center + axis * (c0 * r * dir) + (Mathf.Cos(u1) * orthoA + Mathf.Sin(u1) * orthoB) * s0 * r;
                    Vector3 p10 = center + axis * (c1 * r * dir) + (Mathf.Cos(u0) * orthoA + Mathf.Sin(u0) * orthoB) * s1 * r;

                    int b = v.Count;
                    v.Add(p00);
                    v.Add(p01);
                    v.Add(p00);
                    v.Add(p10);
                    idx.Add(b);
                    idx.Add(b + 1);
                    idx.Add(b + 2);
                    idx.Add(b + 3);
                }
            }
        }

        private static void DrawCircle(Vector3 c, Vector3 a, Vector3 b, float r, int seg, List<Vector3> v, List<int> idx)
        {
            for (int i = 0; i < seg; i++)
            {
                float t1 = i * Mathf.PI * 2f / seg;
                float t2 = (i + 1) * Mathf.PI * 2f / seg;

                int bi = v.Count;
                v.Add(c + (Mathf.Cos(t1) * a + Mathf.Sin(t1) * b) * r);
                v.Add(c + (Mathf.Cos(t2) * a + Mathf.Sin(t2) * b) * r);
                idx.Add(bi); idx.Add(bi + 1);
            }
        }
        #endregion



        #region Fill geometry
        internal static void BuildFill(Collider col, List<Vector3> v, List<int> idx)
        {
            if (col is BoxCollider box) FillBox(box, v, idx);
            else if (col is SphereCollider s) FillSphere(s, v, idx);
            else if (col is CapsuleCollider cap) FillCapsule(cap, v, idx);
            else if (col is MeshCollider mc) FillMeshCollider(mc, v, idx);
        }

        private static void FillBox(BoxCollider box, List<Vector3> v, List<int> idx)
        {
            Transform t = box.transform;
            Matrix4x4 m = Matrix4x4.TRS(t.TransformPoint(box.center), t.rotation, Vector3.Scale(box.size * (1f + SizeFraction), t.lossyScale));

            int b = v.Count;
            v.Add(m.MultiplyPoint3x4(new(-.5f, -.5f, -.5f)));
            v.Add(m.MultiplyPoint3x4(new( .5f, -.5f, -.5f)));
            v.Add(m.MultiplyPoint3x4(new( .5f, -.5f,  .5f)));
            v.Add(m.MultiplyPoint3x4(new(-.5f, -.5f,  .5f)));
            v.Add(m.MultiplyPoint3x4(new(-.5f,  .5f, -.5f)));
            v.Add(m.MultiplyPoint3x4(new( .5f,  .5f, -.5f)));
            v.Add(m.MultiplyPoint3x4(new( .5f,  .5f,  .5f)));
            v.Add(m.MultiplyPoint3x4(new(-.5f,  .5f,  .5f)));

            int[] ti = { 0,1,2, 0,2,3, 4,6,5, 4,7,6, 0,4,5, 0,5,1, 1,5,6, 1,6,2, 2,6,7, 2,7,3, 3,7,4, 3,4,0 };
            foreach (int e in ti) idx.Add(b + e);
        }

        private static void FillSphere(SphereCollider s, List<Vector3> v, List<int> idx, int lat = 10, int lon = 16)
        {
            Transform t = s.transform;
            Vector3 c = t.TransformPoint(s.center);
            float r = s.radius * Mathf.Max(t.lossyScale.x, t.lossyScale.y, t.lossyScale.z) * (1f + SizeFraction);

            for (int y = 0; y < lat; y++)
            {
                float phi0 = Mathf.PI * (y / (float)lat - 0.5f);
                float phi1 = Mathf.PI * ((y + 1) / (float)lat - 0.5f);
                float y0 = Mathf.Sin(phi0), y1 = Mathf.Sin(phi1);
                float r0 = Mathf.Cos(phi0), r1 = Mathf.Cos(phi1);

                for (int x = 0; x < lon; x++)
                {
                    float u0 = x / (float)lon * Mathf.PI * 2f;
                    float u1 = (x + 1) / (float)lon * Mathf.PI * 2f;

                    int b = v.Count;
                    v.Add(c + (t.right * Mathf.Cos(u0) * r0 + t.up * y0 + t.forward * Mathf.Sin(u0) * r0) * r);
                    v.Add(c + (t.right * Mathf.Cos(u1) * r0 + t.up * y0 + t.forward * Mathf.Sin(u1) * r0) * r);
                    v.Add(c + (t.right * Mathf.Cos(u0) * r1 + t.up * y1 + t.forward * Mathf.Sin(u0) * r1) * r);
                    v.Add(c + (t.right * Mathf.Cos(u1) * r1 + t.up * y1 + t.forward * Mathf.Sin(u1) * r1) * r);

                    idx.Add(b);
                    idx.Add(b+2);
                    idx.Add(b+3);
                    idx.Add(b);
                    idx.Add(b+3);
                    idx.Add(b+1);
                }
            }
        }

        private static void FillCapsule(CapsuleCollider c, List<Vector3> v, List<int> idx, int seg = 12, int lat = 8)
        {
            Transform t = c.transform;
            Vector3 center = t.TransformPoint(c.center);

            Vector3 axis = c.direction == 0 ? t.right : c.direction == 1 ? t.up : t.forward;

            Vector3 orthoA = Mathf.Abs(Vector3.Dot(axis, t.up)) < 0.99f ? Vector3.Cross(axis, t.up).normalized : Vector3.Cross(axis, t.right).normalized;
            Vector3 orthoB = Vector3.Cross(axis, orthoA).normalized;

            float scale = Mathf.Max(t.lossyScale.x, t.lossyScale.y, t.lossyScale.z);
            float r = c.radius * scale * (1f + SizeFraction);
            float h = Mathf.Max(0, c.height * scale - 2 * c.radius * scale);

            Vector3 top = center + axis * h * 0.5f;
            Vector3 bottom = center - axis * h * 0.5f;

            for (int i = 0; i < seg; i++)
            {
                float a0 = i * Mathf.PI * 2f / seg;
                float a1 = (i + 1) * Mathf.PI * 2f / seg;
                Vector3 o0 = Mathf.Cos(a0) * orthoA * r + Mathf.Sin(a0) * orthoB * r;
                Vector3 o1 = Mathf.Cos(a1) * orthoA * r + Mathf.Sin(a1) * orthoB * r;

                int b = v.Count;
                v.Add(bottom + o0);
                v.Add(top + o0);
                v.Add(top + o1);
                v.Add(bottom + o1);
                idx.Add(b);
                idx.Add(b+1);
                idx.Add(b+2);
                idx.Add(b);
                idx.Add(b+2);
                idx.Add(b+3);
            }

            FillHemisphere(top, axis, orthoA, orthoB, r, true, lat, seg, v, idx);
            FillHemisphere(bottom, axis, orthoA, orthoB, r, false, lat, seg, v, idx);
        }

        private static void FillMeshCollider(MeshCollider mc, List<Vector3> v, List<int> idx)
        {
            if (!mc.sharedMesh) return;

            Mesh mesh = mc.sharedMesh;
            Transform t = mc.transform;
            Vector3[] verts = mesh.vertices;
            int[] tris = mesh.triangles;

            Vector3[] normals = mesh.normals;
            bool hasNormals = normals != null && normals.Length == verts.Length;
            float push = mesh.bounds.extents.magnitude * SizeFraction;

            int b = v.Count;
            for (int i = 0; i < verts.Length; i++)
                v.Add(t.TransformPoint(PushedVertex(verts, normals, hasNormals, push, i)));
            for (int i = 0; i < tris.Length; i++)
                idx.Add(b + tris[i]);
        }

        private static void FillHemisphere(Vector3 center, Vector3 axis, Vector3 orthoA, Vector3 orthoB, float r, bool top, int lat, int seg, List<Vector3> v, List<int> idx)
        {
            float dir = top ? 1f : -1f;

            for (int y = 0; y < lat; y++)
            {
                float a0 = y / (float)lat * Mathf.PI * 0.5f;
                float a1 = (y + 1) / (float)lat * Mathf.PI * 0.5f;
                float s0 = Mathf.Sin(a0), s1 = Mathf.Sin(a1);
                float c0 = Mathf.Cos(a0), c1 = Mathf.Cos(a1);

                for (int i = 0; i < seg; i++)
                {
                    float u0 = i * Mathf.PI * 2f / seg;
                    float u1 = (i + 1) * Mathf.PI * 2f / seg;

                    int b = v.Count;
                    v.Add(center + axis * (c0 * r * dir) + (Mathf.Cos(u0) * orthoA + Mathf.Sin(u0) * orthoB) * s0 * r);
                    v.Add(center + axis * (c0 * r * dir) + (Mathf.Cos(u1) * orthoA + Mathf.Sin(u1) * orthoB) * s0 * r);
                    v.Add(center + axis * (c1 * r * dir) + (Mathf.Cos(u0) * orthoA + Mathf.Sin(u0) * orthoB) * s1 * r);
                    v.Add(center + axis * (c1 * r * dir) + (Mathf.Cos(u1) * orthoA + Mathf.Sin(u1) * orthoB) * s1 * r);

                    idx.Add(b);
                    idx.Add(b+2);
                    idx.Add(b+3);
                    idx.Add(b);
                    idx.Add(b+3);
                    idx.Add(b+1);
                }
            }
        }
        #endregion
    }
}

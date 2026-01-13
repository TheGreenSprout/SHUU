/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SHUU.Utils.Helpers
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshCollider))]
    public class SplineCollider : MonoBehaviour
    {
        [Header("Spline")]
        public SplineContainer splineContainer;

        [Header("Shape")]
        [Min(0.01f)] public float height = 2f;
        public bool centeredHeight = false;
        public Vector3 localUp = Vector3.up;
        [Min(3)] public int resolution = 64;

        [Header("Gizmos")]
        public bool drawGizmos = true;
        public Color gizmoColor = new Color(0f, 1f, 0f, 0.7f);

        private MeshCollider meshCollider;
        private bool dirty;
        private bool originalEnabled = false;

        // -------------------------------------------------
        // UNITY LIFECYCLE
        // -------------------------------------------------
        private void Awake()
        {
            EnsureCollider();
            MarkDirty();
        }

        private void OnEnable()
        {
            EnsureCollider();
            MarkDirty();

            if (!Application.isPlaying) return;
            originalEnabled = meshCollider.enabled;
            meshCollider.enabled = true;
        }

        private void OnDisable()
        {
            if (!Application.isPlaying) return;

            meshCollider.enabled = originalEnabled;
        }

        private void OnApplicationQuit()
        {
            if (!Application.isPlaying) return;

            meshCollider.enabled = originalEnabled;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            EnsureCollider();
            MarkDirty();
        }
    #endif

        private void LateUpdate()
        {
            if (!dirty)
                return;

            Rebuild();
            dirty = false;
        }

        // -------------------------------------------------
        // PUBLIC API
        // -------------------------------------------------
        public void MarkDirty()
        {
            dirty = true;
        }

        // -------------------------------------------------
        // CORE LOGIC
        // -------------------------------------------------
        private void EnsureCollider()
        {
            if (!meshCollider) meshCollider = GetComponent<MeshCollider>() ?? gameObject.AddComponent<MeshCollider>();
            if (!splineContainer) TryGetComponent(out splineContainer);
        }

        private Vector3 Up => transform.TransformDirection(localUp.normalized);

        private Vector3 HeightOffset => centeredHeight ? -Up * (height * 0.5f) : Vector3.zero;

        public void Rebuild()
        {
            ClearCompoundColliders();

            if (!IsSplineValid())
                return;

            Mesh mesh = GenerateVolumeMesh();

            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;


            if (!meshCollider.enabled)
            {
                meshCollider.enabled = true;
                meshCollider.enabled = false;
            }
        }

        private bool IsSplineValid()
        {
            return splineContainer != null &&
                splineContainer.Spline != null &&
                splineContainer.Spline.Closed;
        }

        // ----------------------------
        // REPLACEMENT: MESH GENERATION
        // ----------------------------
        private Mesh GenerateVolumeMesh()
        {
            // Collect base spline points
            List<Vector3> baseVerts = new();
            for (int i = 0; i < resolution; i++)
            {
                baseVerts.Add(Evaluate(i / (float)resolution) + HeightOffset);
            }

            int vertCount = baseVerts.Count;
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();

            // Add bottom and top vertices
            for (int i = 0; i < vertCount; i++)
            {
                verts.Add(baseVerts[i]);                 // bottom
                verts.Add(baseVerts[i] + Up * height);  // top
            }

            // Side quads
            for (int i = 0; i < vertCount; i++)
            {
                int next = (i + 1) % vertCount;

                int b0 = i * 2;
                int t0 = b0 + 1;
                int b1 = next * 2;
                int t1 = b1 + 1;

                tris.Add(b0); tris.Add(t0); tris.Add(t1);
                tris.Add(b0); tris.Add(t1); tris.Add(b1);
            }

            // Caps
            AddCap(verts, tris, false); // bottom
            AddCap(verts, tris, true);  // top

            return CreateMesh(verts, tris);
        }

        // Improved cap generation using ear-clipping
        private void AddCap(List<Vector3> verts, List<int> tris, bool top)
        {
            int offset = top ? 1 : 0;
            int vertCount = resolution;

            // Project to 2D plane for triangulation
            Vector2[] points2D = new Vector2[vertCount];
            Vector3 origin = verts[offset]; // first vertex as origin
            Vector3 tangent = Vector3.Cross(Up, Vector3.forward).normalized;
            Vector3 bitangent = Vector3.Cross(Up, tangent).normalized;

            for (int i = 0; i < vertCount; i++)
            {
                Vector3 v = verts[i * 2 + offset] - origin;
                points2D[i] = new Vector2(Vector3.Dot(v, tangent), Vector3.Dot(v, bitangent));
            }

            int[] indices = TriangulatePolygon(points2D);

            // Assign to mesh
            for (int i = 0; i < indices.Length; i += 3)
            {
                if (top)
                    tris.AddRange(new[] { indices[i] * 2 + 1, indices[i + 1] * 2 + 1, indices[i + 2] * 2 + 1 });
                else
                    tris.AddRange(new[] { indices[i] * 2, indices[i + 2] * 2, indices[i + 1] * 2 });
            }
        }

        // Ear clipping triangulation
        private int[] TriangulatePolygon(Vector2[] points)
        {
            List<int> indices = new List<int>();
            List<int> V = new List<int>();
            for (int i = 0; i < points.Length; i++) V.Add(i);

            int n = points.Length;
            int count = 0;
            while (n > 3)
            {
                bool earFound = false;
                for (int i = 0; i < n; i++)
                {
                    int prev = V[(i + n - 1) % n];
                    int curr = V[i];
                    int next = V[(i + 1) % n];

                    if (IsEar(prev, curr, next, points, V))
                    {
                        indices.Add(prev);
                        indices.Add(curr);
                        indices.Add(next);
                        V.RemoveAt(i);
                        n--;
                        earFound = true;
                        break;
                    }
                }

                if (!earFound) break; // fallback
                count++;
                if (count > 5000) break; // prevent infinite loop
            }

            if (V.Count == 3)
                indices.AddRange(V);

            return indices.ToArray();
        }

        private bool IsEar(int i0, int i1, int i2, Vector2[] pts, List<int> V)
        {
            Vector2 a = pts[i0], b = pts[i1], c = pts[i2];
            if (Vector2.SignedAngle(b - a, c - b) <= 0) return false;

            for (int j = 0; j < V.Count; j++)
            {
                int vi = V[j];
                if (vi == i0 || vi == i1 || vi == i2) continue;
                if (PointInTriangle(pts[vi], a, b, c)) return false;
            }

            return true;
        }

        private bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float s = a.y * c.x - a.x * c.y + (c.y - a.y) * p.x + (a.x - c.x) * p.y;
            float t = a.x * b.y - a.y * b.x + (a.y - b.y) * p.x + (b.x - a.x) * p.y;

            if ((s < 0) != (t < 0)) return false;

            float A = -b.y * c.x + a.y * (c.x - b.x) + a.x * (b.y - c.y) + b.x * c.y;
            return A < 0 ? (s <= 0 && s + t >= A) : (s >= 0 && s + t <= A);
        }

        private Mesh CreateMesh(List<Vector3> verts, List<int> tris)
        {
            Mesh mesh = new();
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private Vector3 Evaluate(float t)
        {
            return splineContainer.EvaluatePosition(t);
        }

        private void ClearCompoundColliders()
        {
            foreach (Transform c in transform)
                if (c.name.StartsWith("SplineCollider_Segment_"))
                    DestroyImmediate(c.gameObject);
        }

        // -------------------------------------------------
        // GIZMOS
        // -------------------------------------------------
    #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos || !IsSplineValid())
                return;

            Handles.color = gizmoColor;

            if (height <= 0f)
                DrawPlaneGizmo();
            else
                DrawVolumeGizmo();
        }

        private void DrawPlaneGizmo()
        {
            Vector3 prev = Evaluate(0f);

            for (int i = 1; i <= resolution; i++)
            {
                Vector3 p = Evaluate(i / (float)resolution);
                Handles.DrawAAPolyLine(prev, p);
                prev = p;
            }
        }

        private void DrawVolumeGizmo()
        {
            Vector3 prevB = Evaluate(0f) + HeightOffset;
            Vector3 prevT = prevB + Up * height;

            for (int i = 1; i <= resolution; i++)
            {
                Vector3 b = Evaluate(i / (float)resolution) + HeightOffset;
                Vector3 t = b + Up * height;

                Handles.DrawAAPolyLine(prevB, b);
                Handles.DrawAAPolyLine(prevT, t);
                Handles.DrawLine(b, t);

                prevB = b;
                prevT = t;
            }
        }
    #endif
    }
}

/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



using System.Collections.Generic;
using SHUU.Utils.Helpers;
using UnityEngine;

namespace SHUU.Utils.Developer.Debugging
{
    public class Debug_ColliderVisualizerProxy : MonoBehaviour
    {
        private KeyCode activationKey;



        [Header("Rendering")]
        private Material wireMaterial;
        private Material fillMaterial;

        private bool alwaysRenderWire;
        private bool alwaysRenderFill;

        private float updateCollidersInterval;
        private float updateCacheInterval;

        private float maxDistance;


        [Header("Colors")]
        private Color defaultWireColor;
        private Color defaultFillColor;

        [Range(0f, 1f)] private float triggerAlphaMultiplier;
        [Range(0f, 1f)] private float disabledAlphaMultiplier;


        [Header("Overrides")]
        private LayerMask excludedLayers;
        private List<string> excludedTags = new();

        private List<LayerWireColor> layerWireColors = new();
        private List<TagFillColor> tagFillColors = new();



        #region Internal types
        private class CachedBatch
        {
            public int mode;
            public Color color;
            public List<Vector3> verts = new();
        }

        readonly List<CachedBatch> batches = new();
        readonly List<Collider> cachedColliders = new();

        float lastRebuildTime;
        [HideInInspector] public bool initialized;


        private bool toggle = true;
        public bool Toggle()
        {
            toggle = !toggle;
            return toggle;
        }
        #endregion




        #region Setup
        private void OnEnable()
        {
            if (Debug_ColliderVisualizer.instance != null) Debug_ColliderVisualizer.instance.proxy = this;
            else Invoke(nameof(OnEnable), 0.01f);
        }

        private void OnDisable()
        {
            if (Debug_ColliderVisualizer.instance) Debug_ColliderVisualizer.instance.proxy = null;
        }


        public void Init(KeyCode activationKey, Shader matShader, bool alwaysRenderWire, bool alwaysRenderFill, float updateCollidersInterval, float updateCacheInterval, float maxDistance, Color defaultWireColor, Color defaultFillColor, float triggerAlphaMultiplier, float disabledAlphaMultiplier, LayerMask excludedLayers, List<string> excludedTags, List<LayerWireColor> layerWireColors, List<TagFillColor> tagFillColors, bool toggle)
        {
            if (initialized) return;
            initialized = true;


            this.activationKey = activationKey;
            this.alwaysRenderWire = alwaysRenderWire;
            this.alwaysRenderFill = alwaysRenderFill;
            this.updateCollidersInterval = updateCollidersInterval;
            this.updateCacheInterval = updateCacheInterval;
            this.maxDistance = maxDistance;
            this.defaultWireColor = defaultWireColor;
            this.defaultFillColor = defaultFillColor;
            this.triggerAlphaMultiplier = triggerAlphaMultiplier;
            this.disabledAlphaMultiplier = disabledAlphaMultiplier;
            this.excludedLayers = excludedLayers;
            this.excludedTags.CopyFrom_List(excludedTags);
            this.layerWireColors.CopyFrom_List(layerWireColors);
            this.tagFillColors.CopyFrom_List(tagFillColors);

            this.toggle = toggle;


            wireMaterial = new Material(matShader);
            wireMaterial.hideFlags = HideFlags.HideAndDontSave;
            wireMaterial.SetInt("_ZWrite", 0);
            wireMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

            fillMaterial = new Material(matShader);
            fillMaterial.hideFlags = HideFlags.HideAndDontSave;
            fillMaterial.SetInt("_ZWrite", 0);
            fillMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);


            CacheReload();
        }
        #endregion
        


        #region Loop & external
        private bool invoked_colliders = false;
        private bool invoked_rebuild = false;
        private void Update()
        {
            if (!initialized || activationKey == KeyCode.None) return;

            if (Input.GetKeyDown(activationKey)) Toggle();
        }

        void LateUpdate()
        {
            if (!initialized || !toggle) return;
            
            
            if (updateCollidersInterval > 0)
            {
                if (!invoked_colliders)
                {
                    invoked_colliders = true;
                    Invoke(nameof(Invoke_CacheColliders), updateCollidersInterval);
                } 
            }

            if (updateCacheInterval > 0)
            {
                if (!invoked_rebuild)
                {
                    invoked_rebuild = true;
                    Invoke(nameof(Invoke_RebuildCache), updateCacheInterval);
                } 
            }
        }
        private void Invoke_CacheColliders()
        {
            invoked_colliders = false;

            CacheColliders();
        }
        private void Invoke_RebuildCache()
        {
            invoked_rebuild = false;

            RebuildCache();
        }
        public void CacheReload()
        {
            CacheColliders();
            RebuildCache();
        }

        void OnRenderObject()
        {
            if (!initialized || !toggle || batches.Count == 0) return;

            // ---------- FILLS ----------
            fillMaterial.SetInt(
                "_ZTest",
                (int)(alwaysRenderFill
                    ? UnityEngine.Rendering.CompareFunction.Always
                    : UnityEngine.Rendering.CompareFunction.LessEqual));

            fillMaterial.SetPass(0);

            foreach (var b in batches)
            {
                if (b.mode != GL.TRIANGLES) continue;

                GL.Begin(GL.TRIANGLES);
                GL.Color(b.color);

                for (int i = 0; i < b.verts.Count; i++)
                    GL.Vertex(b.verts[i]);

                GL.End();
            }

            // ---------- WIRES ----------
            wireMaterial.SetInt(
                "_ZTest",
                (int)(alwaysRenderWire
                    ? UnityEngine.Rendering.CompareFunction.Always
                    : UnityEngine.Rendering.CompareFunction.LessEqual));

            wireMaterial.SetPass(0);

            foreach (var b in batches)
            {
                if (b.mode != GL.LINES) continue;

                GL.Begin(GL.LINES);
                GL.Color(b.color);

                for (int i = 0; i < b.verts.Count; i++)
                    GL.Vertex(b.verts[i]);

                GL.End();
            }
        }
        #endregion

        #region Cache building

        public void CacheColliders()
        {
            cachedColliders.Clear();
            
            #if UNITY_2023_1_OR_NEWER
            cachedColliders.AddRange(FindObjectsByType<Collider>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            #else
            cachedColliders.AddRange(FindObjectsOfType<Collider>(true));
            #endif

            cachedColliders.RemoveAll(x => excludedLayers.Contains_Layer(x.gameObject.layer) || excludedTags.Contains(x.gameObject.tag));
        }

        public void RebuildCache()
        {
            batches.Clear();

            Camera cam = Camera.main;
            if (!cam) return;

            float maxDistSqr = maxDistance * maxDistance;

            foreach (var col in cachedColliders)
            {
                if (!col) continue;

                if ((col.transform.position - cam.transform.position).sqrMagnitude > maxDistSqr)
                    continue;

                // ---- Fill ----
                Color fill = ApplyAlpha(col, GetFillColor(col.tag));
                if (fill.a > 0f)
                    BuildFill(col, fill);

                // ---- Wire ----
                Color wire = ApplyAlpha(col, GetWireColor(col.gameObject.layer));
                BuildWire(col, wire);
            }
        }

        CachedBatch NewBatch(int mode, Color color)
        {
            var b = new CachedBatch { mode = mode, color = color };
            batches.Add(b);
            return b;
        }

        #endregion

        #region Colors

        Color ApplyAlpha(Collider col, Color c)
        {
            float a = c.a;

            if (!col.enabled || !col.gameObject.activeInHierarchy)
                a *= disabledAlphaMultiplier;
            else if (col.isTrigger)
                a *= triggerAlphaMultiplier;

            return new Color(c.r, c.g, c.b, a);
        }

        Color GetWireColor(int layer)
        {
            foreach (var lw in layerWireColors)
                if ((lw.layers.value & (1 << layer)) != 0)
                    return lw.wireColor;
            return defaultWireColor;
        }

        Color GetFillColor(string tag)
        {
            foreach (var tf in tagFillColors)
                if (tf.tags.Contains(tag))
                    return tf.fillColor;
            return defaultFillColor;
        }

        #endregion

        #region Builders

        void BuildWire(Collider col, Color c)
        {
            var b = NewBatch(GL.LINES, c);

            if (col is BoxCollider box) DrawBox(box, b.verts);
            else if (col is SphereCollider s) DrawSphere(s, b.verts);
            else if (col is CapsuleCollider cap) DrawCapsule(cap, b.verts);
        }

        void BuildFill(Collider col, Color c)
        {
            var b = NewBatch(GL.TRIANGLES, c);

            if (col is BoxCollider box) FillBox(box, b.verts);
            else if (col is SphereCollider s) FillSphere(s, b.verts);
            else if (col is CapsuleCollider cap) FillCapsule(cap, b.verts);
        }

        #endregion

        #region Wire geometry

        void DrawBox(BoxCollider box, List<Vector3> v)
        {
            Transform t = box.transform;
            Matrix4x4 m = Matrix4x4.TRS(
                t.TransformPoint(box.center),
                t.rotation,
                Vector3.Scale(box.size, t.lossyScale));

            Vector3[] p =
            {
                m.MultiplyPoint3x4(new(-.5f,-.5f,-.5f)),
                m.MultiplyPoint3x4(new(.5f,-.5f,-.5f)),
                m.MultiplyPoint3x4(new(.5f,-.5f,.5f)),
                m.MultiplyPoint3x4(new(-.5f,-.5f,.5f)),
                m.MultiplyPoint3x4(new(-.5f,.5f,-.5f)),
                m.MultiplyPoint3x4(new(.5f,.5f,-.5f)),
                m.MultiplyPoint3x4(new(.5f,.5f,.5f)),
                m.MultiplyPoint3x4(new(-.5f,.5f,.5f)),
            };

            int[] e =
            {
                0,1,1,2,2,3,3,0,
                4,5,5,6,6,7,7,4,
                0,4,1,5,2,6,3,7
            };

            for (int i = 0; i < e.Length; i++)
                v.Add(p[e[i]]);
        }

        void DrawSphere(SphereCollider s, List<Vector3> v, int seg = 16)
        {
            Transform t = s.transform;
            Vector3 c = t.TransformPoint(s.center);
            float r = s.radius * Mathf.Max(t.lossyScale.x, t.lossyScale.y, t.lossyScale.z);

            DrawCircle(c, t.right, t.up, r, seg, v);
            DrawCircle(c, t.up, t.forward, r, seg, v);
            DrawCircle(c, t.forward, t.right, r, seg, v);
        }

        void DrawCapsule(CapsuleCollider c, List<Vector3> v, int seg = 12, int lat = 8)
        {
            Transform t = c.transform;
            Vector3 center = t.TransformPoint(c.center);

            Vector3 axis =
                c.direction == 0 ? t.right :
                c.direction == 1 ? t.up :
                t.forward;

            Vector3 orthoA =
                Mathf.Abs(Vector3.Dot(axis, t.up)) < 0.99f
                    ? Vector3.Cross(axis, t.up).normalized
                    : Vector3.Cross(axis, t.right).normalized;

            Vector3 orthoB = Vector3.Cross(axis, orthoA).normalized;

            float scale = Mathf.Max(t.lossyScale.x, t.lossyScale.y, t.lossyScale.z);
            float r = c.radius * scale;
            float h = Mathf.Max(0, c.height * scale - 2 * r);

            Vector3 top = center + axis * h * 0.5f;
            Vector3 bottom = center - axis * h * 0.5f;

            for (int i = 0; i < seg; i++)
            {
                float a0 = i * Mathf.PI * 2f / seg;
                float a1 = (i + 1) * Mathf.PI * 2f / seg;

                Vector3 o0 = Mathf.Cos(a0) * orthoA * r + Mathf.Sin(a0) * orthoB * r;
                Vector3 o1 = Mathf.Cos(a1) * orthoA * r + Mathf.Sin(a1) * orthoB * r;

                v.Add(top + o0); v.Add(bottom + o0);
                v.Add(top + o0); v.Add(top + o1);
                v.Add(bottom + o0); v.Add(bottom + o1);
            }

            AddHemisphereWire(top, axis, orthoA, orthoB, r, true, lat, seg, v);
            AddHemisphereWire(bottom, axis, orthoA, orthoB, r, false, lat, seg, v);
        }

        void AddHemisphereWire(
            Vector3 center,
            Vector3 axis,
            Vector3 orthoA,
            Vector3 orthoB,
            float r,
            bool top,
            int lat,
            int seg,
            List<Vector3> v)
        {
            float dir = top ? 1f : -1f;

            for (int y = 0; y < lat; y++)
            {
                float a0 = y / (float)lat * Mathf.PI * 0.5f;
                float a1 = (y + 1) / (float)lat * Mathf.PI * 0.5f;

                float s0 = Mathf.Sin(a0);
                float s1 = Mathf.Sin(a1);
                float c0 = Mathf.Cos(a0);
                float c1 = Mathf.Cos(a1);

                for (int i = 0; i < seg; i++)
                {
                    float u0 = i * Mathf.PI * 2f / seg;
                    float u1 = (i + 1) * Mathf.PI * 2f / seg;

                    Vector3 p00 = center + axis * (c0 * r * dir) +
                                  (Mathf.Cos(u0) * orthoA + Mathf.Sin(u0) * orthoB) * s0 * r;
                    Vector3 p01 = center + axis * (c0 * r * dir) +
                                  (Mathf.Cos(u1) * orthoA + Mathf.Sin(u1) * orthoB) * s0 * r;
                    Vector3 p10 = center + axis * (c1 * r * dir) +
                                  (Mathf.Cos(u0) * orthoA + Mathf.Sin(u0) * orthoB) * s1 * r;

                    v.Add(p00); v.Add(p01);
                    v.Add(p00); v.Add(p10);
                }
            }
        }

        void DrawCircle(Vector3 c, Vector3 a, Vector3 b, float r, int seg, List<Vector3> v)
        {
            for (int i = 0; i < seg; i++)
            {
                float t1 = i * Mathf.PI * 2f / seg;
                float t2 = (i + 1) * Mathf.PI * 2f / seg;

                v.Add(c + (Mathf.Cos(t1) * a + Mathf.Sin(t1) * b) * r);
                v.Add(c + (Mathf.Cos(t2) * a + Mathf.Sin(t2) * b) * r);
            }
        }

        #endregion

        #region Fill geometry

        void FillBox(BoxCollider box, List<Vector3> v)
        {
            Transform t = box.transform;
            Matrix4x4 m = Matrix4x4.TRS(
                t.TransformPoint(box.center),
                t.rotation,
                Vector3.Scale(box.size, t.lossyScale));

            Vector3[] p =
            {
                m.MultiplyPoint3x4(new(-.5f,-.5f,-.5f)),
                m.MultiplyPoint3x4(new(.5f,-.5f,-.5f)),
                m.MultiplyPoint3x4(new(.5f,-.5f,.5f)),
                m.MultiplyPoint3x4(new(-.5f,-.5f,.5f)),
                m.MultiplyPoint3x4(new(-.5f,.5f,-.5f)),
                m.MultiplyPoint3x4(new(.5f,.5f,-.5f)),
                m.MultiplyPoint3x4(new(.5f,.5f,.5f)),
                m.MultiplyPoint3x4(new(-.5f,.5f,.5f)),
            };

            int[] tIdx =
            {
                0,1,2, 0,2,3,
                4,6,5, 4,7,6,
                0,4,5, 0,5,1,
                1,5,6, 1,6,2,
                2,6,7, 2,7,3,
                3,7,4, 3,4,0
            };

            for (int i = 0; i < tIdx.Length; i++)
                v.Add(p[tIdx[i]]);
        }

        void FillSphere(SphereCollider s, List<Vector3> v, int lat = 10, int lon = 16)
        {
            Transform t = s.transform;
            Vector3 c = t.TransformPoint(s.center);
            float r = s.radius * Mathf.Max(t.lossyScale.x, t.lossyScale.y, t.lossyScale.z);

            for (int y = 0; y < lat; y++)
            {
                float v0 = y / (float)lat;
                float v1 = (y + 1) / (float)lat;

                float phi0 = Mathf.PI * (v0 - 0.5f);
                float phi1 = Mathf.PI * (v1 - 0.5f);

                float y0 = Mathf.Sin(phi0);
                float y1 = Mathf.Sin(phi1);

                float r0 = Mathf.Cos(phi0);
                float r1 = Mathf.Cos(phi1);

                for (int x = 0; x < lon; x++)
                {
                    float u0 = x / (float)lon * Mathf.PI * 2f;
                    float u1 = (x + 1) / (float)lon * Mathf.PI * 2f;

                    Vector3 p00 = c + (t.right * Mathf.Cos(u0) * r0 + t.up * y0 + t.forward * Mathf.Sin(u0) * r0) * r;
                    Vector3 p01 = c + (t.right * Mathf.Cos(u1) * r0 + t.up * y0 + t.forward * Mathf.Sin(u1) * r0) * r;
                    Vector3 p10 = c + (t.right * Mathf.Cos(u0) * r1 + t.up * y1 + t.forward * Mathf.Sin(u0) * r1) * r;
                    Vector3 p11 = c + (t.right * Mathf.Cos(u1) * r1 + t.up * y1 + t.forward * Mathf.Sin(u1) * r1) * r;

                    v.Add(p00); v.Add(p10); v.Add(p11);
                    v.Add(p00); v.Add(p11); v.Add(p01);
                }
            }
        }

        void FillCapsule(CapsuleCollider c, List<Vector3> v, int seg = 12, int lat = 8)
        {
            Transform t = c.transform;
            Vector3 center = t.TransformPoint(c.center);

            Vector3 axis =
                c.direction == 0 ? t.right :
                c.direction == 1 ? t.up :
                t.forward;

            Vector3 orthoA =
                Mathf.Abs(Vector3.Dot(axis, t.up)) < 0.99f
                    ? Vector3.Cross(axis, t.up).normalized
                    : Vector3.Cross(axis, t.right).normalized;

            Vector3 orthoB = Vector3.Cross(axis, orthoA).normalized;

            float scale = Mathf.Max(t.lossyScale.x, t.lossyScale.y, t.lossyScale.z);
            float r = c.radius * scale;
            float h = Mathf.Max(0, c.height * scale - 2 * r);

            Vector3 top = center + axis * h * 0.5f;
            Vector3 bottom = center - axis * h * 0.5f;

            for (int i = 0; i < seg; i++)
            {
                float a0 = i * Mathf.PI * 2f / seg;
                float a1 = (i + 1) * Mathf.PI * 2f / seg;

                Vector3 o0 = Mathf.Cos(a0) * orthoA * r + Mathf.Sin(a0) * orthoB * r;
                Vector3 o1 = Mathf.Cos(a1) * orthoA * r + Mathf.Sin(a1) * orthoB * r;

                v.Add(bottom + o0); v.Add(top + o0); v.Add(top + o1);
                v.Add(bottom + o0); v.Add(top + o1); v.Add(bottom + o1);
            }

            FillHemisphere(top, axis, orthoA, orthoB, r, true, lat, seg, v);
            FillHemisphere(bottom, axis, orthoA, orthoB, r, false, lat, seg, v);
        }

        void FillHemisphere(
            Vector3 center,
            Vector3 axis,
            Vector3 orthoA,
            Vector3 orthoB,
            float r,
            bool top,
            int lat,
            int seg,
            List<Vector3> v)
        {
            float dir = top ? 1f : -1f;

            for (int y = 0; y < lat; y++)
            {
                float a0 = y / (float)lat * Mathf.PI * 0.5f;
                float a1 = (y + 1) / (float)lat * Mathf.PI * 0.5f;

                float s0 = Mathf.Sin(a0);
                float s1 = Mathf.Sin(a1);
                float c0 = Mathf.Cos(a0);
                float c1 = Mathf.Cos(a1);

                for (int i = 0; i < seg; i++)
                {
                    float u0 = i * Mathf.PI * 2f / seg;
                    float u1 = (i + 1) * Mathf.PI * 2f / seg;

                    Vector3 p00 = center + axis * (c0 * r * dir) +
                                  (Mathf.Cos(u0) * orthoA + Mathf.Sin(u0) * orthoB) * s0 * r;
                    Vector3 p01 = center + axis * (c0 * r * dir) +
                                  (Mathf.Cos(u1) * orthoA + Mathf.Sin(u1) * orthoB) * s0 * r;
                    Vector3 p10 = center + axis * (c1 * r * dir) +
                                  (Mathf.Cos(u0) * orthoA + Mathf.Sin(u0) * orthoB) * s1 * r;
                    Vector3 p11 = center + axis * (c1 * r * dir) +
                                  (Mathf.Cos(u1) * orthoA + Mathf.Sin(u1) * orthoB) * s1 * r;

                    v.Add(p00); v.Add(p10); v.Add(p11);
                    v.Add(p00); v.Add(p11); v.Add(p01);
                }
            }
        }

        #endregion
    }
}

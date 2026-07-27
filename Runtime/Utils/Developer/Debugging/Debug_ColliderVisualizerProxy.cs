/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

using SHUU.Utils.Globals;
using SHUU.Utils.Helpers;

using static SHUU.Utils.Helpers.HandyFunctions;
using System;
using SHUU.Utils.InputSystem;

namespace SHUU.Utils.Developer.Debugging
{
    public class Debug_ColliderVisualizerProxy : MonoBehaviour
    {
        #region Variables
        private Debug_ColliderVisualizer source;



        private Material wireMat_LessEqual;
        private Material wireMat_Always;
        private Material fillMat_LessEqual;
        private Material fillMat_Always;

        private bool alwaysRenderWire;
        private bool alwaysRenderFill;
        public bool Toggle_WireRender(bool? toggle = null) => toggle == null ? alwaysRenderWire = !alwaysRenderWire : alwaysRenderWire = toggle.Value;
        public bool Toggle_FillRender(bool? toggle = null) => toggle == null ? alwaysRenderFill = !alwaysRenderFill : alwaysRenderFill = toggle.Value;


        private SHUU_Timer chacheColliders_timer = null;
        private SHUU_Timer rebuildCache_timer    = null;


        private Camera cam;
        private Transform camTransform => cam?.transform;



        #region Rendering
        private struct DrawGroup { public Mesh mesh; }

        private DrawGroup[] wireGroups = Array.Empty<DrawGroup>();
        private DrawGroup[] fillGroups = Array.Empty<DrawGroup>();


        // Scratch buffers
        private readonly Dictionary<Color, List<Vector3>> wireScratch    = new();
        private readonly Dictionary<Color, List<Color>>   wireColScratch = new();
        private readonly Dictionary<Color, List<int>>     wireIdxScratch = new();
        private readonly Dictionary<Color, List<Vector3>> fillScratch    = new();
        private readonly Dictionary<Color, List<Color>>   fillColScratch = new();
        private readonly Dictionary<Color, List<int>>     fillIdxScratch = new();

        private readonly List<Vector3> tempVerts = new(512);
        private readonly List<int>     tempIdx   = new(1024);


        private readonly List<Collider> cachedColliders = new();
        #endregion


        
        [HideInInspector] public bool initialized;


        private bool _toggle = false;
        private bool toggle
        {
            get => _toggle;
            set
            {
                if (value)
                {
                    CacheColliders();
                    RebuildCache();
                }
                else
                {
                    chacheColliders_timer?.Cancel();
                    rebuildCache_timer?.Cancel();
                }
                _toggle = value;
            }
        }
        public bool Toggle(bool? t = null) => t == null ? toggle = !toggle : toggle = t.Value;
        #endregion




        #region Main
        public void Init(Debug_ColliderVisualizer source, bool toggle)
        {
            if (initialized) return;
            initialized = true;

            this.source = source;
            this.toggle = toggle;

            alwaysRenderWire = source.alwaysRenderWire;
            alwaysRenderFill = source.alwaysRenderFill;

            wireMat_LessEqual = MakeMaterial(source.matShader, CompareFunction.LessEqual);
            wireMat_Always    = MakeMaterial(source.matShader, CompareFunction.Always);
            fillMat_LessEqual = MakeMaterial(source.matShader, CompareFunction.LessEqual);
            fillMat_Always    = MakeMaterial(source.matShader, CompareFunction.Always);

            cam = Camera.main;

            CacheReload();
        }

        private static Material MakeMaterial(Shader shader, CompareFunction zTest)
        {
            var mat = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            mat.SetInt("_ZWrite", 0);
            mat.SetInt("_Cull",   (int)CullMode.Off);
            mat.SetInt("_ZTest",  (int)zTest);

            if (zTest == CompareFunction.Always) mat.renderQueue = 4500;

            return mat;
        }


        public void OnColliderAdded(Debug_ColliderTracker tracker)
        {
            foreach (var col in tracker.GetComponents<Collider>())
                if (!cachedColliders.Contains(col) && !source.excludedLayers.Contains(col.gameObject.layer) && !source.excludedTags.Contains(col.gameObject.tag))
                    cachedColliders.Add(col);
            
            RebuildCache();
        }

        public void OnColliderRemoved(Debug_ColliderTracker tracker)
        {
            cachedColliders.RemoveAll(c => c != null && c.gameObject == tracker.gameObject);

            RebuildCache();
        }


        private void OnEnable()
        {
            if (Debug_ColliderVisualizer.instance != null) Debug_ColliderVisualizer.instance.proxy = this;
            else SHUU_Time.Timer(0.01f, OnEnable);
        }

        private void OnDisable()
        {
            if (Debug_ColliderVisualizer.instance) Debug_ColliderVisualizer.instance.proxy = null;
            DestroyGroups(ref wireGroups);
            DestroyGroups(ref fillGroups);
        }


        private void Update()
        {
            if (initialized)
            {
                if (!string.IsNullOrEmpty(source.activationActionPath) && SHUU_Input.GetInputDown(source.activationActionPath)) Toggle();
                else if (source.activationKey != KeyCode.None && Input.GetKeyDown(source.activationKey)) Toggle();
            }

            if (!initialized || !toggle) return;
            if (wireGroups.Length == 0 && fillGroups.Length == 0) return;

            Material wireMat = alwaysRenderWire ? wireMat_Always : wireMat_LessEqual;
            Material fillMat = alwaysRenderFill ? fillMat_Always : fillMat_LessEqual;

            for (int i = 0; i < fillGroups.Length; i++)
            {
                Mesh m = fillGroups[i].mesh;
                if (m != null) Graphics.DrawMesh(m, Matrix4x4.identity, fillMat, 0);
            }

            for (int i = 0; i < wireGroups.Length; i++)
            {
                Mesh m = wireGroups[i].mesh;
                if (m != null) Graphics.DrawMesh(m, Matrix4x4.identity, wireMat, 0);
            }
        }
        #endregion



        #region Logic

        #region Timers / public cache API
        public void CacheReload()
        {
            CacheColliders();
            RebuildCache();
        }

        public void CacheColliders()
        {
            if (chacheColliders_timer != null) chacheColliders_timer.Cancel();
            chacheColliders_timer = SHUU_Time.Timer(source.updateCollidersInterval, CacheColliders);

            _CacheColliders();
        }

        public void RebuildCache()
        {
            if (rebuildCache_timer != null) rebuildCache_timer.Cancel();
            rebuildCache_timer = SHUU_Time.Timer(source.rebuildCacheInterval, RebuildCache);

            _RebuildCache();
        }
        #endregion



        #region Cache
        private void _CacheColliders()
        {
            cachedColliders.Clear();

#if UNITY_2023_1_OR_NEWER
            var all = FindObjectsByType<Collider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            var all = FindObjectsOfType<Collider>(true);
#endif
            foreach (var col in all)
            {
                if (!source.excludedLayers.Contains(col.gameObject.layer) && !source.excludedTags.Contains(col.gameObject.tag)) cachedColliders.Add(col);
            }

            foreach (var col in cachedColliders)
                if (!col.gameObject.GetComponent<Debug_ColliderTracker>()) col.gameObject.AddComponent<Debug_ColliderTracker>();
        }

        private void _RebuildCache()
        {
            if (!cam) return;

            foreach (var kv in wireScratch)
                kv.Value.Clear();
            foreach (var kv in wireColScratch)
                kv.Value.Clear();
            foreach (var kv in wireIdxScratch)
                kv.Value.Clear();
            foreach (var kv in fillScratch)
                kv.Value.Clear();
            foreach (var kv in fillColScratch)
                kv.Value.Clear();
            foreach (var kv in fillIdxScratch)
                kv.Value.Clear();

            Vector3 camPos = camTransform.position;
            float maxDistSqr = source.maxDistance * source.maxDistance;

            foreach (var col in cachedColliders)
            {
                if (!col) continue;

                if (maxDistSqr > 0 && (col.transform.position - camPos).sqrMagnitude > maxDistSqr) continue;

                int layer = col.gameObject.layer;
                string tag = col.tag;

                // Fill
                Color fillColor = ApplyAlpha(col, GetFillColor(layer, tag));
                if (fillColor.a > 0f)
                {
                    tempVerts.Clear(); tempIdx.Clear();
                    BuildFill(col, tempVerts, tempIdx);
                    if (tempVerts.Count > 0) AppendToScratch(fillColor, tempVerts, tempIdx, fillScratch, fillColScratch, fillIdxScratch);
                }

                // Wire
                Color wireColor = ApplyAlpha(col, GetWireColor(layer, tag));
                if (wireColor.a > 0f)
                {
                    tempVerts.Clear(); tempIdx.Clear();
                    BuildWire(col, tempVerts, tempIdx);
                    if (tempVerts.Count > 0) AppendToScratch(wireColor, tempVerts, tempIdx, wireScratch, wireColScratch, wireIdxScratch);
                }
            }

            BakeIntoGroups(ref wireGroups, wireScratch, wireColScratch, wireIdxScratch, MeshTopology.Lines);
            BakeIntoGroups(ref fillGroups, fillScratch, fillColScratch, fillIdxScratch, MeshTopology.Triangles);
        }


        private static void AppendToScratch(Color color, List<Vector3> verts, List<int> indices, Dictionary<Color, List<Vector3>> vScratch, Dictionary<Color, List<Color>> cScratch, Dictionary<Color, List<int>> iScratch)
        {
            if (!vScratch.TryGetValue(color, out var vList)) vList = new List<Vector3>(verts.Count); vScratch[color] = vList;

            if (!cScratch.TryGetValue(color, out var cList)) cList = new List<Color>(verts.Count); cScratch[color] = cList;

            if (!iScratch.TryGetValue(color, out var iList)) iList = new List<int>(indices.Count); iScratch[color] = iList;

            int offset = vList.Count;
            vList.AddRange(verts);
            for (int i = 0; i < verts.Count; i++) cList.Add(color);
            for (int i = 0; i < indices.Count; i++) iList.Add(indices[i] + offset);
        }


        private static void BakeIntoGroups(ref DrawGroup[] groups, Dictionary<Color, List<Vector3>> vScratch, Dictionary<Color, List<Color>> cScratch, Dictionary<Color, List<int>> iScratch, MeshTopology topology)
        {
            int needed = 0;
            foreach (var kv in vScratch)
                if (kv.Value.Count > 0) needed++;

            if (groups.Length > needed)
            {
                for (int i = needed; i < groups.Length; i++)
                    if (groups[i].mesh != null) Destroy(groups[i].mesh);

                Array.Resize(ref groups, needed);
            }

            if (groups.Length < needed)
            {
                int old = groups.Length;
                Array.Resize(ref groups, needed);
                for (int i = old; i < needed; i++)
                    groups[i].mesh = new Mesh { hideFlags = HideFlags.HideAndDontSave };
            }

            int gi = 0;
            foreach (var kv in vScratch)
            {
                if (kv.Value.Count == 0) continue;

                Color color = kv.Key;
                Mesh  mesh  = groups[gi].mesh;

                mesh.Clear();
                mesh.SetVertices(kv.Value);
                mesh.SetColors(cScratch[color]);
                mesh.SetIndices(iScratch[color], topology, 0);

                mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1e9f);

                gi++;
            }
        }


        private static void DestroyGroups(ref DrawGroup[] groups)
        {
            for (int i = 0; i < groups.Length; i++)
                if (groups[i].mesh != null) Destroy(groups[i].mesh);

            groups = Array.Empty<DrawGroup>();
        }
        #endregion



        #region Colors
        Color ApplyAlpha(Collider col, Color c)
        {
            float a = c.a;
            if (!col.enabled || !col.gameObject.activeInHierarchy) a *= source.disabledAlphaMultiplier;
            else if (col.isTrigger) a *= source.triggerAlphaMultiplier;
            return new Color(c.r, c.g, c.b, a);
        }

        Color GetWireColor(int layer, string tag)
        {
            CustomColors best = null;
            int score = -1;

            foreach (var custom in source.customColors)
            {
                if (!custom.overrideWireColor) continue;
                if (!Matches(custom, layer, tag)) continue;

                bool lm = custom.layerMask.Contains(layer);
                bool tm = custom.tagMask.Contains(tag);
                int s = (lm ? 1 : 0) + (tm ? 1 : 0);
                if (custom.useAndMatching && lm && tm) s += 10;

                if (s > score) score = s; best = custom;
            }

            return best != null ? best.wireColor : source.defaultWireColor;
        }

        Color GetFillColor(int layer, string tag)
        {
            CustomColors best = null;
            int score = -1;

            foreach (var custom in source.customColors)
            {
                if (!custom.overrideFillColor) continue;
                if (!Matches(custom, layer, tag)) continue;

                bool lm = custom.layerMask.Contains(layer);
                bool tm = custom.tagMask.Contains(tag);
                int s = (lm ? 1 : 0) + (tm ? 1 : 0);
                if (custom.useAndMatching && lm && tm) s += 10;

                if (s > score) score = s; best = custom;
            }

            return best != null ? best.fillColor : source.defaultFillColor;
        }

        bool Matches(CustomColors custom, int layer, string tag)
        {
            bool lm = custom.layerMask.Contains(layer);
            bool tm = custom.tagMask.Contains(tag);
            return custom.useAndMatching ? lm && tm : lm || tm;
        }
        #endregion



        #region Geometry

        #region Wire geometry
        void BuildWire(Collider col, List<Vector3> v, List<int> idx)
        {
            if (col is BoxCollider box) DrawBox(box, v, idx);
            else if (col is SphereCollider s) DrawSphere(s, v, idx);
            else if (col is CapsuleCollider cap) DrawCapsule(cap, v, idx);
            else if (col is MeshCollider mc) DrawMeshCollider(mc, v, idx);
        }

        void DrawBox(BoxCollider box, List<Vector3> v, List<int> idx)
        {
            Transform t = box.transform;
            Matrix4x4 m = Matrix4x4.TRS(t.TransformPoint(box.center), t.rotation, Vector3.Scale(box.size, t.lossyScale));

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

        void DrawSphere(SphereCollider s, List<Vector3> v, List<int> idx, int seg = 16)
        {
            Transform t = s.transform;
            Vector3 c = t.TransformPoint(s.center);
            float r = s.radius * Mathf.Max(t.lossyScale.x, t.lossyScale.y, t.lossyScale.z);

            DrawCircle(c, t.right, t.up, r, seg, v, idx);
            DrawCircle(c, t.up, t.forward, r, seg, v, idx);
            DrawCircle(c, t.forward, t.right, r, seg, v, idx);
        }

        void DrawCapsule(CapsuleCollider c, List<Vector3> v, List<int> idx, int seg = 12, int lat = 8)
        {
            Transform t = c.transform;
            Vector3 center = t.TransformPoint(c.center);

            Vector3 axis = c.direction == 0 ? t.right : c.direction == 1 ? t.up : t.forward;

            Vector3 orthoA = Mathf.Abs(Vector3.Dot(axis, t.up)) < 0.99f ? Vector3.Cross(axis, t.up).normalized : Vector3.Cross(axis, t.right).normalized;
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

        void DrawMeshCollider(MeshCollider mc, List<Vector3> v, List<int> idx)
        {
            if (!mc.sharedMesh) return;

            Mesh mesh = mc.sharedMesh;
            Transform t = mc.transform;
            Vector3[] verts = mesh.vertices;
            int[] tris = mesh.triangles;

            for (int i = 0; i < tris.Length; i += 3)
            {
                int b = v.Count;
                v.Add(t.TransformPoint(verts[tris[i]]));
                v.Add(t.TransformPoint(verts[tris[i + 1]]));
                v.Add(t.TransformPoint(verts[tris[i + 2]]));
                idx.Add(b);
                idx.Add(b + 1);
                idx.Add(b + 1);
                idx.Add(b + 2);
                idx.Add(b + 2);
                idx.Add(b);
            }
        }

        void AddHemisphereWire( Vector3 center, Vector3 axis, Vector3 orthoA, Vector3 orthoB, float r, bool top, int lat, int seg, List<Vector3> v, List<int> idx)
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

        void DrawCircle(Vector3 c, Vector3 a, Vector3 b, float r, int seg, List<Vector3> v, List<int> idx)
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
        void BuildFill(Collider col, List<Vector3> v, List<int> idx)
        {
            if (col is BoxCollider box) FillBox(box, v, idx);
            else if (col is SphereCollider s) FillSphere(s, v, idx);
            else if (col is CapsuleCollider cap) FillCapsule(cap, v, idx);
            else if (col is MeshCollider mc) FillMeshCollider(mc, v, idx);
        }

        void FillBox(BoxCollider box, List<Vector3> v, List<int> idx)
        {
            Transform t = box.transform;
            Matrix4x4 m = Matrix4x4.TRS(t.TransformPoint(box.center), t.rotation, Vector3.Scale(box.size, t.lossyScale));

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

        void FillSphere(SphereCollider s, List<Vector3> v, List<int> idx, int lat = 10, int lon = 16)
        {
            Transform t = s.transform;
            Vector3 c = t.TransformPoint(s.center);
            float r = s.radius * Mathf.Max(t.lossyScale.x, t.lossyScale.y, t.lossyScale.z);

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

        void FillCapsule(CapsuleCollider c, List<Vector3> v, List<int> idx, int seg = 12, int lat = 8)
        {
            Transform t = c.transform;
            Vector3 center = t.TransformPoint(c.center);

            Vector3 axis = c.direction == 0 ? t.right : c.direction == 1 ? t.up : t.forward;

            Vector3 orthoA = Mathf.Abs(Vector3.Dot(axis, t.up)) < 0.99f ? Vector3.Cross(axis, t.up).normalized : Vector3.Cross(axis, t.right).normalized;
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

        void FillMeshCollider(MeshCollider mc, List<Vector3> v, List<int> idx)
        {
            if (!mc.sharedMesh) return;

            Mesh mesh = mc.sharedMesh;
            Transform t = mc.transform;
            Vector3[] verts = mesh.vertices;
            int[] tris = mesh.triangles;

            int b = v.Count;
            for (int i = 0; i < verts.Length; i++)
                v.Add(t.TransformPoint(verts[i]));
            for (int i = 0; i < tris.Length; i++)
                idx.Add(b + tris[i]);
        }

        void FillHemisphere(Vector3 center, Vector3 axis, Vector3 orthoA, Vector3 orthoB, float r, bool top, int lat, int seg, List<Vector3> v, List<int> idx)
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

        #endregion

        #endregion
    }
}

/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

using SHUU.Utils.Helpers;
using static SHUU.Utils.Helpers.HandyFunctions;

using Object = UnityEngine.Object;

namespace SHUU.Utils.Developer.Debugging.Systems
{
    internal class ColliderVisualizer_Cache
    {
        #region Variables
        public Camera camera;



        // Materials
        private Shader materialsShader;

        private Material wireMat_LessEqual;
        private Material wireMat_Always;
        private Material fillMat_LessEqual;
        private Material fillMat_Always;



        // Colliders
        private readonly HashSet<Collider> colliders = new();



        // Meshes
        private Mesh[] wireMeshes = Array.Empty<Mesh>();
        private Mesh[] fillMeshes = Array.Empty<Mesh>();


        private class Scratch
        {
            public readonly Dictionary<Color, List<Vector3>> verts   = new();
            public readonly Dictionary<Color, List<Color>>   colors  = new();
            public readonly Dictionary<Color, List<int>>     indices = new();

            public void Clear()
            {
                foreach (var list in verts.Values) list.Clear();
                foreach (var list in colors.Values) list.Clear();
                foreach (var list in indices.Values) list.Clear();
            }
        }

        private readonly Scratch wireScratch = new();
        private readonly Scratch fillScratch = new();

        private readonly List<Vector3> tempVerts = new(512);
        private readonly List<int>     tempIdx   = new(1024);
        #endregion




        #region Logic

        #region Materials
        public void EnsureMaterials(Shader shader)
        {
            if (shader == null || (shader == materialsShader && wireMat_LessEqual != null)) return;

            DestroyMaterials();

            materialsShader = shader;

            wireMat_LessEqual = MakeMaterial(shader, CompareFunction.LessEqual);
            wireMat_Always = MakeMaterial(shader, CompareFunction.Always);
            fillMat_LessEqual = MakeMaterial(shader, CompareFunction.LessEqual);
            fillMat_Always = MakeMaterial(shader, CompareFunction.Always);
        }

        private static Material MakeMaterial(Shader shader, CompareFunction zTest)
        {
            var mat = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            mat.SetInt("_ZWrite", 0);
            mat.SetInt("_Cull", (int)CullMode.Off);
            mat.SetInt("_ZTest", (int)zTest);

            if (zTest == CompareFunction.Always) mat.renderQueue = 4500;

            return mat;
        }

        private void DestroyMaterials()
        {
            DestroySafe(wireMat_LessEqual);
            DestroySafe(wireMat_Always);
            DestroySafe(fillMat_LessEqual);
            DestroySafe(fillMat_Always);

            wireMat_LessEqual = wireMat_Always = fillMat_LessEqual = fillMat_Always = null;
            materialsShader = null;
        }
        #endregion



        #region Colliders
        public void CacheColliders(SHUU_Debug settings)
        {
            colliders.Clear();

#if UNITY_2023_1_OR_NEWER
            var all = Object.FindObjectsByType<Collider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            var all = Object.FindObjectsOfType<Collider>(true);
#endif
            foreach (var col in all)
                if (Passes(col, settings)) colliders.Add(col);
        }

        private static bool Passes(Collider col, SHUU_Debug settings) => !settings.colliderVisualizer_excludedLayers.Contains(col.gameObject.layer) && !settings.colliderVisualizer_excludedTags.Contains(col.gameObject.tag);
        #endregion



        #region Rebuild
        public void Rebuild(SHUU_Debug settings)
        {
            colliders.RemoveWhere(c => c == null);

            wireScratch.Clear();
            fillScratch.Clear();


            float maxDistance = settings.colliderVisualizer_maxDistance;
            float maxDistSqr = maxDistance * maxDistance;

            if (maxDistance > 0f && camera == null) camera = Camera.main;
            if (maxDistance > 0f && camera == null) return;

            Vector3 camPos = maxDistance > 0f ? camera.transform.position : Vector3.zero;


            foreach (var col in colliders)
            {
                if (maxDistance > 0f && (col.transform.position - camPos).sqrMagnitude > maxDistSqr) continue;

                int layer = col.gameObject.layer;
                string tag = col.tag;

                // Fill
                if (TryGetColor(settings, layer, tag, false, out Color rawFillColor))
                {
                    Color fillColor = ApplyAlpha(col, rawFillColor, settings);
                    if (fillColor.a > 0f)
                    {
                        tempVerts.Clear(); tempIdx.Clear();
                        ColliderVisualizer_Geometry.BuildFill(col, tempVerts, tempIdx);
                        if (tempVerts.Count > 0) AppendToScratch(fillColor, tempVerts, tempIdx, fillScratch);
                    }
                }

                // Wire
                if (TryGetColor(settings, layer, tag, true, out Color rawWireColor))
                {
                    Color wireColor = ApplyAlpha(col, rawWireColor, settings);
                    if (wireColor.a > 0f)
                    {
                        tempVerts.Clear(); tempIdx.Clear();
                        ColliderVisualizer_Geometry.BuildWire(col, tempVerts, tempIdx);
                        if (tempVerts.Count > 0) AppendToScratch(wireColor, tempVerts, tempIdx, wireScratch);
                    }
                }
            }

            Bake(ref wireMeshes, wireScratch, MeshTopology.Lines);
            Bake(ref fillMeshes, fillScratch, MeshTopology.Triangles);
        }


        private static void AppendToScratch(Color color, List<Vector3> verts, List<int> indices, Scratch scratch)
        {
            if (!scratch.verts.TryGetValue(color, out var vList)) scratch.verts[color] = vList = new List<Vector3>(verts.Count);
            if (!scratch.colors.TryGetValue(color, out var cList)) scratch.colors[color] = cList = new List<Color>(verts.Count);
            if (!scratch.indices.TryGetValue(color, out var iList)) scratch.indices[color] = iList = new List<int>(indices.Count);

            int offset = vList.Count;
            vList.AddRange(verts);
            for (int i = 0; i < verts.Count; i++) cList.Add(color);
            for (int i = 0; i < indices.Count; i++) iList.Add(indices[i] + offset);
        }


        private static void Bake(ref Mesh[] meshes, Scratch scratch, MeshTopology topology)
        {
            int needed = 0;
            foreach (var list in scratch.verts.Values)
                if (list.Count > 0) needed++;

            if (meshes.Length > needed)
            {
                for (int i = needed; i < meshes.Length; i++) DestroySafe(meshes[i]);

                Array.Resize(ref meshes, needed);
            }
            else if (meshes.Length < needed)
            {
                int old = meshes.Length;
                Array.Resize(ref meshes, needed);

                for (int i = old; i < needed; i++) meshes[i] = new Mesh { hideFlags = HideFlags.HideAndDontSave };
            }


            int mi = 0;
            foreach (var kv in scratch.verts)
            {
                if (kv.Value.Count == 0) continue;

                Color color = kv.Key;
                Mesh mesh = meshes[mi++];

                mesh.Clear();
                mesh.indexFormat = kv.Value.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
                mesh.SetVertices(kv.Value);
                mesh.SetColors(scratch.colors[color]);
                mesh.SetIndices(scratch.indices[color], topology, 0);

                mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1e9f);
            }
        }
        #endregion



        #region Drawing
        public void Draw(bool wireOnTop, bool fillOnTop)
        {
            Material fillMat = fillOnTop ? fillMat_Always : fillMat_LessEqual;
            Material wireMat = wireOnTop ? wireMat_Always : wireMat_LessEqual;

            if (fillMat != null)
            {
                foreach (Mesh m in fillMeshes)
                    if (m != null) DrawOverlay(m, fillMat);
            }

            if (wireMat != null)
            {
                foreach (Mesh m in wireMeshes)
                    if (m != null) DrawOverlay(m, wireMat);
            }
        }

        private static void DrawOverlay(Mesh mesh, Material material)
            => Graphics.DrawMesh(mesh, Matrix4x4.identity, material, 0, null, 0, null, ShadowCastingMode.Off, false);
        #endregion



        #region Colors
        private static Color ApplyAlpha(Collider col, Color c, SHUU_Debug settings)
        {
            float a = c.a;

            if (!col.enabled || !col.gameObject.activeInHierarchy) a *= settings.colliderVisualizer_disabledAlphaMultiplier;
            else if (col.isTrigger) a *= settings.colliderVisualizer_triggerAlphaMultiplier;

            return new Color(c.r, c.g, c.b, a);
        }

        private static bool TryGetColor(SHUU_Debug settings, int layer, string tag, bool wire, out Color color)
        {
            CustomColors best = null;
            int bestScore = -1;

            foreach (var custom in settings.colliderVisualizer_customColors)
            {
                bool relevant = wire ? (custom.overrideWireColor || custom.hideWire) : (custom.overrideFillColor || custom.hideFill);
                if (!relevant) continue;

                bool layerMatch = custom.layerMask.Contains(layer);
                bool tagMatch = custom.tagMask.Contains(tag);

                if (custom.useAndMatching ? !(layerMatch && tagMatch) : !(layerMatch || tagMatch)) continue;

                int score = (layerMatch ? 1 : 0) + (tagMatch ? 1 : 0);
                if (custom.useAndMatching) score += 10;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = custom;
                }
            }

            if (best != null)
            {
                if (wire ? best.hideWire : best.hideFill)
                {
                    color = default;
                    return false;
                }

                color = wire ? best.wireColor : best.fillColor;
                return true;
            }

            color = wire ? settings.colliderVisualizer_defaultWireColor : settings.colliderVisualizer_defaultFillColor;
            return true;
        }
        #endregion



        #region Cleanup
        public void ClearMeshes()
        {
            foreach (Mesh m in wireMeshes) DestroySafe(m);
            foreach (Mesh m in fillMeshes) DestroySafe(m);

            wireMeshes = Array.Empty<Mesh>();
            fillMeshes = Array.Empty<Mesh>();

            colliders.Clear();
        }

        public void Dispose()
        {
            ClearMeshes();
            DestroyMaterials();
        }


        private static void DestroySafe(Object obj)
        {
            if (obj == null) return;

            if (Application.isPlaying) Object.Destroy(obj);
            else Object.DestroyImmediate(obj);
        }
        #endregion
    
        #endregion
    }
}

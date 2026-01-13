/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



using UnityEngine;
using System.Collections.Generic;

namespace SHUU.Utils.Developer.Debugging
{
    public class Debug_ColliderVisualizer : MonoBehaviour
    {
        public static Debug_ColliderVisualizer instance;


        public Shader shader;

        [Header("Toggle")]
        public KeyCode toggleKey = KeyCode.F1;

        [Header("Trigger Tone")]
        [Range(0f, 1f)] public float triggerToneMultiplier = 0.75f;

        [Header("Wire Settings")]
        [Range(0.5f, 3f)] public float wireThickness = 2f;

        [Header("Defaults")]
        public Color defaultFillColor = new Color(0, 1, 0, 0.15f);
        public Color defaultWireColor = new Color(0, 1, 0, 0.55f);

        [Header("Layer → Wire Color")]
        public List<LayerWireColor> layerWireColors = new();

        [Header("Tag → Fill Color")]
        public List<TagFillColor> tagFillColors = new();

        private readonly List<GameObject> visuals = new();
        private bool visible = true;

        #region Data

        [System.Serializable]
        public class LayerWireColor
        {
            public LayerMask layers;
            public Color wireColor;
        }

        [System.Serializable]
        public class TagFillColor
        {
            public List<string> tags = new();
            public Color fillColor;
        }

        #endregion

        #region Unity

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            CreateVisuals();
            Toggle(); // start hidden
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey)) Toggle();
        }

        #endregion

        #region Toggle

        public static bool Toggle()
        {
            return instance && instance._Toggle();
        }

        private bool _Toggle()
        {
            visible = !visible;
            foreach (var v in visuals)
                if (v) v.SetActive(visible);

            return visible;
        }

        #endregion

        #region Visual Creation

        void CreateVisuals()
        {
#if UNITY_6000_0_OR_NEWER
            Collider[] colliders = FindObjectsByType<Collider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            Collider[] colliders = Resources.FindObjectsOfTypeAll<Collider>();
#endif

            foreach (Collider col in colliders)
            {
                if (!col) continue;

                #if !UNITY_6000_0_OR_NEWER
                // Skip assets / prefabs
                if (col.gameObject.scene.name == null) continue;
                #endif

                GameObject vis = CreateForCollider(col);
                if (!vis) continue;

                vis.transform.SetParent(col.transform, false);
                visuals.Add(vis);
            }
        }

        GameObject CreateForCollider(Collider col)
        {
            return col switch
            {
                BoxCollider box => CreateBox(box),
                SphereCollider sphere => CreateSphere(sphere),
                CapsuleCollider capsule => CreateCapsule(capsule),
                MeshCollider mesh => CreateMesh(mesh),
                _ => null
            };
        }

        #endregion

        #region Collider Types

        GameObject CreateBox(BoxCollider box)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(go.GetComponent<Collider>());

            go.transform.localPosition = box.center;
            go.transform.localScale = box.size;

            ApplyMaterial(go, box);
            return go;
        }

        GameObject CreateSphere(SphereCollider sphere)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(go.GetComponent<Collider>());

            go.transform.localPosition = sphere.center;
            go.transform.localScale = Vector3.one * sphere.radius * 2f;

            ApplyMaterial(go, sphere);
            return go;
        }

        GameObject CreateCapsule(CapsuleCollider capsule)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Destroy(go.GetComponent<Collider>());

            go.transform.localPosition = capsule.center;

            float d = capsule.radius * 2f;
            float h = capsule.height;

            go.transform.localScale = capsule.direction switch
            {
                0 => new Vector3(h, d, d),
                1 => new Vector3(d, h, d),
                2 => new Vector3(d, d, h),
                _ => Vector3.one
            };

            ApplyMaterial(go, capsule);
            return go;
        }

        GameObject CreateMesh(MeshCollider mesh)
        {
            if (!mesh.sharedMesh) return null;

            GameObject go = new GameObject("MeshCollider_Visual");
            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();

            mf.sharedMesh = Instantiate(mesh.sharedMesh);
            mr.material = CreateMaterialForCollider(mesh);

            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;

            return go;
        }

        #endregion

        #region Material / Colors

        void ApplyMaterial(GameObject go, Collider col)
        {
            Renderer r = go.GetComponent<Renderer>();
            r.material = CreateMaterialForCollider(col);
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
        }

        Material CreateMaterialForCollider(Collider col)
        {
            Material mat = new Material(shader);

            Color fill = GetFillColor(col);
            Color wire = GetWireColor(col);

            if (col.isTrigger)
            {
                fill *= triggerToneMultiplier;
                wire *= triggerToneMultiplier;
            }

            mat.SetColor("_FillColor", fill);
            mat.SetColor("_WireColor", wire);
            mat.SetFloat("_WireThickness", wireThickness);

            mat.renderQueue = 3000;
            return mat;
        }

        Color GetWireColor(Collider col)
        {
            int colLayerMask = 1 << col.gameObject.layer;

            foreach (var l in layerWireColors)
            {
                if ((l.layers.value & colLayerMask) != 0)
                    return l.wireColor;
            }

            return defaultWireColor;
        }

        Color GetFillColor(Collider col)
        {
            foreach (var t in tagFillColors)
                if (t.tags.Contains(col.tag))
                    return t.fillColor;

            return defaultFillColor;
        }

        #endregion
    }
}

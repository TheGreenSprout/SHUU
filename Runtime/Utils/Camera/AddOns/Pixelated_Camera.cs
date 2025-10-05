using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace SHUU.Utils.Cameras.AddOns
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class Pixelated_Camera : MonoBehaviour
    {
        [Header("Pixelation Settings")]
        [Tooltip("Multiplier for the aspect ratio — bigger = chunkier pixels")]
        [Min(1)] public int scaleMultiplier = 50;

        [Tooltip("Show pixelation in Scene view in Edit mode")]
        public bool previewInSceneView = false;

        [Header("URP/HDRP Output")]
        [Tooltip("Optional: assign a RawImage to display the pixelated view in URP/HDRP")]
        public RawImage targetRawImage;

        private Camera cam;
        public RenderTexture rt;


        private enum Pipeline { BuiltIn, URP, HDRP }
        private Pipeline activePipeline;

        private float[] screenValues = new float[2];

        private void Awake()
        {
            if (screenValues == null || screenValues.Length < 2) screenValues = new float[2];
        }

        private void OnEnable()
        {
            if (screenValues == null || screenValues.Length < 2) screenValues = new float[2];
            cam = GetComponent<Camera>();
            DetectPipeline();

            UpdateRenderTexture();
        }

        private void DetectPipeline()
        {
            RenderPipelineAsset rpAsset = null;

#if UNITY_2022_1_OR_NEWER
            rpAsset = GraphicsSettings.defaultRenderPipeline;
#else
            rpAsset = GraphicsSettings.renderPipelineAsset;
#endif

            if (rpAsset == null)
                activePipeline = Pipeline.BuiltIn;
            else
            {
                string rpName = rpAsset.GetType().ToString();
                if (rpName.Contains("HD"))
                    activePipeline = Pipeline.HDRP;
                else
                    activePipeline = Pipeline.URP;
            }
        }

        // Built-in pipeline display
        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (activePipeline != Pipeline.BuiltIn) return;

#if UNITY_EDITOR
            if (!Application.isPlaying && !previewInSceneView) return;
#endif

            GetCurrentScreenSize();

            int[] closestRatio = GetClosestAspectRatio();
            float currentRatioSum = closestRatio[0] + closestRatio[1];

            int expectedWidth = Mathf.RoundToInt(closestRatio[0] * scaleMultiplier);
            int expectedHeight = Mathf.RoundToInt(closestRatio[1] * scaleMultiplier);

            if (rt == null || rt.width != expectedWidth || rt.height != expectedHeight)
                UpdateRenderTexture();

            Graphics.Blit(rt, dest);
        }

        private void OnDisable()
        {
            cam.targetTexture = null;
            if (rt != null) rt.Release();

            if (activePipeline != Pipeline.BuiltIn && targetRawImage != null)
                targetRawImage.texture = null;
        }

        public void UpdateRenderTexture(Vector2Int? customResolution = null)
        {
            if (rt != null) rt.Release();


            if (cam.targetTexture != null)
            {
                cam.targetTexture = null;
            }



            if (customResolution != null)
            {
                screenValues[0] = ((Vector2Int)customResolution)[0];
                screenValues[1] = ((Vector2Int)customResolution)[1];
            }
            else
            {
                GetCurrentScreenSize();
            }


            int[] closestRatio = GetClosestAspectRatio();

            int aspectWidth = closestRatio[0];
            int aspectHeight = closestRatio[1];

            int rt_width = Mathf.RoundToInt(aspectWidth * scaleMultiplier);
            int rt_height = Mathf.RoundToInt(aspectHeight * scaleMultiplier);

            rt = new RenderTexture(rt_width, rt_height, 16);
            rt.filterMode = FilterMode.Point;

            rt.Create();

            cam.targetTexture = rt;


            if (activePipeline != Pipeline.BuiltIn && targetRawImage != null)
                targetRawImage.texture = rt;
                
#if UNITY_EDITOR
            // Force RawImage update in editor preview
            if (!Application.isPlaying && targetRawImage != null) EditorUtility.SetDirty(targetRawImage);
#endif
        }

        // Returns the closest standard aspect ratio to the current screen
        int[] GetClosestAspectRatio()
        {
            float aspectRatio = screenValues[0] / screenValues[1];


            int[][] commonRatios = new int[][]
            {
                new int[] { 1, 1 },  // 1:1
                new int[] { 1, 2 },  // 1:2
                new int[] { 2, 3 },  // 2:3
                new int[] { 3, 4 },  // 3:4
                new int[] { 4, 5 },  // 4:5
                new int[] { 5, 6 },  // 5:6
                new int[] { 6, 7 },  // 6:7
                new int[] { 7, 8 },  // 7:8
                new int[] { 8, 9 },  // 8:9
                new int[] { 9, 10 }, // 9:10
                new int[] { 1, 3 },  // 1:3
                new int[] { 2, 5 },  // 2:5
                new int[] { 3, 7 },  // 3:7
                new int[] { 4, 9 },  // 4:9
                new int[] { 5, 8 },  // 5:8
                new int[] { 7, 10 }, // 7:10
                new int[] { 3, 5 },  // 3:5
                new int[] { 2, 7 },  // 2:7
                new int[] { 5, 9 },  // 5:9
                new int[] { 6, 11 }, // 6:11
                new int[] { 11, 13 }, // 11:13
                new int[] { 13, 14 }, // 13:14
                new int[] { 16, 9 }, // 16:9 (HD, Full HD, etc.)
                new int[] { 21, 9 }, // 21:9 (Ultrawide)
                new int[] { 16, 10 }, // 16:10 (Common for some displays)
                new int[] { 4, 3 },  // 4:3 (Traditional CRT aspect ratio)
                new int[] { 3, 2 },  // 3:2 (Common for cameras, such as 35mm film)
                new int[] { 2, 1 },  // 2:1 (Some cinematic aspect ratios)
                new int[] { 5, 4 },  // 5:4 (Common in older computer monitors)
                new int[] { 1, 4 },  // 1:4 (Tall aspect ratio, used for some phones)
            };


            // Find the closest match
            int[] closestRatio = new int[2];
            float closestDifference = float.MaxValue;

            for (int i = 0; i < commonRatios.Length; i++)
            {
                float difference = Mathf.Abs(aspectRatio - ((float)commonRatios[i][0] / commonRatios[i][1]));

                if (difference < closestDifference)
                {
                    closestDifference = difference;
                    closestRatio = commonRatios[i];
                }
            }

            while (closestRatio[0] + closestRatio[1] < 19)
            {
                closestRatio[0] *= 2;
                closestRatio[1] *= 2;
            }

            return closestRatio;
        }

        public void GetCurrentScreenSize()
        {
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);

#if UNITY_EDITOR
            screenSize = GetMainGameViewSize();
#endif


            screenValues[0] = screenSize.x;
            screenValues[1] = screenSize.y;
        }


#if UNITY_EDITOR
        public Vector2 GetMainGameViewSize()
        {
            System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
            var gv = EditorWindow.GetWindow(T);
            var sizeProp = T.GetProperty("currentGameViewSize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var gvSize = sizeProp.GetValue(gv, null);
            var width = (int)gvSize.GetType().GetProperty("width").GetValue(gvSize, null);
            var height = (int)gvSize.GetType().GetProperty("height").GetValue(gvSize, null);
            return new Vector2(width, height);
        }
#endif



        //[ContextMenu("Refresh All Pixelated Cameras")]
        public static void RefreshAll(Vector2Int? customResolution = null)
        {
            // Using the new API to avoid obsolete warning
            Pixelated_Camera[] cameras = Object.FindObjectsByType<Pixelated_Camera>(FindObjectsSortMode.None);

            foreach (var pixelCam in cameras)
            {
                pixelCam.UpdateRenderTexture(customResolution);
            }
        }
    }
}

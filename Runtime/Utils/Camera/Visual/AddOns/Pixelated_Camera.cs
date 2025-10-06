using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace SHUU.Utils.Cameras.Visual.AddOns
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class Pixelated_Camera : MonoBehaviour
    {
        [Header("Pixelation Settings")]
        [Tooltip("Multiplier for the aspect ratio. Smaller, more pixelated")]
        [Min(1)] public int scaleMultiplier = 50;


        [Header("URP/HDRP Output")]
        [Tooltip("Only for URP/HDRP: assign a RawImage to display the pixelated view")]
        [SerializeField] private RawImage _targetRawImage;
        private RawImage _assign_targetRawImage = null;
        public RawImage targetRawImage
        {
            get => _targetRawImage;
            set
            {
                _targetRawImage = value;

                EditorApplication.delayCall += () =>
                {
                    if (this != null) RefreshAll_MenuCommand();
                };
            }
        }



        private Camera cam;

        private RenderTexture rt;


        private enum Pipeline { BuiltIn, URP, HDRP }
        private Pipeline activePipeline;


        private int[] screenValues = new int[2];




        #region Setup
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_targetRawImage != null && _targetRawImage != _assign_targetRawImage)
            {
                _assign_targetRawImage = _targetRawImage;

                targetRawImage = _targetRawImage;
            }
        }
#endif


        void Awake()
        {
            cam = GetComponent<Camera>();
            DetectPipeline();
            UpdateRenderTexture();
        }

        void OnDestroy()
        {
            cam.targetTexture = null;
            if (rt != null) rt.Release();

            if (activePipeline != Pipeline.BuiltIn && targetRawImage != null)
                targetRawImage.texture = null;
        }

        void DetectPipeline()
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
        #endregion



        #region URP/HDRP pipeline display
        void UpdateRenderTexture()
        {
            if (rt != null) rt.Release();


            int[] closestRatio = GetClosestAspectRatio();

            int rtWidth = closestRatio[0] * scaleMultiplier;
            int rtHeight = closestRatio[1] * scaleMultiplier;


            rt = new RenderTexture(rtWidth, rtHeight, 16);
            rt.filterMode = FilterMode.Point;
            cam.targetTexture = rt;


            if (activePipeline != Pipeline.BuiltIn && targetRawImage != null)
                targetRawImage.texture = rt;
        }
        #endregion


        #region Built-in pipeline display
        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (activePipeline != Pipeline.BuiltIn) return;

            int[] closestRatio = GetClosestAspectRatio();
            int expectedWidth = closestRatio[0] * scaleMultiplier;
            int expectedHeight = closestRatio[1] * scaleMultiplier;

            if (rt == null || rt.width != expectedWidth || rt.height != expectedHeight)
                UpdateRenderTexture();

            Graphics.Blit(rt, dest);
        }
        #endregion



        #region Inner workings
        int[] GetClosestAspectRatio()
        {
            float aspectRatio = 0f;
            if (screenValues[1] != 0) aspectRatio = screenValues[0] / screenValues[1];


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
                new int[] { 16, 9 }, // 16:9
                new int[] { 21, 9 }, // 21:9
                new int[] { 16, 10 }, // 16:10
                new int[] { 4, 3 },  // 4:3
                new int[] { 3, 2 },  // 3:2
                new int[] { 2, 1 },  // 2:1
                new int[] { 5, 4 },  // 5:4
                new int[] { 1, 4 },  // 1:4
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

        public void GetCurrentScreenSize(Vector2Int? customResolution = null)
        {
            Vector2Int screenSize;
            if (customResolution == null)
            {
                screenSize = new Vector2Int(Screen.width, Screen.height);

#if UNITY_EDITOR
                screenSize = GetMainGameViewSize();
#endif
            }
            else
            {
                screenSize = new Vector2Int(((Vector2Int)customResolution)[0], ((Vector2Int)customResolution)[1]);
            }



            screenValues[0] = screenSize.x;
            screenValues[1] = screenSize.y;
        }

#if UNITY_EDITOR
        public static Vector2Int GetMainGameViewSize()
        {
            System.Type gameViewType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
            EditorWindow gameView = EditorWindow.GetWindow(gameViewType);
            Rect rect = gameView.position; // This gives the actual Game View rect
            return new Vector2Int(Mathf.RoundToInt(rect.width), Mathf.RoundToInt(rect.height));
        }
#endif
        #endregion



        // External handling
        [ContextMenu("Refresh All Pixelated Cameras")]
        public void RefreshAll_MenuCommand()
        {
            RefreshAll();
        }
        public static void RefreshAll(Vector2Int? customResolution = null)
        {
            Pixelated_Camera[] cameras = FindObjectsByType<Pixelated_Camera>(FindObjectsSortMode.None);

            foreach (var pixelCam in cameras)
            {
                pixelCam.GetCurrentScreenSize(customResolution);
                pixelCam.UpdateRenderTexture();
            }
        }
    }
}

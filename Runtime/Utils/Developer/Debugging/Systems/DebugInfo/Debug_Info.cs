using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using TMPro;

using Alchemy.Inspector;

using SHUU.Utils.Helpers;
using SHUU.Utils.Globals;
using SHUU.Utils.SceneManagement;

namespace SHUU.Utils.Developer.Debugging.Systems
{
    public class Debug_Info : MonoBehaviour
    {
        #region Variables

        #region Static Toggles
        private static bool ShowFrameGraph = false;
        private static bool FrameGraphCorner = true;


        private static bool ShowAxisIndicator = false;
        private static bool AxisIndicatorMode = true;

        private static bool ShowCompass = false;
        private static bool CompassEdgeShrink = true;
        private static bool CompassBottom = false;
        #endregion



        #region Inspector
        [Tooltip("Defaults to Camera.main if left empty.")]
        [SerializeField, BoxGroup("References")] private Transform playerTransform;
        [Tooltip("Defaults to Camera.main if left empty.")]
        [SerializeField, BoxGroup("References")] private Camera targetCamera;

        [SerializeField, BoxGroup("References/Internal")] private GameObject container;
        [SerializeField, BoxGroup("References/Internal")] private TMP_Text leftText;
        [SerializeField, BoxGroup("References/Internal")] private TMP_Text rightText;

        [SerializeField, BoxGroup("References/Internal"), LabelText("GL Shader")] private Shader glShader;


        [Tooltip("Rebuild text every N seconds. Unaffected by timescale.")]
        [SerializeField, BoxGroup("Settings"), Range(0.02f, 2f)] private float updateInterval = 0.05f;
        [Tooltip("Background color drawn behind each line of text.")]
        [SerializeField, BoxGroup("Settings")] private Color lineBackground = new Color(0f, 0f, 0f, 0.5f);
        [SerializeField, BoxGroup("Settings")] private bool showLookTarget = true;
        [Tooltip("If 0f the raycast distance will be unlimited.")]
        [SerializeField, BoxGroup("Settings"), ShowIf("showLookTarget"), Min(0f)] private float lookMaxDistance = 100f;
        #endregion



        #region Internal
        private SHUU_Timer update = null;


        private IDebug_InfoTarget _lastInfoTarget = null;
        private IDebug_InfoTarget lastInfoTarget
        {
            get => _lastInfoTarget;
            set
            {
                if (_lastInfoTarget == value) return;

                _lastInfoTarget?.HoverEnd(); 
                _lastInfoTarget = value;
                _lastInfoTarget?.HoverStart();
            }
        }

        
        private Material _glMat = null;
        private Material glMat
        {
            get
            {
                if (_glMat != null) return _glMat;
                if (glShader == null) return null;

                _glMat = new Material(glShader) { hideFlags = HideFlags.HideAndDontSave };
                return _glMat;
            }
        }

        private const int FrameBufferSize = 128;
        private float[] frameTimes;
        private int frameHead;

        
        private string cachedLeftInfo;

        private readonly StringBuilder sb = new(512);
        #endregion

        #endregion




        #region Main
        private void Awake()
        {
            var cam = Camera.main;
            if (targetCamera == null) targetCamera = cam;
            if (playerTransform == null) playerTransform = cam.transform;

            if (container != null && container.activeInHierarchy) container.SetActive(false);

            frameTimes = new float[FrameBufferSize];

            CacheSysInfo();
        }


        #region Toggles
        public void Toggle()
        {
            if (container == null || !SHUU_Debug.Instance.debugInfo_enabled) return;


            container.SetActive(!container.activeInHierarchy);

            if (container.activeInHierarchy) BuildText();
            else 
            {
                update?.Cancel();
                update = null;
            }
        }

        public void ToggleFpsGraph(){
            if (SHUU_Debug.Instance.debugInfo_enabled) ShowFrameGraph = !ShowFrameGraph;
        }
        public void FpsGraphCorner() {
            if (ShowFrameGraph) FrameGraphCorner = !FrameGraphCorner;
        }

        public void ToggleAxis() {
            if (container.activeInHierarchy) ShowAxisIndicator = !ShowAxisIndicator;
        }
        public void AxisMode() {
            if (container.activeInHierarchy && ShowAxisIndicator) AxisIndicatorMode = !AxisIndicatorMode;
        }
        public void ToggleCompass() {
            if (container.activeInHierarchy) ShowCompass = !ShowCompass;
        }
        public void CompassMode() {
            if (container.activeInHierarchy && ShowCompass) CompassEdgeShrink = !CompassEdgeShrink;
        }
        public void CompassPosition() {
            if (container.activeInHierarchy && ShowCompass) CompassBottom = !CompassBottom;
        }
        #endregion


        private void Update()
        {
            frameTimes[frameHead] = Time.unscaledDeltaTime * 1000f;
            frameHead = (frameHead + 1) % FrameBufferSize;
        }

        private void OnGUI() => BuildOverlays();


        private void BuildText()
        {
            update?.Cancel();
            update = SHUU_Time.Timer(updateInterval, BuildText, ignoreTimeScale: true);
            
            BuildLeft();
            BuildRight();
        }
        #endregion



        #region Logic

        #region Text Building
        private void BuildLeft()
        {
            Clear();


            // FPS
            Line($"{Stats.Fps:F0} fps  ({Stats.Frametimems:F2} ms)");

            Line();

            // Position
            Transform t = playerTransform != null ? playerTransform : Camera.main   != null ? Camera.main.transform : null;
            if (t != null)
            {
                Vector3 pos = t.position;
                Line($"Position  [X: {pos.x:F3} / Y: {pos.y:F3} / Z: {pos.z:F3}]");

                Line();
            }

            // Camera look direction
            Camera cam = targetCamera != null ? targetCamera : Camera.main;
            if (cam != null)
            {
                Vector3 fwd = cam.transform.forward;
                float yaw = cam.transform.eulerAngles.y;

                Line($"Facing: {CardinalDirection(yaw)}  [{fwd.x:F2} / {fwd.y:F2} / {fwd.z:F2}]");
                Line($"FOV: {cam.fieldOfView:F1}°");

                Line();
            }

            // Look target
            if (showLookTarget && cam != null)
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                bool hitRegistered = lookMaxDistance > 0 ? Physics.Raycast(ray, out hit, lookMaxDistance) : Physics.Raycast(ray, out hit);
                if (hitRegistered && hit.collider == null && hit.collider.gameObject.TryGetComponent(out IDebug_InfoTarget infoTarget)) lastInfoTarget = infoTarget;
                else lastInfoTarget = null;
                string target = hitRegistered ? hit.collider.gameObject.name : "—";

                Line($"Looking at: {target}");

                Line();
            }

            // Scene / time
            Line($"Scene: {SceneLoader.GetCurrentSceneName()}");
            Line($"Scene roots: {SceneLoader.GetCurrentScene().rootCount}");

            Line();

            // Pools
            var pools = ObjectPooling.Pools;
            Line($"Pools: {pools.Count}");
            foreach (var pool in pools)
            {
                int active = pool.totalCount - pool.poolCount;
                Line($"{pool.name}  {active}/{pool.totalCount}", prefix: "  ");
            }

            Line();

            Line($"Time: {FormatTime(Time.time)}  (x{Time.timeScale:F2})");
            Line($"Fixed dt: {Time.fixedDeltaTime * 1000f:F2} ms");

            Line();

            // Cursor
            string cursorLock = Cursor.lockState switch
            {
                CursorLockMode.None => "Free",
                CursorLockMode.Locked => "Locked",
                CursorLockMode.Confined => "Confined",
                _ => Cursor.lockState.ToString()
            };
            Line($"Cursor: {cursorLock}  ({(Cursor.visible ? "Visible" : "Hidden")})");


            leftText.text = GetString();
        }


        private void BuildRight()
        {
            sb.Clear();


            Line($"Mem: {Stats.Totalallocated / 1_048_576L} / {Stats.Totalreserved / 1_048_576L} MB");
            Line($"Mono: {Stats.Monoused / 1_048_576L} / {Stats.Monoheap / 1_048_576L} MB");
        
            Line();

            Line(cachedLeftInfo, false);


            rightText.text = GetString();
        }

        private void CacheSysInfo()
        {
            Clear();


            Line($"Display: {Screen.width}x{Screen.height}  ({Screen.dpi:F0} dpi)");
            Line($"Refresh: {Stats.Refreshrate:F0} Hz  VSync: {QualitySettings.vSyncCount}");
            Line($"Target FPS: {(Application.targetFrameRate == -1 ? "Uncapped" : Application.targetFrameRate.ToString())}");
            Line($"Fullscreen: {Screen.fullScreenMode}");

            Line();

            Line($"OS: {Stats.Os}");
            Line($"CPU: {Stats.Cpu}");
            Line($"     {Stats.Cpucores} cores @ {SystemInfo.processorFrequency} MHz");
            Line($"GPU: {Stats.Gpu}");
            Line($"     VRAM: {Stats.Gpumemory} MB  ({SystemInfo.graphicsDeviceType})");

            Line();

            Line($"Platform: {Application.platform}");
            Line($"Version: {Application.version}");
            Line($"Unity: {Application.unityVersion}");
            Line($"Quality: {QualitySettings.names[QualitySettings.GetQualityLevel()]}");
            Line($"Pipeline: {GraphicsSettings.currentRenderPipeline?.name ?? "Built-in"}");

            Line();

            Line($"Audio: {AudioSettings.outputSampleRate} Hz");


            cachedLeftInfo = GetString();
        }
        #endregion



        /*
        ⚠️‼️ AI ASSISTED SNIPPET
        This code snippet was written with the assistance of AI.
        */
        #region Overlays
        private void BuildOverlays()
        {
            if (Event.current.type != EventType.Repaint) return;

            Camera cam = targetCamera != null ? targetCamera : Camera.main;
            if (cam == null) return;

            if (container != null && container.activeInHierarchy)
            {
                if (ShowAxisIndicator) DrawAxisWidget(cam);
                if (ShowCompass) DrawCompass(cam);
            }
            if (ShowFrameGraph) DrawFrameGraph();
        }

        
        private void DrawAxisWidget(Camera cam)
        {
            if (glMat == null) return;

            const float len = 40f;
            const float labelSz = 16f;
            float cx = Screen.width  * 0.5f;
            float cy = Screen.height * 0.5f;

            Vector3[] world = { Vector3.right, Vector3.up, Vector3.forward };
            Color[] colors = { Color.red, new Color(0.2f, 0.9f, 0.2f), new Color(0.35f, 0.6f, 1f) };
            string[] lbls = { AxisIndicatorMode ? "X" : "", AxisIndicatorMode ? "Y" : "", AxisIndicatorMode ? "Z" : "" };
            float[] depth = {
                Vector3.Dot(Vector3.right, cam.transform.forward),
                Vector3.Dot(Vector3.up, cam.transform.forward),
                Vector3.Dot(Vector3.forward, cam.transform.forward),
            };

            // Insertion sort 3 elements back-to-front (largest depth first = furthest behind camera first)
            for (int i = 1; i < 3; i++)
                for (int j = i; j > 0 && depth[j] > depth[j - 1]; j--)
                {
                    (world[j], world[j-1]) = (world[j-1], world[j]);
                    (colors[j], colors[j-1]) = (colors[j-1], colors[j]);
                    (lbls[j], lbls[j-1]) = (lbls[j-1], lbls[j]);
                    (depth[j], depth[j-1]) = (depth[j-1], depth[j]);
                }

            GL.PushMatrix();
            GL.LoadPixelMatrix();
            glMat.SetPass(0);
            GL.Begin(GL.LINES);
            for (int i = 0; i < 3; i++)
            {
                float dx = Vector3.Dot(world[i], cam.transform.right) * len;
                float dy = -Vector3.Dot(world[i], cam.transform.up) * len;
                Color c = depth[i] < 0f ? new Color(colors[i].r * 0.3f, colors[i].g * 0.3f, colors[i].b * 0.3f) : colors[i];
                GL.Color(c);
                GL.Vertex3(cx, cy, 0);
                GL.Vertex3(cx + dx, cy + dy, 0);
            }
            GL.End();
            GL.PopMatrix();

            var style = new GUIStyle { fontSize = 10, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            for (int i = 0; i < 3; i++)
            {
                if (lbls[i].Length == 0) continue;
                float nx =  Vector3.Dot(world[i], cam.transform.right);
                float ny = -Vector3.Dot(world[i], cam.transform.up);
                float lx = cx + nx * (len + 8f) - labelSz * 0.5f;
                float ly = cy + ny * (len + 8f) - labelSz * 0.5f;
                Color c = depth[i] < 0f ? new Color(colors[i].r * 0.3f, colors[i].g * 0.3f, colors[i].b * 0.3f) : colors[i];
                Rect r = new Rect(lx, ly, labelSz, labelSz);
                style.normal.textColor = Color.black;
                GUI.Label(new Rect(r.x + 1, r.y + 1, r.width, r.height), lbls[i], style);
                style.normal.textColor = c;
                GUI.Label(r, lbls[i], style);
            }
        }


        private void DrawCompass(Camera cam)
        {
            float yaw = cam.transform.eulerAngles.y;
            const float visibleDeg = 90f;
            const float barW = 260f;
            const float barH = 18f;
            float barX = (Screen.width - barW) * 0.5f;
            float barY = CompassBottom ? Screen.height - barH - 8f : 8f;

            GUI.color = lineBackground;
            GUI.DrawTexture(new Rect(barX, barY, barW, barH), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Tick marks — 32 steps of 11.25° per full rotation
            for (int i = 0; i < 32; i++)
            {
                float angle = i * 11.25f;
                float diff  = Mathf.DeltaAngle(yaw, angle);
                if (Mathf.Abs(diff) > visibleDeg * 0.5f) continue;
                if (i % 4 == 0) continue; // 45° slots have labels instead

                float px = barX + barW * 0.5f - (diff / visibleDeg) * barW;

                float tickH, alpha;
                if (i % 2 == 0) // 22.5° — medium
                {
                    tickH = 7f;
                    alpha = 0.75f;
                }
                else // 11.25° — small
                {
                    tickH = 4f;
                    alpha = 0.45f;
                }

                GUI.color = new Color(1f, 1f, 1f, alpha);
                GUI.DrawTexture(new Rect(px - 0.5f, barY + barH - tickH, 1f, tickH), Texture2D.whiteTexture);
            }
            GUI.color = Color.white;

            // Direction labels
            var style = new GUIStyle { fontSize = 11, alignment = TextAnchor.MiddleCenter };
            foreach (var (text, angle) in CompassDirs)
            {
                float diff = Mathf.DeltaAngle(yaw, angle);
                if (Mathf.Abs(diff) > visibleDeg * 0.5f) continue;

                float px = barX + barW * 0.5f - (diff / visibleDeg) * barW;
                bool cardinal = text.Length == 1;
                style.fontStyle = cardinal ? FontStyle.Bold : FontStyle.Normal;
                style.normal.textColor = cardinal ? Color.white : new Color(0.75f, 0.75f, 0.75f);

                if (CompassEdgeShrink)
                {
                    float t = Mathf.Abs(diff) / (visibleDeg * 0.5f);
                    t = t * t * t; // cubic — stays full-size near center, shrinks sharply near edges
                    float scaleX = Mathf.Lerp(1f, 0.3f, t);
                    var prevMatrix = GUI.matrix;
                    GUIUtility.ScaleAroundPivot(new Vector2(scaleX, 1f), new Vector2(px, barY + barH * 0.5f));
                    GUI.Label(new Rect(px - 15f, barY, 30f, barH), text, style);
                    GUI.matrix = prevMatrix;
                }
                else
                {
                    GUI.Label(new Rect(px - 15f, barY, 30f, barH), text, style);
                }
            }

            // Centre heading marker
            GUI.color = Color.yellow;
            GUI.DrawTexture(new Rect(barX + barW * 0.5f - 1f, barY, 2f, barH), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        private void DrawFrameGraph()
        {
            const float barW = 2f;
            const float graphH = 60f;
            const float maxMs = 50f;
            float graphW = FrameBufferSize * barW;
            float gx = FrameGraphCorner ? Screen.width - graphW - 8f : (Screen.width - graphW) * 0.5f;
            float gy = 22f; // leaves room for the label above

            float targetMs = Application.targetFrameRate > 0 ? 1000f / Application.targetFrameRate : 1000f / 60f;
            float target30Ms = 1000f / 30f;

            // Background
            GUI.color = new Color(0f, 0f, 0f, 0.6f);
            GUI.DrawTexture(new Rect(gx, gy, graphW, graphH), Texture2D.whiteTexture);

            // Target lines
            float yTarget = gy + graphH - Mathf.Clamp01(targetMs  / maxMs) * graphH;
            float y30 = gy + graphH - Mathf.Clamp01(target30Ms / maxMs) * graphH;
            GUI.color = new Color(0.2f, 0.9f, 0.2f, 0.5f);
            GUI.DrawTexture(new Rect(gx, yTarget, graphW, 1f), Texture2D.whiteTexture);
            GUI.color = new Color(1f, 0.85f, 0.1f, 0.5f);
            GUI.DrawTexture(new Rect(gx, y30, graphW, 1f), Texture2D.whiteTexture);

            // Bars oldest → newest
            for (int i = 0; i < FrameBufferSize; i++)
            {
                float ms = frameTimes[(frameHead + i) % FrameBufferSize];
                float h = Mathf.Clamp01(ms / maxMs) * graphH;
                float bx = gx + i * barW;
                float by = gy + graphH - h;

                GUI.color = ms <= targetMs ? new Color(0.2f, 0.9f, 0.2f) : ms <= target30Ms ? new Color(1f, 0.85f, 0.1f) : new Color(1f, 0.25f, 0.25f);

                GUI.DrawTexture(new Rect(bx, by, barW - 0.5f, h), Texture2D.whiteTexture);
            }
            GUI.color = Color.white;

            // FPS labels to the right of target lines
            var labelStyle = new GUIStyle { fontSize = 9, alignment = TextAnchor.MiddleLeft };
            float lx = FrameGraphCorner ? gx - 40f : gx + graphW + 3f;
            labelStyle.normal.textColor = new Color(0.2f, 0.9f, 0.2f);
            GUI.Label(new Rect(lx, yTarget - 6f, 36f, 12f), $"{Mathf.RoundToInt(1000f / targetMs)}fps", labelStyle);
            labelStyle.normal.textColor = new Color(1f, 0.85f, 0.1f);
            GUI.Label(new Rect(lx, y30 - 6f, 36f, 12f), "30fps", labelStyle);

            // Current frametime above the graph
            labelStyle.fontSize = 10;
            labelStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(gx, gy - 14f, graphW, 13f), $"{Stats.Fps:F0} fps  {Stats.Frametimems:F2} ms", labelStyle);
        }
        #endregion



        #region Helpers
        private void Clear() => sb.Clear();
        
        private void Line(string text, bool marked = true, string prefix = "")
        {
            if (marked) sb.AppendLine(prefix + text.RichText_Marked(lineBackground, true));
            else sb.AppendLine(prefix + text);
        }
        private void Line() => sb.AppendLine();

        private string GetString() => sb.ToString();


        private static string CardinalDirection(float yaw)
        {
            float y = ((yaw % 360f) + 360f) % 360f;

            if (y <  22.5f || y >= 337.5f) return "N";
            if (y <  67.5f) return "NE";
            if (y < 112.5f) return "E";
            if (y < 157.5f) return "SE";
            if (y < 202.5f) return "S";
            if (y < 247.5f) return "SW";
            if (y < 292.5f) return "W";

            return "NW";
        }

        private static string FormatTime(float seconds)
        {
            int h = Mathf.FloorToInt(seconds / 3600f);
            int m = Mathf.FloorToInt(seconds % 3600f / 60f);
            int s = Mathf.FloorToInt(seconds % 60f);

            return h > 0 ? $"{h}:{m:D2}:{s:D2}" : $"{m}:{s:D2}";
        }


        private static readonly (string text, float angle)[] CompassDirs =
            { ("N", 0f), ("NE", 45f), ("E", 90f), ("SE", 135f), ("S", 180f), ("SW", 225f), ("W", 270f), ("NW", 315f) };
        #endregion

        #endregion
    }
}

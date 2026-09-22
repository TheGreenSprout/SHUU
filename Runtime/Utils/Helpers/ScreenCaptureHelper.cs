using System.IO;
using System.Threading.Tasks;
using UnityEngine;

using SHUU.Utils.Developer.Debugging;

namespace SHUU.Utils.Helpers
{
    public static class ScreenCaptureHelper
    {
        #region Variables
        public static string LastPath { get; private set; }


        private static Texture2D LastScreenshotTexture;


        private static GameObject[] Cache_objs;
        #endregion




        #region Logic

        #region File Path
        private static string GetFileName(string prefix, string extension)
        {
            if (!string.IsNullOrEmpty(prefix)) return $"{prefix}_{Stats.Timestamp}.{extension}";


            return $"screenshot_{Stats.Timestamp}.{extension}";
        }

        private static string GetDirectory(string customDir)
        {
            if (!string.IsNullOrEmpty(customDir)) return customDir;


            return Application.persistentDataPath;
        }


        private static string BuildFullPath(string prefix, string customDir, string extension)
        {
            string dir = GetDirectory(customDir);

            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string fileName = GetFileName(prefix, extension);


            return Path.Combine(dir, fileName);
        }
        #endregion



        #region Toggle GameObjects
        public static void HideUI(GameObject[] objs)
        {
            Cache_objs = objs;

            foreach (var c in objs) c.SetActive(false);
        }

        public static async void ShowUI()
        {
            await Task.Delay(50);
            

            if (Cache_objs == null) return;

            foreach (var c in Cache_objs) c.SetActive(true);
        }
        #endregion



        #region Capture
        public static void Capture(string prefix = null, string customDir = null, bool showScreenshot = false, GameObject[] hideUI = null)
        {
            string path = BuildFullPath(prefix, customDir, "png");
            LastPath = path;

            if (hideUI != null) HideUI(hideUI);

            ScreenCapture.CaptureScreenshot(path);

            if (hideUI != null) ShowUI();


            if (showScreenshot) Delayed_OpenLastScreenshot();
        }

        public static void CaptureScaled(int scale, string prefix = null, string customDir = null, bool showScreenshot = false, GameObject[] hideUI = null)
        {
            string path = BuildFullPath(prefix, customDir, "png");
            LastPath = path;

            if (hideUI != null) HideUI(hideUI);

            ScreenCapture.CaptureScreenshot(path, scale);

            if (hideUI != null) ShowUI();


            if (showScreenshot) Delayed_OpenLastScreenshot();
        }


        public static Texture2D CaptureCamera(Camera cam, int w, int h, bool hdr = false)
        {
            RenderTexture rt = new RenderTexture(w, h, 24);
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(w, h, hdr ? TextureFormat.RGBAFloat : TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();

            cam.targetTexture = null;
            RenderTexture.active = null;
            Object.Destroy(rt);

            LastScreenshotTexture = tex;
            return tex;
        }


        public static string SaveCameraImage(Camera cam, int w, int h, bool jpg = false, string prefix = null, string dir = null, bool showScreenshot = false)
        {
            Texture2D tex = CaptureCamera(cam, w, h);
            byte[] bytes = jpg ? tex.EncodeToJPG(95) : tex.EncodeToPNG();

            string ext = jpg ? "jpg" : "png";
            string path = BuildFullPath(prefix, dir, ext);
            LastPath = path;

            File.WriteAllBytes(path, bytes);


            if (showScreenshot) Delayed_OpenLastScreenshot();


            return path;
        }


        public static byte[] CaptureCameraBytes(Camera cam, int w, int h, bool jpg = false)
        {
            Texture2D tex = CaptureCamera(cam, w, h);
            return jpg ? tex.EncodeToJPG(95) : tex.EncodeToPNG();
        }


        // Whatever is actually on screen (every camera, all UI), not tied to a single Camera like CaptureCamera is.
        public static Texture2D CaptureScreenshotAsTexture() => ScreenCapture.CaptureScreenshotAsTexture();
        #endregion



        #region Resize
        // Downscales (never upscales) a texture to a target height, keeping its aspect ratio. Doesn't touch/destroy "source", that's the caller's.
        public static Texture2D ResizeTexture(Texture2D source, int targetHeight)
        {
            if (source == null) return null;

            int height = Mathf.Clamp(targetHeight, 16, Mathf.Max(16, source.height));
            int width = Mathf.Max(1, Mathf.RoundToInt(height * (float)source.width / source.height));

            RenderTexture previous = RenderTexture.active;
            RenderTexture target = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);

            try
            {
                Graphics.Blit(source, target);
                RenderTexture.active = target;

                Texture2D resized = new Texture2D(width, height, TextureFormat.RGB24, false);
                resized.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                resized.Apply();

                return resized;
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(target);
            }
        }
        #endregion



        #region Last Screenshot
        private static async void Delayed_OpenLastScreenshot()
        {
            await Task.Delay(200);
            OpenLastScreenshot();
        }
        public static void OpenLastScreenshot()
        {
            if (!string.IsNullOrEmpty(LastPath) && File.Exists(LastPath)) Application.OpenURL(LastPath);
        }


        public static Texture2D LoadLastScreenshot()
        {
            if (string.IsNullOrEmpty(LastPath)) return null;
            if (!File.Exists(LastPath)) return null;

            byte[] data = File.ReadAllBytes(LastPath);

            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(data);

            LastScreenshotTexture = tex;
            return tex;
        }

        public static Texture2D GetLastScreenshotTexture() => LastScreenshotTexture;
        #endregion

        #endregion
    }
}

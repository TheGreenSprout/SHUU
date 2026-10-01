using System.Globalization;
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




        #region Main
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            LastPath = null;
            LastScreenshotTexture = null;
            Cache_objs = null;
        }
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


        public static Texture2D CaptureScreenshotAsTexture() => ScreenCapture.CaptureScreenshotAsTexture();


        #region XML doc
        /// <summary>
        /// Encodes a texture to PNG and saves it, using the same file naming and folder convention as Capture/CaptureScaled. Useful for
        /// saving a texture you've already processed yourself (resized, cropped...) rather than capturing straight to a file.
        /// </summary>
        #endregion
        public static string SaveTexture(Texture2D texture, string prefix = null, string customDir = null, bool showScreenshot = false)
        {
            if (texture == null) return null;

            string path = BuildFullPath(prefix, customDir, "png");
            LastPath = path;

            File.WriteAllBytes(path, texture.EncodeToPNG());
            LastScreenshotTexture = texture;


            if (showScreenshot) Delayed_OpenLastScreenshot();

            return path;
        }
        #endregion



        #region Resize
        #region XML doc
        /// <summary>
        /// Parses a width-divided-by-height ratio from text: "16:9", "16/9", or a plain number like "1.78". Returns false for anything else
        /// (missing/zero height, not a number...).
        /// </summary>
        #endregion
        public static bool TryParseAspectRatio(string text, out float ratio)
        {
            ratio = 0f;
            if (string.IsNullOrWhiteSpace(text)) return false;

            int separatorIndex = text.IndexOfAny(new[] { ':', '/' });

            if (separatorIndex >= 0)
            {
                if (!float.TryParse(text.Substring(0, separatorIndex), NumberStyles.Float, CultureInfo.InvariantCulture, out float w)) return false;
                if (!float.TryParse(text.Substring(separatorIndex + 1), NumberStyles.Float, CultureInfo.InvariantCulture, out float h)) return false;
                if (h <= 0f) return false;

                ratio = w / h;
            }
            else if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out ratio)) return false;

            return ratio > 0f;
        }


        #region XML doc
        /// <summary>
        /// Resizes to a given height, keeping the source's own aspect ratio (the width follows from it, so nothing is cropped or stretched).
        /// </summary>
        #endregion
        public static Texture2D ResizeTexture(Texture2D source, int targetHeight)
        {
            if (source == null) return null;

            int height = Mathf.Clamp(targetHeight, 16, Mathf.Max(16, source.height));
            int width = Mathf.Max(1, Mathf.RoundToInt(height * (float)source.width / source.height));

            return Blit(source, width, height, Vector2.one, Vector2.zero);
        }

        #region XML doc
        /// <summary>
        /// Resizes to a given height and a width worked out from it and an aspect ratio (width divided by height: 16f/9f, 1f for square...),
        /// cropping the source first (centered) to match that ratio. Same idea as the plain height overload, just with a chosen ratio instead
        /// of the source's own.
        /// </summary>
        #endregion
        public static Texture2D ResizeTexture(Texture2D source, int targetHeight, float aspectRatio)
        {
            if (source == null) return null;

            int height = Mathf.Clamp(targetHeight, 16, Mathf.Max(16, source.height));
            int width = Mathf.Max(1, Mathf.RoundToInt(height * aspectRatio));

            return ResizeTexture(source, width, height);
        }

        #region XML doc
        /// <summary>
        /// Resizes to an exact width and height by cropping the source first (centered), so the result fills the whole frame without being
        /// stretched. Whichever side the source has "too much" of, compared to the target's own ratio, is trimmed off both edges equally.
        /// </summary>
        #endregion
        public static Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            if (source == null) return null;

            int width = Mathf.Max(1, targetWidth);
            int height = Mathf.Max(1, targetHeight);

            float sourceAspect = (float)source.width / source.height;
            float targetAspect = (float)width / height;

            Vector2 scale, offset;

            if (sourceAspect > targetAspect)
            {
                scale = new Vector2(targetAspect / sourceAspect, 1f);
                offset = new Vector2((1f - scale.x) * 0.5f, 0f);
            }
            else
            {
                scale = new Vector2(1f, sourceAspect / targetAspect);
                offset = new Vector2(0f, (1f - scale.y) * 0.5f);
            }

            return Blit(source, width, height, scale, offset);
        }


        private static Texture2D Blit(Texture2D source, int width, int height, Vector2 scale, Vector2 offset)
        {
            RenderTexture previous = RenderTexture.active;
            RenderTexture target = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);

            try
            {
                Graphics.Blit(source, target, scale, offset);
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

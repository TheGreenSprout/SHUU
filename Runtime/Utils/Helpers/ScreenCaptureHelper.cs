using System.IO;
using System.Threading.Tasks;
using SHUU.Utils.Developer.Debugging;
using SHUU.Utils.Globals;
using UnityEngine;

namespace SHUU.Utils.Helpers
{
    public static class ScreenCaptureHelper
    {
        public static string lastPath { get; private set; }


        private static Texture2D lastScreenshotTexture;


        private static GameObject[] cache_objs;




        private static string GetFileName(string prefix, string extension)
        {
            if (!string.IsNullOrEmpty(prefix)) return $"{prefix}_{Stats.timestamp}.{extension}";


            return $"screenshot_{Stats.timestamp}.{extension}";
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



        public static void HideUI(GameObject[] objs)
        {
            cache_objs = objs;

            foreach (var c in objs) c.SetActive(false);
        }

        public static async void ShowUI()
        {
            await Task.Delay(50);



            if (cache_objs == null) return;

            foreach (var c in cache_objs) c.SetActive(true);
        }



        public static void Capture(string prefix = null, string customDir = null, bool showScreenshot = false, GameObject[] hideUI = null)
        {
            string path = BuildFullPath(prefix, customDir, "png");
            lastPath = path;

            if (hideUI != null) HideUI(hideUI);

            ScreenCapture.CaptureScreenshot(path);

            if (hideUI != null) ShowUI();


            if (showScreenshot) Delayed_OpenLastScreenshot();
        }

        public static void CaptureScaled(int scale, string prefix = null, string customDir = null, bool showScreenshot = false, GameObject[] hideUI = null)
        {
            string path = BuildFullPath(prefix, customDir, "png");
            lastPath = path;

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

            lastScreenshotTexture = tex;
            return tex;
        }


        public static string SaveCameraImage(Camera cam, int w, int h, bool jpg = false, string prefix = null, string dir = null, bool showScreenshot = false)
        {
            Texture2D tex = CaptureCamera(cam, w, h);
            byte[] bytes = jpg ? tex.EncodeToJPG(95) : tex.EncodeToPNG();

            string ext = jpg ? "jpg" : "png";
            string path = BuildFullPath(prefix, dir, ext);
            lastPath = path;

            File.WriteAllBytes(path, bytes);


            if (showScreenshot) Delayed_OpenLastScreenshot();


            return path;
        }


        public static byte[] CaptureCameraBytes(Camera cam, int w, int h, bool jpg = false)
        {
            Texture2D tex = CaptureCamera(cam, w, h);
            return jpg ? tex.EncodeToJPG(95) : tex.EncodeToPNG();
        }



        private static async void Delayed_OpenLastScreenshot()
        {
            await Task.Delay(200);
            OpenLastScreenshot();
        }
        public static void OpenLastScreenshot()
        {
            if (!string.IsNullOrEmpty(lastPath) && File.Exists(lastPath))
            {
                Application.OpenURL(lastPath);
            }
        }


        public static Texture2D LoadLastScreenshot()
        {
            if (string.IsNullOrEmpty(lastPath)) return null;
            if (!File.Exists(lastPath)) return null;

            byte[] data = File.ReadAllBytes(lastPath);

            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(data);

            lastScreenshotTexture = tex;
            return tex;
        }

        public static Texture2D GetLastScreenshotTexture()
        {
            return lastScreenshotTexture;
        }
    }
}

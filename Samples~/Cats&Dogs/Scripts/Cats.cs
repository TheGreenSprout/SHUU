using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

using SHUU.Utils.Globals;

namespace SHUU.Samples.CatsDogs
{
    public static class Cats
    {
        #region Cat Images
        public static void GetCatImage(Action<Texture2D> onSuccess, Action<string> onError = null, int? width = null, int? height = null)
            => SHUU_Time.StartCoroutineStatic(GetCatImageEnumerator(onSuccess, onError, width, height));
        public static IEnumerator GetCatImageEnumerator(Action<Texture2D> onSuccess, Action<string> onError = null, int? width = null, int? height = null)
        {
            string url = BuildUrl("https://cataas.com/cat", width, height);

            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) onSuccess?.Invoke(DownloadHandlerTexture.GetContent(request));
            else onError?.Invoke(request.error);
        }

        public static void GetCatGif(Action<byte[]> onSuccess, Action<string> onError = null, int? width = null, int? height = null)
            => SHUU_Time.StartCoroutineStatic(GetCatGifEnumerator(onSuccess, onError, width, height));
        public static IEnumerator GetCatGifEnumerator(Action<byte[]> onSuccess, Action<string> onError = null, int? width = null, int? height = null)
        {
            string url = BuildUrl("https://cataas.com/cat/gif", width, height);

            using UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) onSuccess?.Invoke(request.downloadHandler.data);
            else onError?.Invoke(request.error);
        }


        private static string BuildUrl(string baseUrl, int? width, int? height)
        {
            if (width == null && height == null) return baseUrl;
            if (width != null && height != null) return $"{baseUrl}?width={width}&height={height}";
            if (width != null) return $"{baseUrl}?width={width}";

            return $"{baseUrl}?height={height}";
        }
        #endregion



        #region Cat Facts
        public static void GetCatImage(Action<string> onSuccess, Action<string> onError = null)
            => SHUU_Time.StartCoroutineStatic(GetCatFactEnumerator(onSuccess, onError));
        public static IEnumerator GetCatFactEnumerator(Action<string> onSuccess, Action<string> onError = null)
        {
            using UnityWebRequest request = UnityWebRequest.Get("https://catfact.ninja/fact");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                CatFactResponse response = JsonUtility.FromJson<CatFactResponse>(request.downloadHandler.text);
                onSuccess?.Invoke(response.fact);
            }
            else onError?.Invoke(request.error);
        }
        #endregion




        #region Response types
        [Serializable]
        private class CatFactResponse
        {
            public string fact;
            public int length;
        }
        #endregion
    }
}

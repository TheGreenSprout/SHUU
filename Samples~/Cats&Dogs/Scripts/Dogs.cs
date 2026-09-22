using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

using SHUU.Utils.Globals;

namespace SHUU.Samples.CatsDogs
{
    public static class Dogs
    {
        #region Dog Images
        public static void GetDogImage(Action<Texture2D> onSuccess, Action<string> onError = null)
            => SHUU_Time.StartCoroutineStatic(GetDogImageEnumerator(onSuccess, onError));
        public static IEnumerator GetDogImageEnumerator(Action<Texture2D> onSuccess, Action<string> onError = null)
        {
            using UnityWebRequest jsonRequest = UnityWebRequest.Get("https://dog.ceo/api/breeds/image/random");
            yield return jsonRequest.SendWebRequest();

            if (jsonRequest.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(jsonRequest.error);
                yield break;
            }

            DogCeoResponse response = JsonUtility.FromJson<DogCeoResponse>(jsonRequest.downloadHandler.text);

            using UnityWebRequest imgRequest = UnityWebRequestTexture.GetTexture(response.message);
            yield return imgRequest.SendWebRequest();

            if (imgRequest.result == UnityWebRequest.Result.Success) onSuccess?.Invoke(DownloadHandlerTexture.GetContent(imgRequest));
            else onError?.Invoke(imgRequest.error);
        }

        public static void GetDogGif(Action<byte[]> onSuccess, Action<string> onError = null)
            => SHUU_Time.StartCoroutineStatic(GetDogGifEnumerator(onSuccess, onError));
        public static IEnumerator GetDogGifEnumerator(Action<byte[]> onSuccess, Action<string> onError = null)
        {
            const int maxAttempts = 5;

            for (int i = 0; i < maxAttempts; i++)
            {
                using UnityWebRequest jsonRequest = UnityWebRequest.Get("https://random.dog/woof.json");
                yield return jsonRequest.SendWebRequest();

                if (jsonRequest.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(jsonRequest.error);
                    yield break;
                }

                RandomDogResponse response = JsonUtility.FromJson<RandomDogResponse>(jsonRequest.downloadHandler.text);
                if (!response.url.EndsWith(".gif")) continue;

                using UnityWebRequest gifRequest = UnityWebRequest.Get(response.url);
                yield return gifRequest.SendWebRequest();

                if (gifRequest.result == UnityWebRequest.Result.Success) onSuccess?.Invoke(gifRequest.downloadHandler.data);
                else onError?.Invoke(gifRequest.error);

                yield break;
            }

            onError?.Invoke($"Could not find a GIF after {maxAttempts} attempts.");
        }
        #endregion



        #region Dog Facts
        public static void GetDogFact(Action<string> onSuccess, Action<string> onError = null)
            => SHUU_Time.StartCoroutineStatic(GetDogFactEnumerator(onSuccess, onError));
        public static IEnumerator GetDogFactEnumerator(Action<string> onSuccess, Action<string> onError = null)
        {
            using UnityWebRequest request = UnityWebRequest.Get("https://dogapi.dog/api/v2/facts?limit=1");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                DogFactResponse response = JsonUtility.FromJson<DogFactResponse>(request.downloadHandler.text);
                
                if (response.data != null && response.data.Length > 0) onSuccess?.Invoke(response.data[0].attributes.body);
                else onError?.Invoke("No fact data received.");
            }
            else onError?.Invoke(request.error);
        }
        #endregion




        #region Response types
        [Serializable] private class DogCeoResponse { public string message; }
        [Serializable] private class RandomDogResponse { public string url; }
        [Serializable] private class DogFactResponse { public DogFactData[] data; }
        [Serializable] private class DogFactData { public DogFactAttributes attributes; }
        [Serializable] private class DogFactAttributes { public string body; }
        #endregion
    }
}

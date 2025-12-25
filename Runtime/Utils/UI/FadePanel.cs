using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using SHUU.Utils.Globals;

namespace SHUU.Utils.UI
{

    [RequireComponent(typeof(RawImage))]
    public class FadePanel : MonoBehaviour
    {
        private RawImage fadeImage;


        private Coroutine currentFadeCoroutine = null;



        private FadeManager.FadeOptions fadeOptions;




        private void Awake()
        {
            fadeImage = GetComponent<RawImage>();

            DisableSelf();
        }



        public void NewFade(FadeManager.FadeOptions _fadeOptions)
        {
            if (currentFadeCoroutine != null)
            {
                Debug.LogError("Unable to perform multiple SHUU fades at the same time.");

                return;
            }


            fadeOptions = _fadeOptions;

            fadeImage.color = fadeOptions.startColor.Value;


            if (fadeOptions.clearOnEnd)
            {
               fadeOptions.end_Action += DisableSelf;
                fadeOptions.end_Action += () => currentFadeCoroutine = null; 
            }
            

            EnableSelf();


            SHUU_GlobalsProxy.timerManager.Create(fadeOptions.start_delay, StartFade);
        }

        public void StartFade()
        {
            currentFadeCoroutine = StartCoroutine(FadeCoroutine(fadeOptions.startColor.Value, fadeOptions.endColor.Value, fadeOptions.duration.Value, fadeOptions.end_Action, fadeOptions.end_delay));
        }


        private void EnableSelf() { fadeImage.enabled = true; }
        private void DisableSelf() {  fadeImage.enabled = false; }


        private IEnumerator FadeCoroutine(Color startColor, Color endColor, float time, Action onComplete, float onComplete_delay)
        {
            float elapsed = 0f;

            while (elapsed < time)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / time);

                fadeImage.color = Color.Lerp(startColor, endColor, t);

                yield return null;
            }

            // Final exact color
            fadeImage.color = endColor;

            // Auto-hide if fully transparent
            if (Mathf.Approximately(endColor.a, 0f))
                fadeImage.enabled = false;


            SHUU_GlobalsProxy.timerManager.Create(onComplete_delay, onComplete);
        }
    }
    
}

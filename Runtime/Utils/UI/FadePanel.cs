using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

using SHUU.Utils.Globals;
using SHUU.UserSide.Commons.InnerWorkings.ScriptableObjects;

namespace SHUU.Utils.UI
{
    [RequireComponent(typeof(RawImage))]
    public class FadePanel : MonoBehaviour
    {
        #region Variables
        private RawImage fadeImage;

        private Coroutine currentFadeCoroutine = null;


        private FadeOptions fadeOptions;



        private static bool debugLogEmission => SHUU_Preferences.instance.ui_debugLogEmission;
        #endregion




        #region Main
        private void Awake()
        {
            fadeImage = GetComponent<RawImage>();

            DisableSelf();
        }


        private void EnableSelf() { fadeImage.enabled = true; }
        private void DisableSelf() {  fadeImage.enabled = false; }


        public void NewFade(FadeOptions _fadeOptions)
        {
            if (currentFadeCoroutine != null)
            {
                if (debugLogEmission) Debug.LogError("Unable to perform multiple SHUU fades at the same time.");

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

            SHUU_Time.Timer(fadeOptions.start_delay, StartFade);
        }

        public void StartFade()
            => currentFadeCoroutine = StartCoroutine(FadeCoroutine(fadeOptions.startColor.Value,
                                                                    fadeOptions.endColor.Value,
                                                                    fadeOptions.duration.Value,
                                                                    fadeOptions.end_Action,
                                                                    fadeOptions.end_delay));
        #endregion



        #region Logic
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

            fadeImage.color = endColor;

            if (Mathf.Approximately(endColor.a, 0f)) fadeImage.enabled = false;


            SHUU_Time.Timer(onComplete_delay, onComplete);
        }
        #endregion
    }
}

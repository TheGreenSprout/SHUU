using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using SHUU.Utils.Helpers;

//! Convert this into a 1 RawImage System, aka make it not be instantiated and deleted but reused

namespace SHUU.Utils.UI
{

    [RequireComponent(typeof(RawImage))]
    #region XML doc
    /// <summary>
    /// Manages screen fading logic.
    /// </summary>
    #endregion
    public class ScreenFader : MonoBehaviour
    {
        private RawImage _fadeImage;


        private Coroutine _currentFadeCoroutine = null;




        private void Awake()
        {
            _fadeImage = GetComponent<RawImage>();
        }



        public void Fade(Color endColor, float duration, Action onComplete = null, float end_delay = 0f)
        {
            onComplete += DeleteSelf;

            if (_currentFadeCoroutine != null)
            {
                StopCoroutine(_currentFadeCoroutine);
            }


            _currentFadeCoroutine = StartCoroutine(FadeCoroutine(endColor, duration, onComplete, end_delay));
        }
        private void DeleteSelf()
        {
            Destroy(this.gameObject);
        }


        private IEnumerator FadeCoroutine(Color endColor, float time, Action onComplete, float onComplete_delay)
        {
            Color startColor = _fadeImage.color;


            float elapsed = 0f;

            // Ensure starting state
            _fadeImage.color = startColor;
            _fadeImage.gameObject.SetActive(true);

            while (elapsed < time)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / time);

                _fadeImage.color = Color.Lerp(startColor, endColor, t);

                yield return null;
            }

            // Final exact color
            _fadeImage.color = endColor;

            // Auto-hide if fully transparent
            if (Mathf.Approximately(endColor.a, 0f))
                _fadeImage.gameObject.SetActive(false);

            _currentFadeCoroutine = null;

            SHUU_Timer.Create(onComplete_delay, onComplete);
        }
    }

}

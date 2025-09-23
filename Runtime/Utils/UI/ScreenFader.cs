using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace SHUU.Utils.SceneManagement
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


    #region XML doc
    /// <summary>
    /// Causes a fade-in.
    /// </summary>
    /// <param name="duration">Duration of the fade-in.</param>
    /// <param name="null">Action to run after the fade-in is over.</param>
    #endregion
    public void FadeIn(float duration, Action onComplete = null)
    {
        if (_currentFadeCoroutine != null)
        {
            StopCoroutine(_currentFadeCoroutine);
        }


        _currentFadeCoroutine = StartCoroutine(FadeCoroutine(1f, 0f, duration, onComplete));
    }

    #region XML doc
    /// <summary>
    /// Causes a fade-out.
    /// </summary>
    /// <param name="duration">Duration of the fade-out.</param>
    /// <param name="null">Action to run after the fade-out is over.</param>
    #endregion
    public void FadeOut(float duration, Action onComplete = null)
    {
        if (_currentFadeCoroutine != null)
        {
            StopCoroutine(_currentFadeCoroutine);
        }


        _currentFadeCoroutine = StartCoroutine(FadeCoroutine(0f, 1f, duration, onComplete));
    }


    #region XML doc
    /// <summary>
    /// Actual fade logic.
    /// </summary>
    /// <param name="startAlpha">Start transparency of the fade.</param>
    /// <param name="endAlpha">End transparency of the fade.</param>
    /// <param name="time">Duration of the fade.</param>
    /// <param name="onComplete">Action to run after the fade is over.</param>
    #endregion
    private IEnumerator FadeCoroutine(float startAlpha, float endAlpha, float time, Action onComplete)
    {
        float elapsed = 0f;
        Color c = _fadeImage.color;
        c.a = startAlpha;
        _fadeImage.color = c;
        _fadeImage.gameObject.SetActive(true); // ensure it’s visible during the fade


        while (elapsed < time)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / time);
            c.a = Mathf.Lerp(startAlpha, endAlpha, t);
            _fadeImage.color = c;
            yield return null;
        }


        // Ensure exact final alpha:
        c.a = endAlpha;
        _fadeImage.color = c;


        // If we ended fully transparent, disable panel to unblock input:
        if (Mathf.Approximately(endAlpha, 0f))
        {
            _fadeImage.gameObject.SetActive(false);
        }

        _currentFadeCoroutine = null;
        onComplete?.Invoke();
    }
}

}

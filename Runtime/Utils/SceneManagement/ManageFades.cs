using System;
using UnityEngine;
using UnityEngine.UI;
using SHUU.Utils.SceneManagement;

namespace SHUU.Utils.UI
{

#region XML doc
/// <summary>
/// Manages fade-in/outs for scene transitions.
/// </summary>
#endregion
public class ManageFades : MonoBehaviour
{
    [SerializeField] private GameObject fadePanel;



    [SerializeField] private float defaultFadeDuration;



    private Color transparentBlack;




    private void Start()
    {
        transparentBlack = new Color(Color.black.r, Color.black.g, Color.black.b, 0f);
    }


    #region XML doc
    /// <summary>
    /// Deletes fade-in object when it's done.
    /// </summary>
    #endregion
    public void DeleteFadeCallback()
    {
        Destroy(gameObject);
    }


    #region XML doc
    /// <summary>
    /// Creates a default fade-in. (Black color)
    /// </summary>
    /// <returns>Returns the fade-in object.</returns>
    #endregion
    public GameObject CreateFadeIn()
    {
        GameObject fadeIn = Instantiate(fadePanel, SHUU_Globals.canvas.gameObject.transform);

        ((RawImage)fadeIn.GetComponent(typeof(RawImage))).color = Color.black;


        return fadeIn;
    }
    #region XML doc
    /// <summary>
    /// Creates a custom fade-in. (Custom color)
    /// </summary>
    /// <param name="starterColor">The color of the fade-in.</param>
    /// <returns>Returns the fade-in object.</returns>
    #endregion
    public GameObject CreateFadeIn(Color starterColor)
    {
        GameObject fadeIn = Instantiate(fadePanel, SHUU_Globals.canvas.gameObject.transform);

        ((RawImage)fadeIn.GetComponent(typeof(RawImage))).color = starterColor;


        return fadeIn;
    }

    #region XML doc
    /// <summary>
    /// Triggers a fade-in with a custom duration.
    /// </summary>
    /// <param name="fadeIn">Fade-in that's being triggered.</param>
    /// <param name="duration">The duration of the fade-in.</param>
    /// <param name="null">Action to run after the fade-in is over.</param>
    #endregion
    public void TriggerFadeIn(GameObject fadeIn, float duration, Action action = null)
    {
        Action callback = () =>
        {
            Destroy(fadeIn);

            if (action != null)
            {
                action();
            }
        };


        ((ScreenFader)fadeIn.GetComponent(typeof(ScreenFader))).FadeIn(duration, callback);
    }
    #region XML doc
    /// <summary>
    /// Triggers a fade-in with a default duration.
    /// </summary>
    /// <param name="fadeIn">Fade-in that's being triggered.</param>
    /// <param name="null">Action to run after the fade-in is over.</param>
    #endregion
    public void TriggerFadeIn(GameObject fadeIn, Action action = null)
    {
        Action callback = () =>
        {
            Destroy(fadeIn);

            if (action != null)
            {
                action();
            }
        };


        ((ScreenFader)fadeIn.GetComponent(typeof(ScreenFader))).FadeIn(defaultFadeDuration, callback);
    }


    #region XML doc
    /// <summary>
    /// Creates and triggers a default fade-out with a custom duration. (Black color)
    /// </summary>
    /// <param name="duration">The duration of the fade-out.</param>
    /// <param name="null">Action to run after the fade-out is over.</param>
    #endregion
    public void CreateFadeOut(float duration, Action action = null)
    {
        GameObject fadeOut = Instantiate(fadePanel, SHUU_Globals.canvas.gameObject.transform);

        ((RawImage)fadeOut.GetComponent(typeof(RawImage))).color = transparentBlack;

        ((ScreenFader)fadeOut.GetComponent(typeof(ScreenFader))).FadeOut(duration, action);
    }
    #region XML doc
    /// <summary>
    /// Creates and triggers a default fade-out with a default duration. (Black color)
    /// </summary>
    /// <param name="null">Action to run after the fade-out is over.</param>
    #endregion
    public void CreateFadeOut(Action action = null)
    {
        GameObject fadeOut = Instantiate(fadePanel, SHUU_Globals.canvas.gameObject.transform);

        ((RawImage)fadeOut.GetComponent(typeof(RawImage))).color = transparentBlack;

        ((ScreenFader)fadeOut.GetComponent(typeof(ScreenFader))).FadeOut(defaultFadeDuration, action);
    }
    #region XML doc
    /// <summary>
    /// Creates and triggers a custom fade-out with a custom duration. (Custom color)
    /// </summary>
    /// <param name="duration">The duration of the fade-out.</param>
    /// <param name="starterColor">The color of the fade-out.</param>
    /// <param name="null">Action to run after the fade-out is over.</param>
    #endregion
    public void CreateFadeOut(float duration, Color starterColor, Action action = null)
    {
        GameObject fadeOut = Instantiate(fadePanel, SHUU_Globals.canvas.gameObject.transform);

        ((RawImage)fadeOut.GetComponent(typeof(RawImage))).color = starterColor;

        ((ScreenFader)fadeOut.GetComponent(typeof(ScreenFader))).FadeOut(duration, action);
    }
    #region XML doc
    /// <summary>
    /// Creates and triggers a custom fade-out with a default duration. (Custom color)
    /// </summary>
    /// <param name="starterColor">The color of the fade-out.</param>
    /// <param name="null">Action to run after the fade-out is over.</param>
    #endregion
    public void CreateFadeOut(Color starterColor, Action action = null)
    {
        GameObject fadeOut = Instantiate(fadePanel, SHUU_Globals.canvas.gameObject.transform);

        ((RawImage)fadeOut.GetComponent(typeof(RawImage))).color = starterColor;

        ((ScreenFader)fadeOut.GetComponent(typeof(ScreenFader))).FadeOut(defaultFadeDuration, action);
    }
}

}

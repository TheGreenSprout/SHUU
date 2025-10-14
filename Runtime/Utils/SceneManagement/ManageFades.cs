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
        /// Creates a custom fade-in. (Custom color)
        /// </summary>
        /// <param name="starterColor">The color of the fade-in.</param>
        /// <returns>Returns the fade-in object.</returns>
        #endregion
        public GameObject CreateFadeIn(Color? starterColor = null)
        {
            if (starterColor == null)
            {
                starterColor = Color.black;
            }
            

            GameObject fadeIn = Instantiate(fadePanel, SHUU_Globals.canvas.gameObject.transform);

            ((RawImage)fadeIn.GetComponent(typeof(RawImage))).color = (Color)starterColor;


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
        public void TriggerFadeIn(GameObject fadeIn, float? duration = null, Action action = null)
        {
            if (duration == null)
            {
                duration = defaultFadeDuration;
            }


            Action callback = () =>
            {
                Destroy(fadeIn);

                if (action != null)
                {
                    action();
                }
            };


            ((ScreenFader)fadeIn.GetComponent(typeof(ScreenFader))).FadeIn((float)duration, callback);
        }
        public void TriggerFadeIn(Color? starterColor = null, float? duration = null, Action action = null)
        {
            if (duration == null)
            {
                duration = defaultFadeDuration;
            }

            GameObject fadeIn = CreateFadeIn(starterColor);


            Action callback = () =>
            {
                Destroy(fadeIn);

                if (action != null)
                {
                    action();
                }
            };


            ((ScreenFader)fadeIn.GetComponent(typeof(ScreenFader))).FadeIn((float)duration, callback);
        }


        #region XML doc
        /// <summary>
        /// Creates and triggers a default fade-out with a custom duration. (Black color)
        /// </summary>
        /// <param name="starterColor">The color of the fade-out.</param>
        /// <param name="duration">The duration of the fade-out.</param>
        /// <param name="null">Action to run after the fade-out is over.</param>
        #endregion
        public void TriggerFadeOut(Color? starterColor = null, float? duration = null, Action action = null)
        {
            if (starterColor == null)
            {
                starterColor = transparentBlack;
            }
            if (duration == null)
            {
                duration = defaultFadeDuration;
            }


            GameObject fadeOut = Instantiate(fadePanel, SHUU_Globals.canvas.gameObject.transform);

            ((RawImage)fadeOut.GetComponent(typeof(RawImage))).color = (Color)starterColor;

            ((ScreenFader)fadeOut.GetComponent(typeof(ScreenFader))).FadeOut((float)duration, action);
        }
    }

}

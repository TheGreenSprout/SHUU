using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
#endif

using SHUU.Utils.SceneManagement;

namespace SHUU.UserSide.Addons.LoadingScreen
{
    public class Sample_LoadingScreen : LoadingScreenBase
    {
        #region Variables
        [SerializeField] private Slider progressBar;
        [SerializeField] private TMP_Text progressText;

        [SerializeField] private GameObject finishedText;


#if ENABLE_INPUT_SYSTEM
        private bool anyButtonPressed;
        private IDisposable anyButtonListener;
#endif

        #endregion




        #region Main
        protected override void Awake()
        {
            base.Awake();

            finishedText.SetActive(false);
        }

#if ENABLE_INPUT_SYSTEM
        private void OnDestroy() => anyButtonListener?.Dispose();
#endif

        #endregion



        #region Override Points
        protected override void UpdateProgress(float progress)
        {
            if (progressBar != null) progressBar.value = progress;
            if (progressText != null) progressText.text = $"Loading… {(int)(progress * 100f)}%";
        }


        private bool firstCall = true;
        protected override bool ProgressScene()
        {
            if (firstCall)
            {
                firstCall = false;
                finishedText.SetActive(true);
            }

#if ENABLE_INPUT_SYSTEM
            anyButtonListener ??= InputSystem.onAnyButtonPress.CallOnce(_ => anyButtonPressed = true);
            return anyButtonPressed;
#else
            return Input.anyKeyDown;
#endif
        }
        #endregion
    }
}

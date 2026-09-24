using TMPro;
using UnityEngine;
using UnityEngine.UI;

using SHUU.Utils.SceneManagement;

namespace SHUU.UserSide.Addons.LoadingScreen
{
    public class Sample_LoadingScreen : LoadingScreenBase
    {
        #region Variables
        [SerializeField] private Slider progressBar;
        [SerializeField] private TMP_Text progressText;

        [SerializeField] private GameObject finishedText;
        #endregion




        #region Main
        protected override void Awake()
        {
            base.Awake();

            finishedText.SetActive(false);
        }
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

            if (Input.anyKeyDown) return true;
            return false;
        }
        #endregion
    }
}

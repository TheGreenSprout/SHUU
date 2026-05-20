using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SHUU.Utils.SceneManagement
{
    public class LoadingScreen : MonoBehaviour
    {
        #region Variables
        [SerializeField] private Slider progressBar;

        [SerializeField] private TMP_Text progressText;


        private string sceneToLoad;
        #endregion



        
        #region Main
        private void Awake()
        {
            sceneToLoad = SceneLoader.nextScene;

            SceneLoader.nextScene = null;


            if (sceneToLoad == null || sceneToLoad == "") sceneToLoad = "ErrorScene";


            StartCoroutine(LoadAsyncScene());
        }


        private IEnumerator LoadAsyncScene()
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneToLoad);

            op.allowSceneActivation = false;


            while (!op.isDone)
            {
                float progress = Mathf.Clamp01(op.progress / 0.9f);
                
                if (progressBar != null) progressBar.value = progress;
                if (progressText != null) progressText.text = $"Loading… {(int)(progress * 100f)}%";

                if (op.progress >= 0.9f && ProgressScene()) op.allowSceneActivation = true;


                yield return null;
            }
        }
        #endregion



        #region Override Points
        protected virtual bool ProgressScene() => true;
        #endregion
    }
}
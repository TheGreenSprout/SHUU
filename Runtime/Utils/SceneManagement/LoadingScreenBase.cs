using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SHUU.Utils.SceneManagement
{
    public abstract class LoadingScreenBase : MonoBehaviour
    {
        #region Variables
        protected string sceneToLoad;
        #endregion




        #region Main
        protected virtual void Awake()
        {
            sceneToLoad = SceneLoader.NextScene;
            SceneLoader.NextScene = null;

            if (string.IsNullOrEmpty(sceneToLoad) || !SceneLoader.SceneExists(sceneToLoad)) sceneToLoad = SceneLoader.FallbackSceneName;

            StartCoroutine(LoadAsyncScene());
        }


        private IEnumerator LoadAsyncScene()
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneToLoad);

            op.allowSceneActivation = false;


            while (!op.isDone)
            {
                float progress = Mathf.Clamp01(op.progress / 0.9f);

                UpdateProgress(progress);

                if (op.progress >= 0.9f && ProgressScene()) op.allowSceneActivation = true;


                yield return null;
            }
        }
        #endregion



        #region Override Points
        protected abstract void UpdateProgress(float progress);


        protected abstract bool ProgressScene();
        #endregion
    }
}

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SHUU.Utils.SceneManagement
{

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Slider progressBar;

    [SerializeField] private TMP_Text progressText;


    private string sceneToLoad;




    private void Start()
    {
        sceneToLoad = SceneLoader.nextScene;

        SceneLoader.nextScene = null;


        if (sceneToLoad == null || sceneToLoad == "")
        {
            sceneToLoad = "ErrorScene";
        }


        StartCoroutine(LoadAsyncScene());
    }


    private IEnumerator LoadAsyncScene()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneToLoad);

        op.allowSceneActivation = false;


        while (!op.isDone)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);
            
            
            if (progressBar != null)
            {
                if (progressBar != null) progressBar.value = progress;
            }
            
            if (progressText != null)
            {
                if (progressText != null) progressText.text = $"Loading… {(int)(progress * 100f)}%";
            }
            


            if (op.progress >= 0.9f)
            {
                // Optionally perform something before moving scenes, like "Press any button" or something like that.
                op.allowSceneActivation = true;
            }


            yield return null;
        }
    }
}

}
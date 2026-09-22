using UnityEngine;
using UnityEngine.UI;

using SHUU.Utils.Helpers;

using static SHUU.Utils.Helpers.HandyFunctions;

namespace SHUU.Samples.CatsDogs
{
    public class CatsDogs_Manager : MonoBehaviour
    {
        #region Variables
        // External
        [SerializeField] private bool changeCursorVisivility = true;

        [SerializeField] private CodeListener codeListener;

        
        [Header("Cats")]
        [SerializeField] private GameObject catParent;

        [SerializeField] private Image catImage;
        [SerializeField] private Button catImageButton;

        [SerializeField] private GradualRotation catIconRotation;


        [Header("Dogs")]
        [SerializeField] private GameObject dogParent;

        [SerializeField] private Image dogImage;
        [SerializeField] private Button dogImageButton;

        [SerializeField] private GradualRotation dogIconRotation;



        // Internal
        private enum Mode
        {
            None,
            Cat,
            Dog
        }

        private Mode mode = Mode.None;



        private void Reset() => codeListener = GetComponent<CodeListener>();
        #endregion




        #region Main
        public void CatCode_Open()
        {
            if (changeCursorVisivility) ChangeMouseVisibility_Temporary(true);

            codeListener.enabled = false;
            catParent.SetActive(true);

            mode = Mode.Cat;
        }
        public void CatCode_Close()
        {
            if (changeCursorVisivility) ReturnMouseVisibility_FromTemporary();

            catParent.SetActive(false);
            codeListener.enabled = true;
        }

        public void CatImage()
        {
            if (catImage.gameObject.activeInHierarchy) catImage.gameObject.transform.parent.gameObject.SetActive(false);
            catImageButton.interactable = false;
            catIconRotation.enabled = true;

            Cats.GetCatImage(CatImage_Callback);
        }
        public void CatImage_Callback(Texture2D image)
        {
            catImage.gameObject.transform.parent.gameObject.SetActive(true);
            catImageButton.interactable = true;
            

            var sprite = image.ToSprite();

            catImage.sprite = sprite;

            AspectRatioFitter fitter = catImage.GetComponent<AspectRatioFitter>();
            fitter.aspectRatio = (float)sprite.rect.width / sprite.rect.height;
        }


        public void DogCode_Open()
        {
            if (changeCursorVisivility) ChangeMouseVisibility_Temporary(true);

            codeListener.enabled = false;
            dogParent.SetActive(true);

            mode = Mode.Dog;
        }
        public void DogCode_Close()
        {
            if (changeCursorVisivility) ReturnMouseVisibility_FromTemporary();

            dogParent.SetActive(false);
            codeListener.enabled = true;
        }

        public void DogImage()
        {
            if (dogImage.gameObject.activeInHierarchy) dogImage.gameObject.transform.parent.gameObject.SetActive(false);
            dogImageButton.interactable = false;
            dogIconRotation.enabled = true;

            Dogs.GetDogImage(DogImage_Callback);
        }
        public void DogImage_Callback(Texture2D image)
        {
            dogImage.gameObject.transform.parent.gameObject.SetActive(true);
            dogImageButton.interactable = true;


            var sprite = image.ToSprite();

            dogImage.sprite = sprite;

            AspectRatioFitter fitter = dogImage.GetComponent<AspectRatioFitter>();
            fitter.aspectRatio = (float)sprite.rect.width / sprite.rect.height;
        }


        private void Update()
        {
            if (mode == Mode.None) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (mode == Mode.Cat) CatCode_Close();
                else if (mode == Mode.Dog) DogCode_Close();

                mode = Mode.None;
            }
        }
        #endregion
    }
}

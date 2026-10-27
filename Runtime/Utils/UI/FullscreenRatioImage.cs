using UnityEngine;
using UnityEngine.UI;

using Alchemy.Inspector;

namespace SHUU.Utils.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform), typeof(Image))]
    public class FullscreenRatioImage : MonoBehaviour
    {
        #region Variables
        public enum Mode { Cover, Contain }



        // Inspector
        [Title("Settings")]
        [SerializeField] private Mode mode = Mode.Cover;
        [SerializeField] private Vector2 alignment = new Vector2(0.5f, 0.5f);



        // Internal
        private RectTransform rectTransform;
        private RectTransform parentRect;

        private Image image;


        private Vector2 lastParentSize;
        private Sprite lastSprite;
        #endregion




        #region Main
        private void OnEnable()
        {
            CacheReferences();
            Refresh(true);
        }


        private void LateUpdate()
        {
            if (image != null && image.sprite != lastSprite) Refresh(true);
        }


        private void OnValidate()
        {
            alignment.x = Mathf.Clamp01(alignment.x);
            alignment.y = Mathf.Clamp01(alignment.y);
            
            CacheReferences();
            Refresh(true);
        }


        private void OnTransformParentChanged()
        {
            CacheReferences();
            Refresh(true);
        }

        private void OnRectTransformDimensionsChange()
        {
            if (isActiveAndEnabled) Refresh(false);
        }
        #endregion


        #region Internal
        private void CacheReferences()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            if (image == null) image = GetComponent<Image>();

            parentRect = rectTransform.parent as RectTransform;
        }


        private void Refresh(bool force)
        {
            if (parentRect == null || image == null || image.sprite == null) return;

            Vector2 parentSize = parentRect.rect.size;
            Sprite sprite = image.sprite;

            if (!force && parentSize == lastParentSize && sprite == lastSprite) return;

            float imageWidth = sprite.rect.width;
            float imageHeight = sprite.rect.height;
            if (imageWidth <= 0f || imageHeight <= 0f) return;

            float scaleX = parentSize.x / imageWidth;
            float scaleY = parentSize.y / imageHeight;
            float scale = mode == Mode.Cover ? Mathf.Max(scaleX, scaleY) : Mathf.Min(scaleX, scaleY);

            Vector2 size = new Vector2(imageWidth * scale, imageHeight * scale);

            rectTransform.anchorMin = alignment;
            rectTransform.anchorMax = alignment;
            rectTransform.pivot = alignment;
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = Vector2.zero;

            lastParentSize = parentSize;
            lastSprite = sprite;
        }
        #endregion
    }
}

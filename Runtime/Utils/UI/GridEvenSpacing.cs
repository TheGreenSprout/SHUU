using UnityEngine;
using UnityEngine.UI;

namespace SHUU.Utils.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform), typeof(GridLayoutGroup))]
    public class GridEvenSpacing : MonoBehaviour
    {
        #region Variables
        // Inspector
        [SerializeField] private float horizontalMargin = 20f;



        // Internal
        private GridLayoutGroup grid;
        private RectTransform rectTransform;
        #endregion




        #region Main
        private void OnEnable()
        {
            grid = GetComponent<GridLayoutGroup>();
            rectTransform = GetComponent<RectTransform>();

            UpdateSpacing();
        }


        private void OnValidate() => UpdateSpacing();

        private void OnRectTransformDimensionsChange() => UpdateSpacing();


#if UNITY_EDITOR
        private void Update() => UpdateSpacing();
#endif
        #endregion



        #region Logic
        private void UpdateSpacing()
        {
            if (grid == null || rectTransform == null) return;

            int count = grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount ? grid.constraintCount : transform.childCount;

            if (count < 1) return;


            int margin = Mathf.Max(0, Mathf.RoundToInt(horizontalMargin));

            grid.padding.left = margin;
            grid.padding.right = margin;

            float spacing = 0f;

            if (count > 1)
            {
                float availableWidth = rectTransform.rect.width - margin * 2f - grid.cellSize.x * count;

                spacing = availableWidth / (count - 1);
            }

            grid.spacing = new Vector2(
                Mathf.Max(0f, spacing),
                grid.spacing.y
            );
        }
        #endregion
    }
}
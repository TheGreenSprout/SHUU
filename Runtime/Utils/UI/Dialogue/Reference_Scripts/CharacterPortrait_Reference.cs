using System.Collections.Generic;
using UnityEngine;

namespace SHUU.Utils.UI.Dialogue
{

    [DisallowMultipleComponent]
    public class CharacterPortrait_Reference : MonoBehaviour
    {
        [SerializeField] private const string identifier = "generic_identifier";
        public string IDENTIFIER()
        {
            return identifier;
        }



        private DialogueBox_PortraitHandler portrait_handler;


        private bool active = false;


        private RectTransform parent;

        private int position_index = 0;



        [SerializeField] private AutomaticSpacingStyles automaticSpacingStyle = AutomaticSpacingStyles.Horizontal;




        private void Awake()
        {
            active = false;

            position_index = 0;
        }



        public void Initialize(DialogueBox_PortraitHandler handler, int index)
        {
            portrait_handler = handler;


            position_index = index;
        }


        public virtual void Appear()
        {

        }



        public virtual void StartTalking()
        {

        }

        public virtual void StopTalking()
        {

        }


        public virtual void BeginLine()
        {
            if (active) return;
            active = true;
        }

        public virtual void EndLine()
        {
            if (!active) return;
            active = false;
        }



        #region  Automatic Spacing
        protected virtual void RearrangePosition()
        {
            int count = portrait_handler.allPortraitScripts.Count;

            if (automaticSpacingStyle == AutomaticSpacingStyles.None || parent == null || count <= 0) return;



            int index = position_index;


            automaticSpacingStyle = GetCompatibleSpacingStyle(automaticSpacingStyle, count);


            switch (automaticSpacingStyle)
            {
                case AutomaticSpacingStyles.Horizontal:
                    HandleSingleAxis(true, index, count);
                    break;

                case AutomaticSpacingStyles.Vertical:
                    HandleSingleAxis(false, index, count);
                    break;

                case AutomaticSpacingStyles.Grid_2Row:
                    HandleGrid(index, count, 2);
                    break;

                case AutomaticSpacingStyles.Grid_3Row:
                    HandleGrid(index, count, 3);
                    break;
            }
        }
        // Read and organize
        #region Spacing Styes
        private AutomaticSpacingStyles GetCompatibleSpacingStyle(AutomaticSpacingStyles style, int count)
        {
            switch (style)
            {
                case AutomaticSpacingStyles.Grid_2Row:
                    if (count < 3) return AutomaticSpacingStyles.Horizontal;
                    break;

                case AutomaticSpacingStyles.Grid_3Row:
                    if (count >= 5)
                        return style;
                    else if (count >= 3)
                        return AutomaticSpacingStyles.Grid_2Row;
                    else
                        return AutomaticSpacingStyles.Horizontal;
            }

            return style;
        }

        private void HandleSingleAxis(bool horizontalMode, int index, int count, bool topToBottom = false)
        {
            float[] sizes = new float[count];
            float totalSize = 0f;

            for (int i = 0; i < count; i++)
            {
                RectTransform sibling = parent.GetChild(i).GetComponent<RectTransform>();
                sizes[i] = horizontalMode ? sibling.rect.width : sibling.rect.height;
                totalSize += sizes[i];
            }

            float parentSize = horizontalMode ? parent.rect.width : parent.rect.height;
            float freeSpace = parentSize - totalSize;
            float spacing = freeSpace / (count + 1);

            float pos;

            if (horizontalMode)
            {
                // left-to-right, same as before
                pos = -parentSize / 2f + spacing;
                for (int i = 0; i < index - 1; i++)
                    pos += sizes[i] + spacing;
                pos += sizes[index - 1] / 2f;
                transform.localPosition = new Vector3(pos, 0f, 0f);
            }
            else
            {
                // top-to-bottom vertical
                pos = parentSize / 2f - spacing; // start at top
                for (int i = 0; i < index - 1; i++)
                    pos -= sizes[i] + spacing;   // subtract previous sizes + spacing
                pos -= sizes[index - 1] / 2f;     // pivot correction
                transform.localPosition = new Vector3(0f, pos, 0f);
            }
        }
        private void HandleGrid(int index, int count, int numRows)
        {
            int itemsPerRow = Mathf.CeilToInt((float)count / numRows);
            int rowIndex = (index - 1) / itemsPerRow;      // 0 = top row
            int columnIndex = (index - 1) % itemsPerRow;   // left-to-right within row

            int itemsInThisRow = (rowIndex == numRows - 1) ? (count - itemsPerRow * (numRows - 1)) : itemsPerRow;

            // Horizontal layout for this row
            float[] widths = new float[itemsInThisRow];
            float totalWidth = 0f;

            for (int i = 0; i < itemsInThisRow; i++)
            {
                int childIndex = rowIndex * itemsPerRow + i;
                RectTransform sibling = parent.GetChild(childIndex).GetComponent<RectTransform>();
                widths[i] = sibling.rect.width;
                totalWidth += widths[i];
            }

            float parentWidth = parent.rect.width;
            float freeSpace = parentWidth - totalWidth;
            float spacing = freeSpace / (itemsInThisRow + 1);

            float x = -parentWidth / 2f + spacing;
            for (int i = 0; i < columnIndex; i++)
                x += widths[i] + spacing;
            x += widths[columnIndex] / 2f;

            // Vertical offset (top-to-bottom)
            float rowHeight = parent.rect.height / numRows;
            float y = parent.rect.height / 2f - rowHeight / 2f - rowIndex * rowHeight;

            transform.localPosition = new Vector3(x, y, 0f);
        }
        #endregion
        #endregion



        public virtual void Delete()
        {
            EndLine();

            Destroy(this.gameObject);
        }
    }

}

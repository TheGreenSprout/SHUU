using UnityEngine;

namespace SHUU.Utils.Helpers.Interaction
{
    public class DynamicCursor : MonoBehaviour
    {
        protected bool cursorActive => DynamicCursorInteraction.cursorActive;




        protected virtual void OnEnable() => DynamicCursorInteraction.alternateCursorState += AlternateCursorState;

        protected virtual void OnDisable() => DynamicCursorInteraction.alternateCursorState -= AlternateCursorState;


        protected virtual void AlternateCursorState(bool active) { }
    }
}

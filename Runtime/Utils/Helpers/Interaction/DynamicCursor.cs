using UnityEngine;

namespace SHUU.Utils.Helpers.Interaction
{
    public class DynamicCursor : MonoBehaviour
    {
        #region Variables
        protected bool cursorActive => DynamicCursorInteraction.cursorActive;
        #endregion




        #region Main
        protected virtual void OnEnable() => DynamicCursorInteraction.alternateCursorState += AlternateCursorState;

        protected virtual void OnDisable() => DynamicCursorInteraction.alternateCursorState -= AlternateCursorState;
        #endregion



        #region Override points
        protected virtual void AlternateCursorState(bool active, GameObject affector) { }
        #endregion
    }
}

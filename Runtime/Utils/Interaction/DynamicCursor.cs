using UnityEngine;

namespace SHUU.Utils.Interaction
{
    public class DynamicCursor : MonoBehaviour
    {
        #region Variables
        protected bool cursorActive => DynamicCursorInteraction.CursorActive;
        #endregion




        #region Main
        protected virtual void OnEnable() => DynamicCursorInteraction.AlternateCursorState += AlternateCursorState;

        protected virtual void OnDisable() => DynamicCursorInteraction.AlternateCursorState -= AlternateCursorState;
        #endregion



        #region Override points
        protected virtual void AlternateCursorState(bool active, GameObject affector) { }
        #endregion
    }
}

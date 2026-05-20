#if UNITY_EDITOR
using UnityEditor;

using SHUU.Utils.Helpers;

using SETB.SuperClasses;
using static SETB.EditorGUI_Base;

namespace SHUU._Editor.Drawers
{
    [CustomEditor(typeof(SplineCollider))]
    public class SplineCollider_Drawer : Editor_Base<SplineCollider_Drawer>
    {
        #region Main
        protected override void DrawInspector()
        {
            base.DrawInspector();


            SplineCollider sc = (SplineCollider)target;


            Space(6f);
            
            DrawLabel("Utilities", EditorStyles.boldLabel);

            DrawButton("Rebuild Collider Mesh", () => {
                sc.Rebuild();
            });
        }
        #endregion
    }
}
#endif

#if UNITY_EDITOR
using SHUU.Utils.Helpers;
using UnityEditor;

using SETB;
using static SETB.EditorGUI_Base;
using static SETB.HandyEditorFunctions;

namespace SHUU._Editor.Drawers
{
    [CustomEditor(typeof(SplineCollider))]
    public class SplineCollider_Drawer : Editor_Base<SplineCollider_Drawer>
    {
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
    }
}
#endif

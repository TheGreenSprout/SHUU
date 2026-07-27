#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;

using SHUU.Utils.Helpers;

using SETB.SuperClasses;
using static SETB.EditorGUI_Base;

namespace SHUU._Editor
{
    [CustomPropertyDrawer(typeof(TagMask), true)]
    public class TagMask_Drawer : PropertyDrawer_Base<TagMask_Drawer>
    {
        #region Main
        protected override void Build(SerializedProperty property)
        {
            if (!isDrawing) return;


            SerializedProperty maskProp = PropRelative(property, "mask");

            string[] tags = InternalEditorUtility.tags;

            int value = maskProp.intValue;
            DrawInputMask(property.displayName, ref value, tags);
            maskProp.intValue = value;
        }
        #endregion
    }
}
#endif

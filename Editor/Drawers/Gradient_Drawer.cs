#if UNITY_EDITOR
using System;
using SHUU.Utils.UI;
using UnityEditor;

using SETB;
using static SETB.HandyEditorFunctions;

namespace SHUU._Editor.Drawers
{
    [CustomPropertyDrawer(typeof(SHUU_Gradient.GradientType), true)]
    public class Gradient_Drawer : PropertyDrawer_Base<Gradient_Drawer>
    {
        #region Types
        private static readonly string[] typeNames = new[]
        {
            "None",
            "UI Gradient",
            "UI Corners Gradient",
            "UI Text Gradient",
            "UI Text Corners Gradient"
        };

        private static readonly Type[] types = new Type[]
        {
            null,
            typeof(SHUU_Gradient.UIGradient),
            typeof(SHUU_Gradient.UICornersGradient),
            typeof(SHUU_Gradient.UITextGradient),
            typeof(SHUU_Gradient.UITextCornersGradient)
        };
        #endregion




        protected override void Build(SerializedProperty property)
            => DrawManagedReferenceDropdown(property, "Gradient Type", types, typeNames);
    }
}
#endif
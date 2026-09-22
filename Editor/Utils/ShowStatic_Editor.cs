#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

using SETB.SuperClasses;

using static SETB.EditorGUI_Base;

namespace SHUU._Editor.Utils
{
    [CustomEditor(typeof(MonoBehaviour), true)]
    [CanEditMultipleObjects]
    public class ShowStatic_Editor : AlchemyEditor_Base<ShowStatic_Editor>
    {
        #region Main
        protected override void DrawInspector()
        {
            bool drawnHeader = false;

            var t = target.GetType();
            while (t != null && t != typeof(MonoBehaviour))
            {
                foreach (var field in t.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (!field.IsDefined(typeof(ShowStaticAttribute), false) || field.IsLiteral) continue;

                    if (!drawnHeader)
                    {
                        Space(4);

                        DrawLabel("Static Fields", EditorStyles.boldLabel);

                        drawnHeader = true;
                    }

                    DrawStaticField(field);
                }

                t = t.BaseType;
            }
        }
        #endregion




        #region Logic
        public static void DrawStaticField(FieldInfo field)
        {
            bool readOnly = field.IsInitOnly;
            object newValue = null;
            bool changed = ChangeCheck(() =>
            {
                using (new EditorGUI.DisabledScope(readOnly))
                    newValue = DrawField(field.FieldType, ObjectNames.NicifyVariableName(field.Name), field.GetValue(null));
            });
            if (changed && !readOnly) field.SetValue(null, newValue);
        }

        private static object DrawField(Type t, string label, object value)
        {
            if (t == typeof(bool)) return DrawToggle(label, value is bool b && b);
            if (t == typeof(int)) return DrawInputInt(label, value is int i ? i : 0);
            if (t == typeof(long)) return DrawInputLong(label, value is long l ? l : 0L);
            if (t == typeof(float)) return DrawInputFloat(label, value is float f ? f : 0f);
            if (t == typeof(double)) return DrawInputDouble(label, value is double d ? d : 0d);
            if (t == typeof(string)) return DrawInputString(label, value as string ?? "");
            if (t == typeof(Vector2)) return DrawInputVector2(label, value is Vector2 v2 ? v2 : default);
            if (t == typeof(Vector3)) return DrawInputVector3(label, value is Vector3 v3 ? v3 : default);
            if (t == typeof(Vector4)) return DrawInputVector4(label, value is Vector4 v4 ? v4 : default);
            if (t == typeof(Vector2Int)) return DrawInputVector2Int(label, value is Vector2Int v2i ? v2i : default);
            if (t == typeof(Vector3Int)) return DrawInputVector3Int(label, value is Vector3Int v3i ? v3i : default);
            if (t == typeof(Color)) return DrawInputColor(label, value is Color c ? c : Color.white);
            if (t == typeof(Color32)) return (Color32)DrawInputColor(label, value is Color32 c32 ? (Color)c32 : Color.white);
            if (t == typeof(Rect)) return DrawInputRect(label, value is Rect r ? r : default);
            if (t == typeof(RectInt)) return DrawInputRectInt(label, value is RectInt ri ? ri : default);
            if (t == typeof(Bounds)) return DrawInputBounds(label, value is Bounds bo ? bo : default);
            if (t == typeof(BoundsInt)) return DrawInputBoundsInt(label, value is BoundsInt bi ? bi : default);
            if (t == typeof(Quaternion))
            {
                var q = value is Quaternion qt ? qt : Quaternion.identity;
                return Quaternion.Euler(DrawInputVector3(label + " (Euler)", q.eulerAngles));
            }
            if (t == typeof(AnimationCurve)) return DrawInputCurve(label, value as AnimationCurve ?? new AnimationCurve());
            if (t == typeof(Gradient)) return DrawInputGradient(label, value as Gradient ?? new Gradient());
            if (t == typeof(LayerMask)) return (LayerMask)DrawInputInt(label, value is LayerMask lm ? (int)lm : 0, layerOrMask: true);
            if (t.IsEnum) return EditorGUILayout.EnumPopup(label, value is Enum e ? e : (Enum)Enum.GetValues(t).GetValue(0));
            if (typeof(UnityEngine.Object).IsAssignableFrom(t)) return EditorGUILayout.ObjectField(label, value as UnityEngine.Object, t, true);

            using (new EditorGUI.DisabledScope(true))
                DrawLabel(label, value?.ToString() ?? "null");

            return value;
        }
        #endregion
    }
}
#endif

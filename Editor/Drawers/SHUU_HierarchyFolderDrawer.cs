#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using SHUU.Utils;

namespace SHUU._Editor.Drawers
{
    [InitializeOnLoad]
    public static class HierarchyFolderDrawer
    {
        #region Variables
        private static readonly Dictionary<int, SHUU_HierarchyFolder> Cache = new();


        private static Texture2D IconFolder;
        private static Texture2D GradientTex;
        #endregion




        #region Main
        static HierarchyFolderDrawer()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;

            EditorApplication.hierarchyChanged += () => Cache.Clear();
        }


        private static void OnHierarchyGUI(int instanceID, Rect position) => Logic(instanceID, position);
        #endregion



        #region Logic
        /*
        ⚠️‼️ AI ASSISTED SNIPPET

        This code snippet was written with the assistance of AI.
        */
        private static void Logic(int instanceID, Rect position)
        {
            if (Event.current.type != EventType.Repaint) return;


            if (!Cache.TryGetValue(instanceID, out SHUU_HierarchyFolder component))
            {

#if UNITY_6000_3_OR_NEWER
                GameObject go = EditorUtility.EntityIdToObject(instanceID) as GameObject;
#else
                GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
#endif

                component = go != null ? go.GetComponent<SHUU_HierarchyFolder>() : null;
                Cache[instanceID] = component;
            }
            if (component == null) return;


            EnsureIcons();


            const int pixelHeight = 16;

#if UNITY_6000_3_OR_NEWER
            bool isSelected = Selection.entityIds.Contains(instanceID);
#else
            bool isSelected = Selection.instanceIDs.Contains(instanceID);
#endif

            bool isActive = component.isActiveAndEnabled;
            Color defaultContentColor = GUI.contentColor;

            bool isPrefabRoot = component.prefabTint switch
            {
                SHUU_HierarchyFolder.PrefabTintMode.Root => PrefabUtility.IsAnyPrefabInstanceRoot(component.gameObject),
                SHUU_HierarchyFolder.PrefabTintMode.Always => PrefabUtility.IsPartOfPrefabInstance(component.gameObject),
                _ => false
            };

            Color normalTextColor = EditorGUIUtility.isProSkin ? new Color(0.82f, 0.82f, 0.82f) : new Color(0.06f, 0.06f, 0.06f);
            Color textColor;

            if (isActive || isSelected) textColor = component.coloredText ? component.textColor : normalTextColor;
            else
            {
                Color dimBase = component.coloredText ? component.textColor : normalTextColor;
                textColor = new Color(dimBase.r * 0.6f, dimBase.g * 0.6f, dimBase.b * 0.6f, 1f);
            }
            if (isPrefabRoot) textColor = Color.Lerp(textColor, component.prefabTextTint, component.prefabTintStrength);

            Texture2D folderIcon = component.customIcon != null ? component.customIcon : IconFolder;

            // background (always solid)
            Color bgColor = isSelected
                ? (EditorGUIUtility.isProSkin ? new Color(0.173f, 0.365f, 0.529f) : new Color(0.227f, 0.447f, 0.69f))
                : (EditorGUIUtility.isProSkin ? new Color(0.219f, 0.219f, 0.219f) : new Color(0.784f, 0.784f, 0.784f));

            EditorGUI.DrawRect(new Rect(position.xMin, position.yMin, position.width + pixelHeight, position.height), bgColor);

            if (component.coloredHighlight)
            {
                Color hColor = isPrefabRoot ? Color.Lerp(component.highlightColor, component.prefabHighlightTint, component.prefabTintStrength) : component.highlightColor;
                Color prev = GUI.color;
                GUI.color = hColor;
                GUI.DrawTexture(new Rect(position.xMin, position.yMin, position.width + pixelHeight, position.height), component.customGradient != null ? component.customGradient : GradientTex);
                GUI.color = prev;
            }

            int iconPixels = Mathf.RoundToInt(pixelHeight * component.iconSize);
            float iconY = position.yMin + (position.height - iconPixels) * 0.5f;

            if (folderIcon != null)
            {
                Color iconColor = component.coloredIcon ? component.iconColor : defaultContentColor;
                if (isPrefabRoot) iconColor = Color.Lerp(iconColor, component.prefabIconTint, component.prefabTintStrength);
                GUI.contentColor = iconColor;
                EditorGUIUtility.SetIconSize(new Vector2(iconPixels, iconPixels));
                float iconX = position.xMin + component.iconOffset.x;
                float iconYOffset = iconY + component.iconOffset.y;
                EditorGUI.LabelField(new Rect(iconX, iconYOffset, iconPixels, iconPixels), new GUIContent(folderIcon));
                EditorGUIUtility.SetIconSize(Vector2.zero);
                GUI.contentColor = defaultContentColor;
            }

           GUIStyle style = new GUIStyle();
            style.normal = new GUIStyleState { textColor = textColor };
            style.fontStyle = component.textStyle;
            int indentX;
            if (component.textAlignment == SHUU_HierarchyFolder.TextAlignment.Center)
            {
                style.alignment = TextAnchor.MiddleCenter;
                indentX = 0;
            }
            else
            {
                style.alignment = TextAnchor.MiddleLeft;
                indentX = iconPixels + 2;
            }
            EditorGUI.LabelField(new Rect(position.xMin + indentX + component.iconOffset.x, position.yMin + component.iconOffset.y, position.width, position.height), component.gameObject.name, style);
        }


        #region Helpers
        private static void EnsureIcons()
        {
            IconFolder ??= EditorGUIUtility.IconContent("Folder Icon").image as Texture2D;

            if (GradientTex == null)
            {
                GradientTex = new Texture2D(2, 1, TextureFormat.RGBA32, false)
                {
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear
                };
                GradientTex.SetPixels(new[] { Color.white, Color.clear });
                GradientTex.Apply();
            }
        }
        #endregion

        #endregion
    }
}
#endif

/*
⚠️‼️ AI ASSISTED SCRIPT

This script was written with the assistance of AI.
*/



#if UNITY_EDITOR
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using SHUU.Utils;

namespace SHUU._Editor.Drawers
{
    [InitializeOnLoad]
    public static class HierarchyIconDrawer
    {
        #region Variables
        private static readonly Dictionary<int, SHUU_HierarchyIcon> Cache = new();
        private static readonly Dictionary<SHUU_HierarchyIcon.IconType, Texture2D> IconCache = new();

        private static Texture2D GradientTex;
        private static GUIStyle LabelStyle;


        private const float HoverLighten = 0.08f;
        #endregion




        #region Main
        static HierarchyIconDrawer()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;

            EditorApplication.hierarchyChanged += () => Cache.Clear();
        }


        private static void OnHierarchyGUI(int instanceID, Rect position) => Logic(instanceID, position);
        #endregion



        #region Logic
        private static void Logic(int instanceID, Rect position)
        {
            if (Event.current.type != EventType.Repaint) return;


            if (!Cache.TryGetValue(instanceID, out SHUU_HierarchyIcon component))
            {

#if UNITY_6000_3_OR_NEWER
                GameObject go = EditorUtility.EntityIdToObject(instanceID) as GameObject;
#else
                GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
#endif

                component = go != null ? go.GetComponent<SHUU_HierarchyIcon>() : null;
                Cache[instanceID] = component;
            }
            if (component == null) return;


            EnsureResources();


            const int pixelHeight = 16;

#if UNITY_6000_3_OR_NEWER
            bool isSelected = Selection.entityIds.Contains(instanceID);
#else
            bool isSelected = Selection.instanceIDs.Contains(instanceID);
#endif

            bool isActive = component.gameObject.activeInHierarchy;
            Color defaultContentColor = GUI.contentColor;

            bool isPrefabRoot = component.prefabTint switch
            {
                SHUU_HierarchyIcon.PrefabTintMode.Root => PrefabUtility.IsAnyPrefabInstanceRoot(component.gameObject),
                SHUU_HierarchyIcon.PrefabTintMode.Always => PrefabUtility.IsPartOfPrefabInstance(component.gameObject),
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

            Texture2D icon = component.customIcon != null ? component.customIcon : GetIcon(component.icon);

            Color bgColor = isSelected
                ? (EditorGUIUtility.isProSkin ? new Color(0.173f, 0.365f, 0.529f) : new Color(0.227f, 0.447f, 0.69f))
                : (EditorGUIUtility.isProSkin ? new Color(0.219f, 0.219f, 0.219f) : new Color(0.784f, 0.784f, 0.784f));

            bool isHovered = IsMouseOver(position);
            if (isHovered) bgColor = Color.Lerp(bgColor, Color.white, HoverLighten);

            EditorGUI.DrawRect(new Rect(position.xMin, position.yMin, position.width + pixelHeight, position.height), bgColor);

            if (component.coloredHighlight)
            {
                Color hColor = isPrefabRoot ? Color.Lerp(component.highlightColor, component.prefabHighlightTint, component.prefabTintStrength) : component.highlightColor;
                if (isHovered) hColor = Color.Lerp(hColor, Color.white, HoverLighten);
                Color prev = GUI.color;
                GUI.color = hColor;
                GUI.DrawTexture(new Rect(position.xMin, position.yMin, position.width + pixelHeight, position.height), component.customGradient != null ? component.customGradient : GradientTex);
                GUI.color = prev;
            }

            int iconPixels = Mathf.RoundToInt(pixelHeight * component.iconSize);
            float iconY = position.yMin + (position.height - iconPixels) * 0.5f;

            if (icon != null)
            {
                Color iconColor = component.coloredIcon ? component.iconColor : defaultContentColor;
                if (isPrefabRoot) iconColor = Color.Lerp(iconColor, component.prefabIconTint, component.prefabTintStrength);
                if (!isActive && !isSelected) iconColor = new Color(iconColor.r, iconColor.g, iconColor.b, iconColor.a * 0.5f);
                GUI.contentColor = iconColor;
                EditorGUIUtility.SetIconSize(new Vector2(iconPixels, iconPixels));
                float iconX = position.xMin + component.iconOffset.x;
                float iconYOffset = iconY + component.iconOffset.y;
                EditorGUI.LabelField(new Rect(iconX, iconYOffset, iconPixels, iconPixels), new GUIContent(icon));
                EditorGUIUtility.SetIconSize(Vector2.zero);
                GUI.contentColor = defaultContentColor;
            }

            LabelStyle ??= new GUIStyle();
            LabelStyle.normal = new GUIStyleState { textColor = textColor };
            LabelStyle.fontStyle = component.textStyle;
            int indentX;
            if (component.textAlignment == SHUU_HierarchyIcon.TextAlignment.Center)
            {
                LabelStyle.alignment = TextAnchor.MiddleCenter;
                indentX = 0;
            }
            else
            {
                LabelStyle.alignment = TextAnchor.MiddleLeft;
                indentX = iconPixels + 2;
            }
            EditorGUI.LabelField(new Rect(position.xMin + indentX + component.iconOffset.x, position.yMin + component.iconOffset.y, position.width, position.height), component.gameObject.name, LabelStyle);
        }


        #region Helpers
        private static bool IsMouseOver(Rect position)
        {
            EditorWindow hovered = EditorWindow.mouseOverWindow;
            if (hovered == null || hovered.GetType().Name != "SceneHierarchyWindow") return false;

            return position.Contains(Event.current.mousePosition);
        }


        private static void EnsureResources()
        {
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


        private static Texture2D GetIcon(SHUU_HierarchyIcon.IconType type)
        {
            if (IconCache.TryGetValue(type, out Texture2D cached) && cached != null) return cached;


            Texture2D tex = EditorGUIUtility.IconContent(IconName(type)).image as Texture2D;

            if (tex == null && type != SHUU_HierarchyIcon.IconType.GameObject) tex = GetIcon(SHUU_HierarchyIcon.IconType.GameObject);

            IconCache[type] = tex;

            return tex;
        }


        private static string IconName(SHUU_HierarchyIcon.IconType type) => type switch
        {
            SHUU_HierarchyIcon.IconType.Prefab => "Prefab Icon",
            SHUU_HierarchyIcon.IconType.Camera => "Camera Icon",
            SHUU_HierarchyIcon.IconType.Light => "Light Icon",
            SHUU_HierarchyIcon.IconType.Audio => "AudioSource Icon",
            SHUU_HierarchyIcon.IconType.Particles => "ParticleSystem Icon",
            SHUU_HierarchyIcon.IconType.Canvas => "Canvas Icon",
            SHUU_HierarchyIcon.IconType.Physics => "Rigidbody Icon",
            SHUU_HierarchyIcon.IconType.Script => "cs Script Icon",
            _ => "GameObject Icon"
        };
        #endregion

        #endregion
    }
}
#endif

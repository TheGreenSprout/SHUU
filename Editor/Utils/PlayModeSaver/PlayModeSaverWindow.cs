/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using SHUU.InnerWorkings.Preferences;
using UnityEditor;
using UnityEngine;

namespace SHUU._Editor.Utils.PlayModeSaver
{
    internal class PlayModeSaverWindow : EditorWindow
    {
        #region Variables
        private const float RowHeight = 20f;

        private Vector2 scroll;

        private static GUIStyle rowStyle;
        private static GUIStyle gameObjectStyle;
        #endregion




        #region Main
        public static void Open()
        {
            PlayModeSaverWindow window = GetWindow<PlayModeSaverWindow>("Play Mode Saver");

            window.minSize = new Vector2(260f, 140f);
            window.Show();
        }


        private void OnEnable()
        {
            PlayModeComponentSaver.FlagsChanged += Repaint;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            PlayModeComponentSaver.FlagsChanged -= Repaint;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state) => Repaint();

        private void OnInspectorUpdate()
        {
            if (EditorApplication.isPlaying) Repaint();
        }


        private void OnGUI()
        {
            if (SHUUPreferences_EditorHierarchy.Instance == null || !SHUUPreferences_EditorHierarchy.Instance.sceneSwitcher_enabled)
            {
                EditorGUILayout.HelpBox("The Play Mode saver is turned off. You can turn it on in Preferences > SHUU > Editor > Inspector.", MessageType.Info);

                return;
            }

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode, then flag what you want to keep: the save icon in a component's header, or right-click GameObjects in the Hierarchy > Play Mode Saver. What you flag is listed here.", MessageType.Info);

                return;
            }


            List<Component> flagged = PlayModeComponentSaver.GetFlagged();
            List<GameObject> gameObjects = GroupByGameObject(flagged, out Dictionary<GameObject, List<Component>> byGameObject);

            DrawToolbar(flagged.Count, gameObjects.Count);

            if (flagged.Count == 0)
            {
                EditorGUILayout.HelpBox("Nothing is flagged yet.", MessageType.None);

                return;
            }


            if (rowStyle == null)
            {
                rowStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft, clipping = TextClipping.Clip };
                gameObjectStyle = new GUIStyle(rowStyle) { fontStyle = FontStyle.Bold };
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);

            foreach (GameObject gameObject in gameObjects) DrawGameObject(gameObject, byGameObject[gameObject]);

            EditorGUILayout.EndScrollView();
        }
        #endregion



        #region GUI
        private static void DrawToolbar(int components, int gameObjects)
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label($"{components} component(s) on {gameObjects} GameObject(s)", EditorStyles.miniLabel);

                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledScope(components == 0))
                    if (GUILayout.Button("Stop keeping all", EditorStyles.toolbarButton)) PlayModeComponentSaver.ClearFlags();
            }
        }


        private static void DrawGameObject(GameObject gameObject, List<Component> components)
        {
            Rect row = GUILayoutUtility.GetRect(0f, RowHeight, GUILayout.ExpandWidth(true));
            Rect remove = new Rect(row.xMax - RowHeight, row.y, RowHeight, row.height);
            Rect label = new Rect(row.x + 4f, row.y, row.width - RowHeight - 4f, row.height);

            EditorGUI.DrawRect(row, new Color(0f, 0f, 0f, 0.12f));

            string path = PathOf(gameObject.transform);

            if (GUI.Button(label, new GUIContent(path, path), gameObjectStyle))
            {
                Selection.activeGameObject = gameObject;

                EditorGUIUtility.PingObject(gameObject);
            }

            if (GUI.Button(remove, new GUIContent("x", "Stop keeping the components of this GameObject"), EditorStyles.miniButton))
                PlayModeComponentSaver.SetFlagged(components, false);


            foreach (Component component in components)
            {
                Rect componentRow = GUILayoutUtility.GetRect(0f, RowHeight, GUILayout.ExpandWidth(true));
                Rect componentRemove = new Rect(componentRow.xMax - RowHeight, componentRow.y, RowHeight, componentRow.height);

                GUIContent content = EditorGUIUtility.ObjectContent(component, component.GetType());
                Rect icon = new Rect(componentRow.x + 18f, componentRow.y + 2f, 16f, 16f);

                if (content.image != null) GUI.DrawTexture(icon, content.image, ScaleMode.ScaleToFit);

                GUI.Label(new Rect(componentRow.x + 40f, componentRow.y, componentRow.width - 40f - RowHeight, componentRow.height), component.GetType().Name, rowStyle);

                if (GUI.Button(componentRemove, new GUIContent("x", "Stop keeping this component"), EditorStyles.miniButton))
                    PlayModeComponentSaver.SetFlagged(new[] { component }, false);
            }
        }
        #endregion



        #region Logic
        private static List<GameObject> GroupByGameObject(List<Component> flagged, out Dictionary<GameObject, List<Component>> byGameObject)
        {
            byGameObject = new Dictionary<GameObject, List<Component>>();

            foreach (Component component in flagged)
            {
                if (component == null) continue;

                if (!byGameObject.TryGetValue(component.gameObject, out List<Component> list)) byGameObject[component.gameObject] = list = new List<Component>();

                list.Add(component);
            }

            foreach (KeyValuePair<GameObject, List<Component>> pair in byGameObject)
            {
                Component[] onObject = pair.Key.GetComponents<Component>();

                pair.Value.Sort((a, b) => Array.IndexOf(onObject, a).CompareTo(Array.IndexOf(onObject, b)));
            }

            List<GameObject> gameObjects = new List<GameObject>(byGameObject.Keys);

            gameObjects.Sort((a, b) => string.CompareOrdinal(SortKey(a.transform), SortKey(b.transform)));

            return gameObjects;
        }

        private static string SortKey(Transform transform)
        {
            string key = "";

            for (Transform current = transform; current != null; current = current.parent) key = current.GetSiblingIndex().ToString("D6") + "/" + key;

            return transform.gameObject.scene.name + "|" + key;
        }

        private static string PathOf(Transform transform)
        {
            string path = transform.name;

            for (Transform parent = transform.parent; parent != null; parent = parent.parent) path = parent.name + "/" + path;

            return path;
        }
        #endregion
    }
}
#endif

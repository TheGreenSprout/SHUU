#if UNITY_EDITOR
using SHUU.InnerWorkings.Preferences;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SHUU._Editor.Utils.SceneSwitcher
{
    [InitializeOnLoad]
    public static class HierarchySceneSwitcher
    {
        #region Variables
        private const float LeftInset = 20f;
        private const float RightInset = 24f;
        #endregion




        #region Main
        static HierarchySceneSwitcher()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
        }


        private static void OnHierarchyGUI(int instanceID, Rect rect)
        {
            if (SHUUPreferences_EditorHierarchy.Instance == null || !SHUUPreferences_EditorHierarchy.Instance.sceneSwitcher_enabled) return;

            if (!TryGetSceneHeader(instanceID, out Scene scene)) return;


            Rect clickRect = new Rect(rect.x + LeftInset, rect.y, Mathf.Max(0f, rect.width - LeftInset - RightInset), rect.height);

            EditorGUIUtility.AddCursorRect(clickRect, MouseCursor.Link);


            Event e = Event.current;

            if (e.type != EventType.MouseDown || e.button != 0 || !clickRect.Contains(e.mousePosition)) return;

            e.Use();

            PopupWindow.Show(clickRect, new SceneSwitcherPopup(scene.path, clickRect.width));
        }
        #endregion



        #region Logic
        private static bool TryGetSceneHeader(int instanceID, out Scene scene)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                scene = SceneManager.GetSceneAt(i);

                int headerID = scene.handle;
                if (headerID == instanceID) return true;
            }

            scene = default;

            return false;
        }


        internal static void SwitchTo(string path)
        {
            if (EditorApplication.isPlaying)
            {
                EditorSceneManager.LoadSceneInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Single));

                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        }
        #endregion
    }
}
#endif

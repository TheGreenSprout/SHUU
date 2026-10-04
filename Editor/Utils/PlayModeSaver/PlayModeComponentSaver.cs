/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

using SHUU.InnerWorkings.Preferences;

using Object = UnityEngine.Object;

namespace SHUU._Editor.Utils.PlayModeSaver
{
    [InitializeOnLoad]
    public static class PlayModeComponentSaver
    {
        #region Variables
        private const string PendingKey = "SHUU.PlayModeComponentSaver.Pending";

        private static readonly Color Accent = new Color(0.4f, 0.75f, 1f);


        private static readonly HashSet<Component> Flagged = new HashSet<Component>();

        private static readonly List<Component> Drawing = new List<Component>();

        private static GUIContent[,] contents;


        internal enum FlagState { None, Some, All }

        internal static event Action FlagsChanged;


        [Serializable]
        private class PendingChanges
        {
            public List<ComponentSnapshot> snapshots = new List<ComponentSnapshot>();
            public SaveReport report = new SaveReport();
        }



        private static bool enabled => SHUUPreferences_EditorInspector.Instance ? SHUUPreferences_EditorInspector.Instance.playModeSaver_enabled : false;

        private static bool debugLogEmission => SHUUPreferences_EditorInspector.Instance ? SHUUPreferences_EditorInspector.Instance.playModeSaver_debugLogEmission : false;
        #endregion




        #region Main
        static PlayModeComponentSaver()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            ComponentHeaderItems.Register(HeaderItem);

            EditorApplication.delayCall += ApplyPending;
        }


        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    ClearFlags();
                    break;

                case PlayModeStateChange.ExitingPlayMode:
                    Capture();
                    break;

                case PlayModeStateChange.EnteredEditMode:
                    EditorApplication.delayCall += ApplyPending;
                    break;
            }
        }
        #endregion



        #region Flags
        internal static bool IsActive => EditorApplication.isPlaying && enabled;


        internal static bool IsFlagged(Component component) => component != null && Flagged.Contains(component);

        internal static List<Component> GetFlagged()
        {
            Flagged.RemoveWhere(component => component == null);

            return new List<Component>(Flagged);
        }


        internal static int SetFlagged(IEnumerable<Component> components, bool flagged)
        {
            int changed = 0;

            foreach (Component component in components)
            {
                if (component == null) continue;

                if (flagged ? CanBeKept(component) && Flagged.Add(component) : Flagged.Remove(component)) changed++;
            }

            if (changed > 0) FlagsChanged?.Invoke();

            return changed;
        }

        internal static void ClearFlags()
        {
            if (Flagged.Count == 0) return;

            Flagged.Clear();

            FlagsChanged?.Invoke();
        }


        internal static List<Component> ComponentsOf(IEnumerable<GameObject> gameObjects, bool includeChildren)
        {
            List<Component> components = new List<Component>();

            foreach (GameObject gameObject in gameObjects)
            {
                if (gameObject == null) continue;

                foreach (Component component in includeChildren ? gameObject.GetComponentsInChildren<Component>(true) : gameObject.GetComponents<Component>())
                    if (component != null) components.Add(component);
            }

            return components;
        }

        internal static void ToggleAllOn(IEnumerable<GameObject> gameObjects)
        {
            List<Component> components = ComponentsOf(gameObjects, false);

            SetFlagged(components, StateOf(components) != FlagState.All);
        }


        internal static FlagState StateOf(List<Component> components)
        {
            int keepable = 0;
            int flagged = 0;

            foreach (Component component in components)
            {
                if (!CanBeKept(component)) continue;

                keepable++;

                if (Flagged.Contains(component)) flagged++;
            }

            if (flagged == 0) return FlagState.None;

            return flagged == keepable ? FlagState.All : FlagState.Some;
        }


        internal static bool CanBeKept(Component component)
        {
            if (component == null || EditorUtility.IsPersistent(component)) return false;

            GameObject gameObject = component.gameObject;

            return (gameObject.hideFlags & HideFlags.DontSaveInEditor) == 0 && !string.IsNullOrEmpty(gameObject.scene.path);
        }


        private static List<GameObject> DistinctGameObjects(List<Component> components)
        {
            List<GameObject> gameObjects = new List<GameObject>();

            foreach (Component component in components)
                if (component != null && !gameObjects.Contains(component.gameObject)) gameObjects.Add(component.gameObject);

            return gameObjects;
        }
        #endregion



        #region Header icon
        private static bool HeaderItem(Rect slot, Object[] targets)
        {
            if (!IsActive || targets == null || targets.Length == 0) return false;

            Drawing.Clear();

            foreach (Object target in targets)
                if (target is Component component && CanBeKept(component)) Drawing.Add(component);

            if (Drawing.Count == 0) return false;


            EnsureContent();

            FlagState state = StateOf(Drawing);
            Event e = Event.current;

            bool rightClick = e.type == EventType.ContextClick || (e.type == EventType.MouseDown && e.button == 1);

            if (rightClick && slot.Contains(e.mousePosition))
            {
                ShowOptionsMenu(DistinctGameObjects(Drawing));

                e.Use();
            }

            if (state != FlagState.None) EditorGUI.DrawRect(slot, new Color(Accent.r, Accent.g, Accent.b, state == FlagState.All ? 0.22f : 0.1f));

            Color previous = GUI.contentColor;
            if (state != FlagState.None) GUI.contentColor = state == FlagState.All ? Accent : new Color(Accent.r, Accent.g, Accent.b, 0.65f);

            bool shift = e.shift;
            bool clicked = GUI.Button(slot, contents[(int)state, Drawing.Count > 1 ? 1 : 0], EditorStyles.iconButton);

            GUI.contentColor = previous;


            if (clicked)
            {
                if (shift) ToggleAllOn(DistinctGameObjects(Drawing));
                else SetFlagged(Drawing, state != FlagState.All);
            }

            return true;
        }


        private static void ShowOptionsMenu(List<GameObject> gameObjects)
        {
            string these = gameObjects.Count > 1 ? "these GameObjects" : "this GameObject";

            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent($"Keep all the components of {these}"), false, () => SetFlagged(ComponentsOf(gameObjects, false), true));
            menu.AddItem(new GUIContent($"Keep all the components of {these} and their children"), false, () => SetFlagged(ComponentsOf(gameObjects, true), true));
            menu.AddSeparator("");
            menu.AddItem(new GUIContent($"Stop keeping the components of {these} and their children"), false, () => SetFlagged(ComponentsOf(gameObjects, true), false));
            menu.AddItem(new GUIContent("Stop keeping anything"), false, ClearFlags);
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Show everything that's kept"), false, PlayModeSaverWindow.Open);

            menu.ShowAsContext();
        }


        private static void EnsureContent()
        {
            if (contents != null && contents[0, 0].image != null) return;

            Texture icon = EditorGUIUtility.IconContent("SaveAs").image;

            const string more = "\nShift-click: every component of the GameObject. Right-click: more options.";

            contents = new GUIContent[3, 2];

            contents[(int)FlagState.None, 0] = new GUIContent(icon, "Keep this component's changes when you exit Play Mode." + more);
            contents[(int)FlagState.None, 1] = new GUIContent(icon, "Keep these components' changes when you exit Play Mode." + more);

            contents[(int)FlagState.Some, 0] = new GUIContent(icon, "Some of these are kept when you exit Play Mode. Click to keep them all." + more);
            contents[(int)FlagState.Some, 1] = new GUIContent(icon, "Some of these are kept when you exit Play Mode. Click to keep them all." + more);

            contents[(int)FlagState.All, 0] = new GUIContent(icon, "This component's changes will be kept when you exit Play Mode. Click to stop." + more);
            contents[(int)FlagState.All, 1] = new GUIContent(icon, "These components' changes will be kept when you exit Play Mode. Click to stop." + more);
        }
        #endregion



        #region Saving
        private static void Capture()
        {
            if (!enabled)
            {
                ClearFlags();

                return;
            }


            PendingChanges pending = new PendingChanges();

            foreach (Component component in Flagged)
            {
                if (component == null) continue;

                ComponentSnapshot snapshot = ComponentSnapshot.Capture(component, pending.report, out string problem);

                if (snapshot != null) pending.snapshots.Add(snapshot);
                else Warn($"Can't keep the Play Mode changes of {component.GetType().Name} on '{component.gameObject.name}': {problem}.");
            }

            ClearFlags();

            if (pending.snapshots.Count > 0) SessionState.SetString(PendingKey, JsonUtility.ToJson(pending));
        }


        private static void ApplyPending()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            string json = SessionState.GetString(PendingKey, "");

            if (string.IsNullOrEmpty(json)) return;

            SessionState.EraseString(PendingKey);


            PendingChanges pending = JsonUtility.FromJson<PendingChanges>(json);
            int components = 0;

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Keep Play Mode changes");
            int undoGroup = Undo.GetCurrentGroup();

            foreach (ComponentSnapshot snapshot in pending.snapshots)
            {
                if (!snapshot.TryFind(out Component component, out string problem))
                {
                    Warn($"Couldn't keep the Play Mode changes of {snapshot.Description}: {problem}.");

                    continue;
                }

                snapshot.ApplyTo(component, pending.report);

                EditorSceneManager.MarkSceneDirty(component.gameObject.scene);

                components++;
            }

            Undo.CollapseUndoOperations(undoGroup);

            if (components == 0) return;


            string message = $"Kept the Play Mode changes of {components} component(s): {pending.report.saved} value(s)";

            if (pending.report.sceneReferences > 0) message += $", {pending.report.sceneReferences} of them references to scene objects";

            message += ". Save the scene to keep them.";

            if (pending.report.skippedReferences > 0) message += $" {pending.report.skippedReferences} reference(s) to objects that only existed while playing (or are in another scene) were left as they were.";
            if (pending.report.unsupported > 0) message += $" {pending.report.unsupported} value(s) of a type that can't be kept were skipped.";
            if (pending.report.failed > 0) message += $" {pending.report.failed} value(s) couldn't be restored.";

            Log(message);
        }
        #endregion



        #region Logging
        internal static void Log(string message)
        {
            if (debugLogEmission) Debug.Log($"[SHUU] {message}");
        }

        internal static void Warn(string message)
        {
            if (debugLogEmission) Debug.LogWarning($"[SHUU] {message}");
        }
        #endregion
    }
}
#endif

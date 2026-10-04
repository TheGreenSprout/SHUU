#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace SHUU._Editor.Utils.PlayModeSaver
{
    internal static class PlayModeSaverMenu
    {
        #region Variables
        private const string Root = "GameObject/Play Mode Saver/";
        private const int Priority = 70;
        #endregion




        #region Menu
        [MenuItem(Root + "Keep All Components", false, Priority)]
        private static void KeepAll() => PlayModeComponentSaver.SetFlagged(PlayModeComponentSaver.ComponentsOf(Selected(), false), true);

        [MenuItem(Root + "Keep All Components", true)]
        private static bool KeepAllValidate() => CanUse();


        [MenuItem(Root + "Keep All Components (With Children)", false, Priority + 1)]
        private static void KeepAllWithChildren() => PlayModeComponentSaver.SetFlagged(PlayModeComponentSaver.ComponentsOf(Selected(), true), true);

        [MenuItem(Root + "Keep All Components (With Children)", true)]
        private static bool KeepAllWithChildrenValidate() => CanUse();


        [MenuItem(Root + "Stop Keeping Components (With Children)", false, Priority + 2)]
        private static void StopKeeping() => PlayModeComponentSaver.SetFlagged(PlayModeComponentSaver.ComponentsOf(Selected(), true), false);

        [MenuItem(Root + "Stop Keeping Components (With Children)", true)]
        private static bool StopKeepingValidate() => CanUse();


        [MenuItem("Tools/Sprout's Handy Unity Utils/Play Mode Saver")]
        private static void OpenWindow() => PlayModeSaverWindow.Open();
        #endregion



        #region Logic
        private static GameObject[] Selected() => Selection.GetFiltered<GameObject>(SelectionMode.ExcludePrefab);

        private static bool CanUse() => PlayModeComponentSaver.IsActive && Selected().Length > 0;
        #endregion
    }
}
#endif

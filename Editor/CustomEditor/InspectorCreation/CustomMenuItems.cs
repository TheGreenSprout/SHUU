using UnityEditor;
using UnityEngine;

namespace SHUU._Editor._CustomEditor.InspectorCreation
{
    public class CustomMenuItems : MonoBehaviour
    {
        private static string objectsName = "EditorCreatable_Objects";

        private static EditorCreatable_Objects creatable_Objects = null;




        private static void CreatePrefab(EditorCreatable_ObjectNames name, MenuCommand menuCommand, bool unpack)
        {
            if (creatable_Objects == null)
            {
                creatable_Objects = Resources.Load<EditorCreatable_Objects>(objectsName);

                if (creatable_Objects == null) return;
            }


            GameObject prefab = creatable_Objects.GetObject(name);

            if (prefab == null)
            {
                Debug.LogError($"Prefab not found.");

                return;
            }


            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            GameObjectUtility.SetParentAndAlign(instance, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(instance, "Create Prefab");

            if (unpack)
            {
                PrefabUtility.UnpackPrefabInstance(
                    instance,
                    PrefabUnpackMode.Completely,
                    InteractionMode.UserAction
                );
            }

            Selection.activeObject = instance;
        }




        [MenuItem("GameObject/SHUU/OnEveryScene", false, 20)]
        private static void OnEveryScene(MenuCommand menuCommand)
        {
            CreatePrefab(EditorCreatable_ObjectNames.OnEveryScene, menuCommand, false);
        }



        [MenuItem("GameObject/SHUU/SplineCollider", false, 50)]
        private static void SplineCollider(MenuCommand menuCommand)
        {
            CreatePrefab(EditorCreatable_ObjectNames.SplineCollider, menuCommand, true);
        }



        [MenuItem("GameObject/SHUU/Audio/AudioAmbienceZone_Collider", false, 31)]
        private static void AudioAmbienceZone_Collider(MenuCommand menuCommand)
        {
            CreatePrefab(EditorCreatable_ObjectNames.AudioAmbienceZone_Collider, menuCommand, true);
        }
        [MenuItem("GameObject/SHUU/Audio/AudioAmbienceZone_Shape", false, 32)]
        private static void AudioAmbienceZone_Shape(MenuCommand menuCommand)
        {
            CreatePrefab(EditorCreatable_ObjectNames.AudioAmbienceZone_Shape, menuCommand, true);
        }
        [MenuItem("GameObject/SHUU/Audio/AudioAmbienceZone_Path", false, 33)]
        private static void AudioAmbienceZone_Path(MenuCommand menuCommand)
        {
            CreatePrefab(EditorCreatable_ObjectNames.AudioAmbienceZone_Path, menuCommand, true);
        }
    }
}

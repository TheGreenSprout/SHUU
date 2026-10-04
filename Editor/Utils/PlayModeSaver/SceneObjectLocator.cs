/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Object = UnityEngine.Object;

namespace SHUU._Editor.Utils
{
    [Serializable]
    internal class SceneObjectLocator
    {
        #region Variables
        public string scenePath;
        public int[] siblingIndices;
        public string[] names;

        public string typeName;
        public int componentIndex;


        public string Description
        {
            get
            {
                string path = names == null ? "?" : string.Join("/", names);

                if (string.IsNullOrEmpty(typeName)) return path;

                Type type = Type.GetType(typeName);

                return $"{path} ({(type != null ? type.Name : typeName)})";
            }
        }
        #endregion




        #region Create
        public static bool TryCreate(Object target, out SceneObjectLocator locator, out string problem)
        {
            locator = null;

            Component component = target as Component;
            GameObject gameObject = target as GameObject;

            if (gameObject == null && component != null) gameObject = component.gameObject;

            if (gameObject == null)
            {
                problem = "it isn't a GameObject or a component";

                return false;
            }

            Scene scene = gameObject.scene;

            if (string.IsNullOrEmpty(scene.path))
            {
                problem = "it isn't in a saved scene";

                return false;
            }


            locator = new SceneObjectLocator { scenePath = scene.path };

            FillPath(locator, gameObject.transform);

            if (component != null)
            {
                Type type = component.GetType();

                locator.typeName = type.AssemblyQualifiedName;
                locator.componentIndex = IndexAmongSameType(gameObject, component, type);
            }

            problem = null;

            return true;
        }


        private static void FillPath(SceneObjectLocator locator, Transform transform)
        {
            List<int> indices = new List<int>();
            List<string> names = new List<string>();

            for (Transform current = transform; current != null; current = current.parent)
            {
                indices.Add(current.GetSiblingIndex());
                names.Add(current.name);
            }

            indices.Reverse();
            names.Reverse();

            locator.siblingIndices = indices.ToArray();
            locator.names = names.ToArray();
        }


        private static int IndexAmongSameType(GameObject gameObject, Component component, Type type)
        {
            int index = 0;

            foreach (Component other in gameObject.GetComponents(type))
            {
                if (other == null || other.GetType() != type) continue;

                if (other == component) break;

                index++;
            }

            return index;
        }
        #endregion



        #region Find
        public bool TryFind(out Object target, out string problem)
        {
            target = null;

            if (siblingIndices == null || names == null || siblingIndices.Length == 0 || siblingIndices.Length != names.Length)
            {
                problem = "it wasn't written down properly";

                return false;
            }


            Scene scene = SceneManager.GetSceneByPath(scenePath);

            if (!scene.IsValid() || !scene.isLoaded)
            {
                problem = $"the scene '{scenePath}' isn't open";

                return false;
            }


            Transform current = null;

            for (int depth = 0; depth < siblingIndices.Length; depth++)
            {
                if (depth == 0) current = FindRoot(scene, siblingIndices[0]);
                else current = siblingIndices[depth] < current.childCount ? current.GetChild(siblingIndices[depth]) : null;

                if (current != null && current.name == names[depth]) continue;

                problem = $"couldn't find '{string.Join("/", names)}' where it was (the hierarchy changed, or it was created while playing)";

                return false;
            }


            if (string.IsNullOrEmpty(typeName))
            {
                target = current.gameObject;
                problem = null;

                return true;
            }


            Type type = Type.GetType(typeName);

            if (type == null)
            {
                problem = $"the type '{typeName}' doesn't exist anymore";

                return false;
            }

            int seen = 0;

            foreach (Component candidate in current.GetComponents(type))
            {
                if (candidate == null || candidate.GetType() != type) continue;

                if (seen++ != componentIndex) continue;

                target = candidate;
                problem = null;

                return true;
            }

            problem = $"'{string.Join("/", names)}' doesn't have a {type.Name} in the same place anymore";

            return false;
        }


        private static Transform FindRoot(Scene scene, int siblingIndex)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.transform.GetSiblingIndex() == siblingIndex) return root.transform;

            return null;
        }
        #endregion
    }
}
#endif

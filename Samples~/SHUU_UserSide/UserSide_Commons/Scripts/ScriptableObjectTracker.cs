using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR   
using UnityEditor;
#endif

namespace SHUU.UserSide.Commons
{
    [DefaultExecutionOrder(-20000)]
    public class ScriptableObjectLoader : ScriptableObject
    {
        #region Variables
        public List<ScriptableObject> tracked = new();
        #endregion




        #region Main
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            var loader = Resources.Load<ScriptableObjectLoader>("ScriptableObjectLoader_Asset");

            if (loader != null) loader._Init();
        }

        public void _Init()
        {
            for (int i = 0; i < tracked.Count; i++)
            {
                if (tracked[i] == null)
                {
                    tracked.RemoveAt(i);

                    continue;
                }

                _ = tracked[i];
            }
        }
        #endregion



        #region Logic
        public void Track(ScriptableObject obj)
        {
            if (tracked.Contains(obj)) tracked.Remove(obj);

            tracked.Add(obj);

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
        #endregion
    }
}

#if UNITY_EDITOR   
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;

namespace SHUU.UserSide.Commons
{
    [DefaultExecutionOrder(-20000)]
    public class ScriptableObjectLoader : ScriptableObject
    {
        public List<ScriptableObject> tracked = new();




        private void OnEnable()
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



        public void Track(ScriptableObject obj)
        {
            if (tracked.Contains(obj)) tracked.Remove(obj);


            tracked.Add(obj);


            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }
    }
}

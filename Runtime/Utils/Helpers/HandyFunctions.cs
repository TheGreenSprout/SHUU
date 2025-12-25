using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Linq;
using UnityEngine.UI;


namespace SHUU.Utils.Helpers
{

    #region XML doc
    /// <summary>
    /// Contains handy functions to handle all sorts of small things.
    /// </summary>
    #endregion
    public static class HandyFunctions
    {
        #region Variables

        // Editor Prefs
        public static string ProjectKey => Application.dataPath.GetHashCode().ToString();
        
        #endregion




        #region Strings

        #region XML doc
        /// <summary>
        /// Checks a string for a substring, and in some cases deletes the substring when found.
        /// </summary>
        /// <param name="toCheck">The string you want to check in.</param>
        /// <param name="checkFor">The substring you want to check for.</param>
        /// <param name="startInd">The starting index of the search.</param>
        /// <returns>Returns true if the substring is found, false if not.</returns>
        #endregion
        public static bool CheckAndPopSubstring(ref string toCheck, string checkFor, int startInd, bool removeSubstringChecked)
        {
            int length = checkFor.Length;

            if (toCheck.Substring(startInd, length) == checkFor)
            {
                if (removeSubstringChecked)
                {
                    toCheck = toCheck.Substring(0, startInd) + toCheck.Substring(startInd + length);
                }

                return true;
            }

            return false;
        }


        #region XML doc
        /// <summary>
        /// Turns a string into a "local" version of itself (for the project).
        /// </summary>
        /// <param name="key">The string to be localized.</param>
        /// <returns>Returns the localized string.</returns>
        #endregion
        public static string LocalizeString(string str)
        {
            return ProjectKey + "_" + str;
        }


        public static void CopyToClipboard(string str)
        {
            #if UNITY_2017_1_OR_NEWER
                GUIUtility.systemCopyBuffer = str;
            #else
                TextEditor textEditor = new TextEditor { text = str };

                textEditor.SelectAll();
                textEditor.Copy();
            #endif
        }

        public static string PasteFromClipboard()
        {
            #if UNITY_2017_1_OR_NEWER
                return GUIUtility.systemCopyBuffer;
            #else
                Debug.LogWarning("Pasting from clipboard is not supported in Unity versions older than 2017.1");

                return "";
            #endif
        }

        #endregion



        #region Enums

        public static int GetEnumValFromString<enumType>(string name) where enumType : Enum
        {
            return (int)Enum.Parse(typeof(enumType), name);
        }

        public static string GetEnumNameFromVal<enumType>(int val) where enumType : Enum
        {
            return Enum.GetName(typeof(enumType), val);
        }


        public static int GetEnumLength<enumType>() where enumType : Enum
        {
            return Enum.GetValues(typeof(enumType)).Length;
        }

        #endregion



        #region Lists

        public static bool IndexIsValid<E>(int index, IList<E> list)
        {
            return !(index < 0 || index >= list.Count);
        }

        public static bool IndexIsValidAndNotNull<E>(int index, IList<E> list)
        {
            return IndexIsValid(index, list) && list[index] != null;
        }


        public static void CleanList<E>(this IList<E> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == null)
                {
                    list.RemoveAt(i);

                    i--;
                }
            }
        }


        public static void MoveItemAndShiftList<E>(this IList<E> list, int indexToMove, int newIndex)
        {
            if (!IndexIsValid(indexToMove, list) || !IndexIsValid(newIndex, list))
            {
                return;
            }

            E temp = list[indexToMove];

            list.RemoveAt(indexToMove);


            list.Add(list[list.Count - 1]);
            for (int i = newIndex + 1; i < list.Count; i++)
            {
                list[i] = list[i - 1];
            }


            list[newIndex] = temp;
        }


        public static T RandomElement<T>(this IList<T> list)
        {
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        #endregion



        #region Manage PlayerPrefs

        #region XML doc
        /// <summary>
        /// Checks if an PlayerPref exists.
        /// </summary>
        /// <param name="key">The PlayerPref's key (aka their "name").</param>
        /// <returns>Returns whether the PlayerPref exists.</returns>
        #endregion
        public static bool HasPlayerPref(string key)
        {
            return PlayerPrefs.HasKey(key);
        }


        #region XML doc
        /// <summary>
        /// Saves an PlayerPref.
        /// </summary>
        /// <param name="key">The PlayerPref's key (aka their "name").</param>
        /// <param name="value">The value to save.</param>
        #endregion
        public static void SetPlayerPref<T>(string key, T value)
        {
            if (typeof(T) == typeof(string))
            {
                PlayerPrefs.SetString(key, (string)(object)value);
            }
            else if (typeof(T) == typeof(bool))
            {
                string bool_str = (bool)(object)value ? bool.TrueString : bool.FalseString;
                PlayerPrefs.SetString(key, bool_str);
            }
            else if (typeof(T) == typeof(int))
            {
                PlayerPrefs.SetInt(key, (int)(object)value);
            }
            else if (typeof(T) == typeof(float))
            {
                PlayerPrefs.SetFloat(key, (float)(object)value);
            }
            else if (typeof(T) == typeof(Vector2))
            {
                SetPlayerPref(key + "_x", ((Vector2)(object)value).x);
                SetPlayerPref(key + "_y", ((Vector2)(object)value).y);
            }
            else if (typeof(T) == typeof(Vector3))
            {
                SetPlayerPref(key + "_x", ((Vector3)(object)value).x);
                SetPlayerPref(key + "_y", ((Vector3)(object)value).y);
                SetPlayerPref(key + "_z", ((Vector3)(object)value).z);
            }
            else if (typeof(T) == typeof(Color))
            {
                string hex = ColorUtility.ToHtmlStringRGBA((Color)(object)value);

                SetPlayerPref(key, hex);
            }
            else if (typeof(T).IsEnum)
            {
                PlayerPrefs.SetString(key, (string)(object)value);
            }
            else if (!typeof(T).IsSerializable)
            {
                string json = JsonUtility.ToJson(value);
                PlayerPrefs.SetString(key, json);
            }
            else throw new NotSupportedException($"Type {typeof(T)} is not supported by SetPlayerPref and isn't Serializable.");
        }

        #region XML doc
        /// <summary>
        /// Retrieves an PlayerPref's value.
        /// </summary>
        /// <param name="key">The PlayerPref's key (aka their "name").</param>
        /// <param name="defaultValue">The default value of this PlayerPref.</param>
        /// <returns>Returns the value of the PlayerPref.</returns>
        #endregion
        public static T GetPlayerPref<T>(string key, T defaultValue = default)
        {
            if (!HasPlayerPref(key)) return default;


            if (typeof(T) == typeof(string))
            {
                return (T)(object)PlayerPrefs.GetString(key, (string)(object)defaultValue);
            }
            else if (typeof(T) == typeof(bool))
            {
                string bool_defaultVal_str = (bool)(object)defaultValue ? bool.TrueString : bool.FalseString;
                string bool_str = PlayerPrefs.GetString(key, bool_defaultVal_str);

                bool bool_val = bool_str == bool.TrueString ? true : false;

                return (T)(object)bool_val;
            }
            else if (typeof(T) == typeof(int))
            {
                return (T)(object)PlayerPrefs.GetInt(key, (int)(object)defaultValue);
            }
            else if (typeof(T) == typeof(float))
            {
                return (T)(object)PlayerPrefs.GetFloat(key, (float)(object)defaultValue);
            }
            else if (typeof(T) == typeof(Vector2))
            {
                float x = GetPlayerPref(key + "_x", ((Vector2)(object)defaultValue).x);
                float y = GetPlayerPref(key + "_y", ((Vector2)(object)defaultValue).y);
                return (T)(object)new Vector2(x, y);
            }
            else if (typeof(T) == typeof(Vector3))
            {
                float x = GetPlayerPref(key + "_x", ((Vector3)(object)defaultValue).x);
                float y = GetPlayerPref(key + "_y", ((Vector3)(object)defaultValue).y);
                float z = GetPlayerPref(key + "_z", ((Vector3)(object)defaultValue).z);
                return (T)(object)new Vector3(x, y, z);
            }
            else if (typeof(T) == typeof(Color))
            {
                string hex = GetPlayerPref(key, ((Color)(object)defaultValue).ToString());

                if (ColorUtility.TryParseHtmlString("#" + hex, out var color))
                {
                    return (T)(object)color;
                }

                return defaultValue;
            }
            else if (typeof(T).IsEnum)
            {
                string str = PlayerPrefs.GetString(key, defaultValue.ToString());

                try
                {
                    return (T)Enum.Parse(typeof(T), str);
                }
                catch
                {
                    return defaultValue;
                }
            }
            else if (typeof(T).IsSerializable)
            {
                string json = PlayerPrefs.GetString(key, "");
                if (string.IsNullOrEmpty(json)) return defaultValue;

                try { return JsonUtility.FromJson<T>(json); }
                catch { return defaultValue; }
            }
            else
            {
                throw new NotSupportedException($"Type {typeof(T)} is not supported by GetPlayerPref and isn't Serializable.");
            }
        }


        #region XML doc
        /// <summary>
        /// Deletes an PlayerPref.
        /// </summary>
        /// <param name="key">The PlayerPref's key (aka their "name").</param>
        #endregion
        public static void DeletePlayerPref(string key)
        {
            if (!HasPlayerPref(key)) return;


            PlayerPrefs.DeleteKey(key);
        }

        #endregion



        #region Recursion

        public static T SearchComponent_InSelfAndParents<T>(Transform start) where T : Component
        {
            Transform current = start;


            while (current != null)
            {
                T found = current.GetComponent<T>();

                if (found != null)
                {
                    return found;
                }

                current = current.parent;
            }


            return null; // Not found
        }

        public static T SearchComponent_InSelfAndChildren<T>(Transform start) where T : Component
        {
            Queue<Transform> queue = new Queue<Transform>();

            queue.Enqueue(start);


            while (queue.Count > 0)
            {
                Transform current = queue.Dequeue();


                T found = current.GetComponent<T>();

                if (found != null) return found;


                foreach (Transform child in current) queue.Enqueue(child);
            }


            return null; // Not found
        }


        public static void SetLayer_InSelfAndChildren(GameObject start, LayerMask newLayer)
        {
            SetLayer_InSelfAndChildren(start, LayerMask_To_LayerIndex(newLayer));
        }
        public static void SetLayer_InSelfAndChildren(GameObject start, int newLayer)
        {
            Queue<GameObject> queue = new Queue<GameObject>();
            
            queue.Enqueue(start);


            while (queue.Count > 0)
            {
                GameObject current = queue.Dequeue();


                current.layer = newLayer;


                foreach (Transform child in current.transform) queue.Enqueue(child.gameObject);
            }
        }
        
        #endregion



        #region Layers
        public static int LayerMask_To_LayerIndex(LayerMask mask)
        {
            return Mathf.RoundToInt(Mathf.Log(mask.value, 2));
        }
        #endregion



        #region Screen Resolution

        public static Vector2Int GetClosestAspectRatio(Vector2Int screenValues, bool exactScreenSize = true)
        {
            if (exactScreenSize)
            {
                int width = screenValues.x;
                int height = screenValues.y;

                int gcd = GreatestCommonDivisor(width, height);
                int simpleWidth = width / gcd;
                int simpleHeight = height / gcd;


                return new Vector2Int(simpleWidth, simpleHeight);
            }



            float aspectRatio = 0f;
            if (screenValues.y != 0) aspectRatio = screenValues.x / screenValues.y;


            int[][] commonRatios = new int[][]
            {
                new int[] { 1, 1 },  // 1:1
                new int[] { 1, 2 },  // 1:2
                new int[] { 2, 3 },  // 2:3
                new int[] { 3, 4 },  // 3:4
                new int[] { 4, 5 },  // 4:5
                new int[] { 5, 6 },  // 5:6
                new int[] { 6, 7 },  // 6:7
                new int[] { 7, 8 },  // 7:8
                new int[] { 8, 9 },  // 8:9
                new int[] { 9, 10 }, // 9:10
                new int[] { 1, 3 },  // 1:3
                new int[] { 2, 5 },  // 2:5
                new int[] { 3, 7 },  // 3:7
                new int[] { 4, 9 },  // 4:9
                new int[] { 5, 8 },  // 5:8
                new int[] { 7, 10 }, // 7:10
                new int[] { 3, 5 },  // 3:5
                new int[] { 2, 7 },  // 2:7
                new int[] { 5, 9 },  // 5:9
                new int[] { 6, 11 }, // 6:11
                new int[] { 11, 13 }, // 11:13
                new int[] { 13, 14 }, // 13:14
                new int[] { 16, 9 }, // 16:9
                new int[] { 21, 9 }, // 21:9
                new int[] { 16, 10 }, // 16:10
                new int[] { 4, 3 },  // 4:3
                new int[] { 3, 2 },  // 3:2
                new int[] { 2, 1 },  // 2:1
                new int[] { 5, 4 },  // 5:4
                new int[] { 1, 4 },  // 1:4
            };


            // Find the closest match
            int[] closestRatio = new int[2];
            float closestDifference = float.MaxValue;

            for (int i = 0; i < commonRatios.Length; i++)
            {
                float difference = Mathf.Abs(aspectRatio - ((float)commonRatios[i][0] / commonRatios[i][1]));

                if (difference < closestDifference)
                {
                    closestDifference = difference;
                    closestRatio = commonRatios[i];
                }
            }

            while (closestRatio[0] + closestRatio[1] < 19)
            {
                closestRatio[0] *= 2;
                closestRatio[1] *= 2;
            }


            return new Vector2Int(closestRatio[0], closestRatio[1]);
        }
        
        private static int GreatestCommonDivisor(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }


        public static Vector2 GetCurrentScreenSize()
        {
            Vector2 screenSize;


            screenSize = new Vector2(Screen.width, Screen.height);

#if UNITY_EDITOR
            screenSize = GetMainGameViewSize();
#endif


            return screenSize;
        }

#if UNITY_EDITOR
        private static Vector2 GetMainGameViewSize()
        {
            Type gameViewType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
            EditorWindow gameView = EditorWindow.GetWindow(gameViewType);

            Rect rect = gameView.position;

            return new Vector2Int(Mathf.RoundToInt(rect.width), Mathf.RoundToInt(rect.height));
        }
#endif

        #endregion



        #region Mouse

        public static Vector2 GetMouseScreenCoords(RectTransform canvasRect, Camera cam = null)
        {
            Vector2 mousePos;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, cam, out mousePos);


            return mousePos;
        }


        public static void ChangeMouseLockState(CursorLockMode state, bool? cursorVisible = null)
        {
            Cursor.lockState = state;


            if (cursorVisible != null) ChangeCursorVisibility((bool)cursorVisible);
        }

        public static void LockMouse(bool? cursorVisible = null)
        {
            Cursor.lockState = CursorLockMode.Locked;


            if (cursorVisible != null) ChangeCursorVisibility((bool)cursorVisible);
        }
        public static void ConfineMouse(bool? cursorVisible = null)
        {
            Cursor.lockState = CursorLockMode.Confined;


            if (cursorVisible != null) ChangeCursorVisibility((bool)cursorVisible);
        }
        public static void FreeMouse(bool? cursorVisible = null)
        {
            Cursor.lockState = CursorLockMode.None;


            if (cursorVisible != null) ChangeCursorVisibility((bool)cursorVisible);
        }


        public static void ChangeCursorVisibility(bool? cursorVisible = null)
        {
            if (cursorVisible == null) Cursor.visible = !Cursor.visible;
            else Cursor.visible = (bool)cursorVisible;
        }

        #endregion



        #region Files

        public static T FindFile<T>(string path, T defaultValue = default) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            if (!File.Exists(path)) return defaultValue;
            else
            {
                return AssetDatabase.LoadAssetAtPath<T>(path);
            }
#else
            return defaultValue;
#endif
        }
        public static T FindFile<T>(string[] pathArray, T defaultValue = default) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            for (int i = 0; i < pathArray.Length; i++)
            {
                if (!File.Exists(pathArray[i])) continue;
                else
                {
                    return AssetDatabase.LoadAssetAtPath<T>(pathArray[i]);
                }
            }

            return defaultValue;
#else
            return defaultValue;
#endif
        }

        #endregion
    
        

        #region Gizmos
        public static void DrawCircle(Transform t, Vector2 offset, float radius, bool filled)
        {
            int segments = 32;
            Vector3 center = t.TransformPoint(offset);

            Vector3 prev = center + new Vector3(radius, 0, 0);

            for (int i = 1; i <= segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2f;
                Vector3 next = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);

                if (!filled)
                    Gizmos.DrawLine(prev, next);
                else
                    Gizmos.DrawLine(center, next);

                prev = next;
            }
        }

        public static void DrawPolygon(Transform t, PolygonCollider2D poly, bool filled)
        {
            for (int p = 0; p < poly.pathCount; p++)
            {
                var path = poly.GetPath(p);

                for (int i = 0; i < path.Length; i++)
                {
                    Vector3 a = t.TransformPoint(path[i]);
                    Vector3 b = t.TransformPoint(path[(i + 1) % path.Length]);

                    if (!filled)
                        Gizmos.DrawLine(a, b);
                    else
                        Gizmos.DrawLine(t.TransformPoint(Vector2.zero), a);
                }
            }
        }

        public static void DrawCapsule(CapsuleCollider cap, bool filled)
        {
            // Draw as sphere + box (good enough for visualization)
            Gizmos.DrawWireSphere(cap.center + Vector3.up * (cap.height * .5f - cap.radius), cap.radius);
            Gizmos.DrawWireSphere(cap.center - Vector3.up * (cap.height * .5f - cap.radius), cap.radius);
            Gizmos.DrawWireCube(cap.center, new Vector3(cap.radius * 2, cap.height - cap.radius * 2, cap.radius * 2));
        }
        #endregion



        #region Interact System
        public static bool InteractionRaycast_Check(RaycastHit hit, out IfaceInteractable inactScript, params string[] tags)
        {
            inactScript = null;

            if (!hit.collider.gameObject.TryGetComponent(out IfaceInteractable inact)) return false;
            inactScript = inact;

            if (!inact.CanBeInteracted()) return false;

            foreach (string tag in tags) if (hit.collider.CompareTag(tag)) return true;


            return false;
        }
        #endregion


        
        #region Misc
        public static string GetTypeName(Type t)
        {
            // Primitive C# types
            if (t == typeof(bool))       return "bool";
            if (t == typeof(byte))       return "byte";
            if (t == typeof(sbyte))      return "sbyte";
            if (t == typeof(short))      return "short";
            if (t == typeof(ushort))     return "ushort";
            if (t == typeof(int))        return "int";
            if (t == typeof(uint))       return "uint";
            if (t == typeof(long))       return "long";
            if (t == typeof(ulong))      return "ulong";
            if (t == typeof(float))      return "float";
            if (t == typeof(double))     return "double";
            if (t == typeof(decimal))    return "decimal";
            if (t == typeof(char))       return "char";
            if (t == typeof(string))     return "string";

            // Unity common types
            if (t == typeof(Vector2))         return "Vector2";
            if (t == typeof(Vector2Int))      return "Vector2Int";
            if (t == typeof(Vector3))         return "Vector3";
            if (t == typeof(Vector3Int))      return "Vector3Int";
            if (t == typeof(Vector4))         return "Vector4";
            if (t == typeof(Quaternion))      return "Quaternion";
            if (t == typeof(Color))           return "Color";
            if (t == typeof(Color32))         return "Color32";
            if (t == typeof(Rect))            return "Rect";
            if (t == typeof(RectInt))         return "RectInt";
            if (t == typeof(Bounds))          return "Bounds";
            if (t == typeof(BoundsInt))       return "BoundsInt";
            if (t == typeof(Plane))           return "Plane";
            if (t == typeof(Ray))             return "Ray";
            if (t == typeof(RaycastHit))      return "RaycastHit";

            // Unity Engine base types
            if (t == typeof(GameObject))      return "GameObject";
            if (t == typeof(Transform))       return "Transform";
            if (t == typeof(Component))       return "Component";
            if (t == typeof(UnityEngine.Object))          return "Object";

            // Physics
            if (t == typeof(Rigidbody))       return "Rigidbody";
            if (t == typeof(Rigidbody2D))     return "Rigidbody2D";
            if (t == typeof(Collider))        return "Collider";
            if (t == typeof(Collider2D))      return "Collider2D";
            if (t == typeof(BoxCollider))     return "BoxCollider";
            if (t == typeof(BoxCollider2D))   return "BoxCollider2D";
            if (t == typeof(SphereCollider))  return "SphereCollider";
            if (t == typeof(CapsuleCollider)) return "CapsuleCollider";
            if (t == typeof(MeshCollider))    return "MeshCollider";

            // Rendering / graphics types
            if (t == typeof(Material))        return "Material";
            if (t == typeof(Shader))          return "Shader";
            if (t == typeof(Texture))         return "Texture";
            if (t == typeof(Texture2D))       return "Texture2D";
            if (t == typeof(Sprite))          return "Sprite";
            if (t == typeof(Mesh))            return "Mesh";
            if (t == typeof(Light))           return "Light";
            if (t == typeof(Camera))          return "Camera";

            // UI types
            if (t == typeof(Canvas))          return "Canvas";
            if (t == typeof(RectTransform))   return "RectTransform";
            if (t == typeof(Image))           return "Image";
            if (t == typeof(Text))            return "Text";
            if (t == typeof(TMPro.TMP_Text))  return "TMP_Text";
            if (t == typeof(TMPro.TMP_InputField)) return "TMP_InputField";
            if (t == typeof(Button))          return "Button";
            if (t == typeof(Slider))          return "Slider";
            if (t == typeof(Toggle))          return "Toggle";
            if (t == typeof(Dropdown))        return "Dropdown";

            // Audio
            if (t == typeof(AudioClip))       return "AudioClip";
            if (t == typeof(AudioSource))     return "AudioSource";

            // Misc Unity Types
            if (t == typeof(AnimationCurve))  return "AnimationCurve";
            if (t == typeof(Keyframe))        return "Keyframe";
            if (t == typeof(Gradient))        return "Gradient";
            if (t == typeof(LayerMask))       return "LayerMask";

            // Fallback for generic objects
            return t.Name;
        }
        #endregion



        /*#region URP
        public static ScriptableRenderer URP_GetActiveRenderer()
        {
            var pipeline = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (pipeline == null)
            {
                Debug.LogWarning("Not using URP!");
                return null;
            }

#if UNITY_6000_0_OR_NEWER
            // Unity 6000+: get the renderer via UniversalRenderPipeline singleton
            return UniversalRenderPipeline.asset?.GetDefaultRenderer();
#else
            // Pre-6000 (URP 16 and below)
            return pipeline.scriptableRenderer;
#endif
        }
        #endregion*/
    }

}

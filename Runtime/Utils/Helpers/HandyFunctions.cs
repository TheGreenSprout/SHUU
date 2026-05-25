using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using UnityEngine.UI;
using System.Collections;

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
        public static string ApplicationPath => Application.dataPath;

        public static string ProjectKey => ApplicationPath.GetHashCode().ToString();

        

        public static event Action<CursorLockMode> OnCursorStateChange;
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
                if (removeSubstringChecked) toCheck = toCheck.Substring(0, startInd) + toCheck.Substring(startInd + length);

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
        public static string LocalizeString(this string str) => ProjectKey + "_" + str;


        public static void CopyToClipboard(this string str)
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


        public static string GetColorOpenTag_RichText(this Color color) => "<color=#" + ColorUtility.ToHtmlStringRGBA(color) + ">";

        public static string EncloseInColorTags_RichText(this string text, Color color) => color.GetColorOpenTag_RichText() + text + "</color>";
        #endregion



        #region Enums
        public static int GetEnumValFromString<enumType>(this string name) where enumType : Enum => (int)Enum.Parse(typeof(enumType), name);

        public static string GetEnumNameFromVal<enumType>(this int val) where enumType : Enum => Enum.GetName(typeof(enumType), val);


        public static int GetEnumLength<enumType>() where enumType : Enum => Enum.GetValues(typeof(enumType)).Length;
        #endregion



        #region Lists
        public static int Count<E>(this IEnumerable<E> source)
        {
            if (source == null) return 0;

            if (source is ICollection<E> collection) return collection.Count;
            else
            {
                int count = 0;
                foreach (var item in source) count++;
                return count;
            }
        }

        public static E ElementAt<E>(this IEnumerable<E> source, int index)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            if (source is IList<E> list) return list[index];
            else
            {
                int currentIndex = 0;
                foreach (var item in source)
                {
                    if (currentIndex == index) return item;
                    currentIndex++;
                }
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
        

        public static bool IndexIsValid<E>(this IEnumerable<E> list, int index) => !(index < 0 || index >= list.Count());

        public static bool IndexIsValidAndNotNull<E>(this IEnumerable<E> list, int index) => list.IndexIsValid(index) && list.ElementAt(index) != null;


        public static void Clean<E>(this IList<E> list)
        {
            if (list is Array) return;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == null)
                {
                    list.RemoveAt(i);

                    i--;
                }
            }
        }

        public static IEnumerable<E> Clean<E>(this IEnumerable<E> source)
        {
            foreach (var item in source)
                if (item != null) yield return item;
        }


        public static bool NonLINQ_Contains<E>(this IEnumerable<E> source, E item)
        {
            foreach (var x in source)
                if (Equals(x, item)) return true;

            return false;
        }


        public static void CopyFrom_List<E>(this IList<E> listTo, IList<E> listFrom)
        {
            if (listTo is Array || listFrom is Array || listFrom == null) return;


            if (listTo == null)
            {
                Debug.LogError("HandyFunctions 'CopyFrom_List' method requires the list to not be null.");

                return;
            }

            if (listTo.Count > 0) listTo.Clear();
            foreach (var item in listFrom) listTo.Add(item);
        }

        public static void CopyFrom_List_CopyContructors<E>(this IList<E> listTo, IList<E> listFrom, Func<E, E> copier)
        {
            if (listTo is Array || listFrom is Array || listFrom == null) return;


            if (listTo == null)
            {
                Debug.LogError("HandyFunctions 'CopyFrom_List' method requires the list to not be null.");

                return;
            }

            if (listTo.Count > 0) listTo.Clear();
            foreach (var item in listFrom) listTo.Add(copier(item));
        }


        public static IEnumerable<T> Merge<T>(params IEnumerable<T>[] sources)
        {
            foreach (var source in sources)
                foreach (var item in source)
                    yield return item;
        }


        #region List functions for arrays
        public static T[] Add<T>(this T[] array, T item)
        {
            if (array == null) return new T[] { item };


            T[] result = new T[array.Length + 1];
            Array.Copy(array, result, array.Length);
            
            result[result.Length - 1] = item;


            return result;
        }

        public static T[] RemoveAt<T>(this T[] array, int index)
        {
            if (array == null) return Array.Empty<T>();

            if (!array.IndexIsValid(index)) throw new ArgumentOutOfRangeException(nameof(index));


            T[] result = new T[array.Length - 1];

            if (index > 0) Array.Copy(array, 0, result, 0, index);

            if (index < array.Length - 1) Array.Copy(array, index + 1, result, index, array.Length - index - 1);


            return result;
        }
        public static T[] Remove<T>(this T[] array, T item)
        {
            if (array == null || !array.Contains(item)) return array;


            int index = Array.IndexOf(array, item);

            if (index == -1) return array;


            return array.RemoveAt(index);
        }

        public static T[] Insert<T>(this T[] array, int index, T item)
        {
            if (array == null) return new T[] { item };

            if (!array.IndexIsValid(index)) throw new ArgumentOutOfRangeException(nameof(index));


            T[] result = new T[array.Length + 1];

            if (index > 0) Array.Copy(array, 0, result, 0, index);

            result[index] = item;

            if (index < array.Length) Array.Copy(array, index, result, index + 1, array.Length - index);


            return result;
        }
        #endregion


        public static void MoveItem<E>(this IList<E> list, int from, int to)
        {
            if (list is Array) return;

            if (!((IReadOnlyList<E>)list).IndexIsValidAndNotNull(from) || !((IReadOnlyList<E>)list).IndexIsValid(to) || from == to) return;


            list.Insert(to, list[from]);

            if (from > to) from++;
            list.RemoveAt(from);
        }
        public static T[] MoveItemArray<T>(this T[] array, int from, int to)
        {
            if (!array.IndexIsValidAndNotNull(from) || !array.IndexIsValid(to) || from == to) return array;


            T item = array[from];
            array = array.RemoveAt(from);

            if (from < to) to--;
            array = array.Insert(to, item);


            return array;
        }


        public static T RandomElement<T>(this IReadOnlyList<T> list) => list[UnityEngine.Random.Range(0, list.Count)];
        public static T RandomElement<T>(this IEnumerable<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var list = source as IList<T> ?? source.ToList();
            return list[UnityEngine.Random.Range(0, list.Count)];
        }
        
        
        public static Dictionary<T, E> ToDictionary<T, E>(this IEnumerable<E> source, Func<E, T> getKey)
        {
            if (source == null || getKey == null) return null;
            
             
            Dictionary<T, E> dict = new();

            foreach (var item in source)
                dict[getKey.Invoke(item)] = item;

            return dict;
        }
        #endregion



        #region Recursion
        public static T SearchComponent_InSelfAndParents<T>(this Transform start) where T : Component
        {
            Transform current = start;


            while (current != null)
            {
                T found = current.GetComponent<T>();

                if (found != null) return found;

                current = current.parent;
            }


            return null;
        }

        public static T SearchComponent_InSelfAndChildren<T>(this Transform start) where T : Component
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


            return null;
        }


        public static void SetLayer_InSelfAndChildren(this GameObject start, LayerMask newLayer) => SetLayer_InSelfAndChildren(start, newLayer.ToLayerIndex());
        public static void SetLayer_InSelfAndChildren(this GameObject start, int newLayer)
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
        public static int ToLayerIndex(this LayerMask mask) => Mathf.RoundToInt(Mathf.Log(mask.value, 2));

        public static bool Contains_Layer(this LayerMask mask, int layer) => (mask.value & (1 << layer)) != 0;
        #endregion



        #region Screen Resolution
        public static Vector2Int GetClosestAspectRatio(this Vector2Int screenValues, bool exactScreenSize = true)
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
        public static Vector2 GetMouseScreenCoords(this RectTransform canvasRect, Camera cam = null)
        {
            Vector2 mousePos;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, cam, out mousePos);


            return mousePos;
        }


        public static void ChangeMouseLockState(CursorLockMode state, bool? cursorVisible = null)
        {
            Cursor.lockState = state;


            if (cursorVisible != null) ChangeMouseVisibility((bool)cursorVisible);


            OnCursorStateChange?.Invoke(state);
        }

        public static void LockMouse(bool? cursorVisible = null) => ChangeMouseLockState(CursorLockMode.Locked, cursorVisible);
        public static void ConfineMouse(bool? cursorVisible = null) => ChangeMouseLockState(CursorLockMode.Confined, cursorVisible);
        public static void FreeMouse(bool? cursorVisible = null) => ChangeMouseLockState(CursorLockMode.None, cursorVisible);

        private static (CursorLockMode, bool?)? savedCursorState = null;
        public static bool ChangeMouseLockState_Temporary(CursorLockMode state, bool? cursorVisible = null)
        {
            if (savedCursorState != null) return false;


            savedCursorState = (Cursor.lockState, Cursor.visible);

            ChangeMouseLockState(state, cursorVisible);


            return true;
        }
        public static bool ReturnMouseLockState_FromTemporary()
        {
            if (savedCursorState == null) return false;


            ChangeMouseLockState(savedCursorState.Value.Item1, savedCursorState.Value.Item2);

            savedCursorState = null;


            return true;
        }
    
    
        public static void ChangeMouseVisibility(bool? cursorVisible = null)
        {
            if (cursorVisible == null) Cursor.visible = !Cursor.visible;
            else Cursor.visible = (bool)cursorVisible;
        }

        private static bool? savedCursorVisibility = null;
        public static bool ChangeMouseVisibility_Temporary(bool cursorVisible)
        {
            if (savedCursorVisibility != null) return false;


            savedCursorVisibility = Cursor.visible;

            ChangeMouseVisibility(cursorVisible);


            return true;
        }
        public static bool ReturnMouseVisibility_FromTemporary()
        {
            if (savedCursorVisibility == null) return false;


            ChangeMouseVisibility(savedCursorVisibility);

            savedCursorVisibility = null;


            return true;
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

                if (!filled) Gizmos.DrawLine(prev, next);
                else Gizmos.DrawLine(center, next);

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

                    if (!filled) Gizmos.DrawLine(a, b);
                    else Gizmos.DrawLine(t.TransformPoint(Vector2.zero), a);
                }
            }
        }

        public static void DrawCapsule(CapsuleCollider cap)
        {
            Gizmos.DrawWireSphere(cap.center + Vector3.up * (cap.height * .5f - cap.radius), cap.radius);
            Gizmos.DrawWireSphere(cap.center - Vector3.up * (cap.height * .5f - cap.radius), cap.radius);
            Gizmos.DrawWireCube(cap.center, new Vector3(cap.radius * 2, cap.height - cap.radius * 2, cap.radius * 2));
        }
        #endregion



        #region Interaction System
        public static bool Contains_Tag(this IEnumerable<string> source, string item)
        {
            if (source == null) return true;
            
            int count = 0;
            foreach (var x in source)
            {
                count++;
                if (Equals(x, item)) return true;
            }
            
            if (count == 0) return true;

            return false;
        }

        
        public static bool InteractionRaycast(ref IfaceInteractable previousInact, Ray ray, float interactionRange, LayerMask? interactionLayers = null, bool modifyDynamicCursor = true, params string[] tags)
        {
            bool raycast;
            RaycastHit hitInfo;

            if (interactionLayers != null) raycast = Physics.Raycast(ray, out hitInfo, interactionRange, interactionLayers.Value);
            else raycast = Physics.Raycast(ray, out hitInfo, interactionRange);


            if (raycast && hitInfo.InteractionRaycast_Check(out IfaceInteractable inact, tags))
            {
                if (previousInact != inact)
                {
                    ClearInteractHover(ref previousInact, modifyDynamicCursor);

                    previousInact = inact;


                    inact.HoverStart(modifyDynamicCursor);
                }
            }
            else ClearInteractHover(ref previousInact, modifyDynamicCursor);


            return raycast;
        }
        public static bool InteractionRaycast(ref IfaceInteractable previousInact, Camera camera, float interactionRange, LayerMask? interactionLayers = null, bool modifyDynamicCursor = true, params string[] tags)
        {
            if (camera == null) return InteractionRaycast(
                                    ref previousInact,
                                    interactionRange,
                                    interactionLayers,
                                    modifyDynamicCursor,
                                    tags
                                );


            return InteractionRaycast(
                ref previousInact,
                camera.ScreenPointToRay(Input.mousePosition),
                interactionRange,
                interactionLayers,
                modifyDynamicCursor,
                tags
            );
        }
        public static bool InteractionRaycast(ref IfaceInteractable previousInact, float interactionRange, LayerMask? interactionLayers = null, bool modifyDynamicCursor = true, params string[] tags)
        {
            return InteractionRaycast(
                ref previousInact,
                Camera.main.ScreenPointToRay(Input.mousePosition),
                interactionRange,
                interactionLayers,
                modifyDynamicCursor,
                tags
            );
        }

        public static bool InteractionRaycast_Check(this RaycastHit hit, out IfaceInteractable inactScript, params string[] tags)
        {
            inactScript = null;

            if (!hit.collider.gameObject.TryGetComponent(out IfaceInteractable inact)) return false;
            inactScript = inact;

            if (!inact.CanBeInteracted()) return false;

            if (!tags.Contains_Tag(hit.collider.tag)) return false;


            return true;
        }

        public static void ClearInteractHover(ref IfaceInteractable previousInact, bool modifyDynamicCursor)
        {
            if (previousInact == null) return;

            previousInact.HoverEnd(modifyDynamicCursor);
            previousInact = null;
        }
        #endregion



        #region Misc
        public static string GetTypeName(this Type t)
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


        public static IEnumerator ListenForInput_Enumerator(Action<string> callback)
        {
            yield return new WaitUntil(() =>
                Input.anyKeyDown ||
                Input.GetMouseButtonDown(0) ||
                Input.GetMouseButtonDown(1) ||
                Input.GetMouseButtonDown(2) ||
                Input.GetMouseButtonDown(3) ||
                Input.GetMouseButtonDown(4) ||
                Input.GetMouseButtonDown(5) ||
                Input.GetMouseButtonDown(6)
            );
    
            callback?.Invoke(DetectInput());


            yield break;
        }
        private static string DetectInput()
        {
            foreach (KeyCode k in Enum.GetValues(typeof(KeyCode)))
                if (Input.GetKeyDown(k)) return InputParser.InputToString(k);
                
            for (int i = 0; i <= 6; i++)
                if (Input.GetMouseButtonDown(i)) return InputParser.InputToString(i);

            return null;
        }


        public static float[] Vector3s_To_FloatArray(params Vector3[] vectors)
        {
            float[] result = new float[vectors.Length * 3];
            for (int i = 0; i < vectors.Length; i++)
            {
                int idx = i * 3;
                result[idx]     = vectors[i].x;
                result[idx + 1] = vectors[i].y;
                result[idx + 2] = vectors[i].z;
            }
            return result;
        }

        public static Vector3[] Floata_To_Vector3Array(float[] data)
        {
            Vector3[] result = new Vector3[data.Length / 3];
            for (int i = 0; i < result.Length; i++)
            {
                int idx = i * 3;
                result[i] = new Vector3(data[idx], data[idx + 1], data[idx + 2]);
            }
            return result;
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

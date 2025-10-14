using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


namespace SHUU.Utils.Helpers
{

    #region XML doc
    /// <summary>
    /// Contains handy functions to handle all sorts of small things.
    /// </summary>
    #endregion
    public static class HandyFunctions
    {
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


        public static void CleanList<E>(ref IList<E> list)
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


        public static void MoveItemAndShiftList<E>(ref IList<E> list, int indexToMove, int newIndex)
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

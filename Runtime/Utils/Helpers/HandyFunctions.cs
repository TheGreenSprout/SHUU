using UnityEngine;
using System;
using System.Collections.Generic;


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
    public static bool CheckAndPopSubstring(ref string toCheck, string checkFor, int startInd, bool removeSubstringChecked){
        int length = checkFor.Length;

        if (toCheck.Substring(startInd, length) == checkFor)
        {
            if (removeSubstringChecked)
            {
                toCheck = toCheck.Substring(0, startInd) + toCheck.Substring(startInd+length);
            }

            return true;
        }

        return false;
    }

    #endregion



    #region Enums

    public static int GetEnumValFromString<enumType>(string name) where enumType : Enum{
        return (int) Enum.Parse(typeof(enumType), name);
    }

    public static string GetEnumNameFromVal<enumType>(int val) where enumType : Enum{
        return Enum.GetName(typeof(enumType), val);
    }


    public static int GetEnumLength<enumType>() where enumType : Enum{
        return Enum.GetValues(typeof(enumType)).Length;
    }

    #endregion



    #region Lists

    public static bool IndexIsValid<T>(int index, List<T> list){
        return !(index < 0 || index >= list.Count);
    }
    
    public static bool IndexIsValidAndNotNull<T>(int index, List<T> list){
        return IndexIsValid(index, list) && list[index] != null;
    }
    

    public static void CleanList<T>(ref List<T> list)
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


        public static void MoveItemAndShiftList<T>(ref List<T> list, int indexToMove, int newIndex)
        {
            if (!IndexIsValid(indexToMove, list) || !IndexIsValid(newIndex, list))
            {
                return;
            }

            T temp = list[indexToMove];

            list.RemoveAt(indexToMove);


            list.Add(list[list.Count - 1]);
            for (int i = newIndex + 1; i < list.Count; i++)
            {
                list[i] = list[i - 1];
            }


            list[newIndex] = temp;
        }

    #endregion
}

}

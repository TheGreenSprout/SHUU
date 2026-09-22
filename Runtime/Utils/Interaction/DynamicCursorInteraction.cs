using System;
using System.Collections.Generic;
using UnityEngine;

using static SHUU.Utils.Helpers.HandyFunctions;

namespace SHUU.Utils.Interaction
{
    public static class DynamicCursorInteraction
    {
        #region Variables
        public static event Action<bool, GameObject> AlternateCursorState = null;

        public static bool CursorActive => AmmountOfInteracts.Count != 0;



        private static List<GameObject> AmmountOfInteracts = new();

        private static bool ActionCalled = false;
        #endregion




        #region Logic
        public static void AddCursorAffector(GameObject id)
        {
            if (AmmountOfInteracts.Contains(id)) return;

            AmmountOfInteracts.Add(id);


            UpdateState();
        }

        public static void RemoveCursorAffector(GameObject id)
        {
            if (!AmmountOfInteracts.Contains(id)) return;

            AmmountOfInteracts.Remove(id);


            UpdateState();
        }


        private static void UpdateState()
        {
            AmmountOfInteracts.Clean();

            
            if (AmmountOfInteracts.Count == 0)
            {
                if (!ActionCalled)
                {
                    ActionCalled = true;

                    AlternateCursorState?.Invoke(true, null);
                }
            }
            else
            {
                if (ActionCalled)
                {
                    ActionCalled = false;

                    AlternateCursorState?.Invoke(false, AmmountOfInteracts[AmmountOfInteracts.Count-1]);
                }
            }
        }
        #endregion
    }
}

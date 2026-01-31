using System;
using System.Collections.Generic;
using UnityEngine;

namespace SHUU.Utils.Helpers.Interaction
{
    public static class DynamicCursorInteraction
    {
        public static event Action<bool, GameObject> alternateCursorState = null;

        public static bool cursorActive => ammountOfInteracts.Count != 0;



        private static List<GameObject> ammountOfInteracts = new();

        private static bool actionCalled = false;




        public static void AddCursorAffector(GameObject id)
        {
            if (ammountOfInteracts.Contains(id)) return;

            ammountOfInteracts.Add(id);


            UpdateState();
        }


        public static void RemoveCursorAffector(GameObject id)
        {
            if (!ammountOfInteracts.Contains(id)) return;

            ammountOfInteracts.Remove(id);


            UpdateState();
        }



        private static void UpdateState()
        {
            ammountOfInteracts.Clean();

            
            if (ammountOfInteracts.Count == 0)
            {
                if (!actionCalled)
                {
                    actionCalled = true;

                    alternateCursorState?.Invoke(true, null);
                }
            }
            else
            {
                if (actionCalled)
                {
                    actionCalled = false;

                    alternateCursorState?.Invoke(false, ammountOfInteracts[ammountOfInteracts.Count-1]);
                }
            }
        }
    }
}

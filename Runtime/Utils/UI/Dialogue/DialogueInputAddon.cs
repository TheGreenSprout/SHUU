using System;
using UnityEngine;

namespace SHUU.Utils.UI.Dialogue
{
    
    [DisallowMultipleComponent]
    public class DialogueInputAddon : MonoBehaviour
    {
        public Action nextLine_callback;

        public Action fastForard_callback;

        public Action skip_callback;




        public virtual void NextLine_Press()
        {
            nextLine_callback?.Invoke();
        }


        public virtual void FastForward_Press()
        {
            fastForard_callback?.Invoke();
        }


        public virtual void Skip_Press()
        {
            skip_callback?.Invoke();
        }
    }

}

using System;
using UnityEngine;

namespace SHUU.Utils.UI
{
    [DisallowMultipleComponent]
    public class TypewriterTextInput : MonoBehaviour
    {
        #region Variables
        public event Action fastForwardRequested;
        public event Action skipRequested;
        #endregion




        #region Logic
        public void RequestFastForward() => fastForwardRequested?.Invoke();
        public void RequestSkip() => skipRequested?.Invoke();
        #endregion
    }
}
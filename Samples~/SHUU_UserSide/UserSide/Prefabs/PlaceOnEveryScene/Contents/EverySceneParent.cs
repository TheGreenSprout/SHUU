using UnityEngine;
using UnityEngine.Events;

using SHUU.Utils.Globals;

namespace SHUU.UserSide
{

    public class EverySceneParent : MonoBehaviour
    {
        #region Variables
        public UnityEvent destroyEvent = null;
        #endregion




        #region Variables
        private void Awake() => SHUU_Time.OnNextFrame += Dissolve;
        
        private void Dissolve() => destroyEvent.Invoke();


        public void DestroyExternal() => Destroy(this);
        #endregion
    }
}
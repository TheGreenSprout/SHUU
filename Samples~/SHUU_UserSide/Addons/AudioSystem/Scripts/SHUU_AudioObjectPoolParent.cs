using SHUU.Utils;
using UnityEngine;

namespace SHUU.UserSide.Addons.AudioSystem
{
    public class SHUU_AudioObjectPoolParent : MonoBehaviour
    {
        #region Variables
        internal static SHUU_AudioObjectPoolParent Instance = null;


        [SerializeField] private string nameReplacePoint = "$NAME$";
        #endregion



        #region Main
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;


        public Transform Init(string channelName, bool dontDestroyOnLoad = false)
        {
            name = name.Replace(nameReplacePoint, channelName);

            if (dontDestroyOnLoad) 
            {
                if (Instance == null)
                {
                    Instance = this;
                    DontDestroyOnLoad(this);
                }
                else if (Instance != this)
                {
                    Destroy(this);
                    return null;
                }
            }

            return transform;
        }


        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
        #endregion
    }
}

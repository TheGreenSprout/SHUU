using UnityEngine;

namespace SHUU.Utils.BaseScripts.Audio
{
    public class AudioEnsurePlay : MonoBehaviour
    {
        #region Main
        private void OnEnable() => GetComponent<AudioSource>().enabled = true;
        #endregion
    }
}

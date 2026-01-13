using UnityEngine;

namespace SHUU.Utils.BaseScripts.Audio
{
    public class AudioEnsurePlay : MonoBehaviour
    {
        private void OnEnable() => GetComponent<AudioSource>().enabled = true;
    }
}

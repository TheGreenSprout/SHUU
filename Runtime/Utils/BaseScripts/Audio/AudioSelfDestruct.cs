using SHUU.Utils.Helpers;
using UnityEngine;

namespace SHUU.Utils.BaseScripts.Audio
{
    #region XML doc
    /// <summary>
    /// Automatically destroys an audio instance when it's done playing.
    /// </summary>
    #endregion
    public class AudioSelfDestruct : MonoBehaviour
    {
        private SHUU_ObjectPool<AudioSource> pool = null;


        private AudioSource thisSource;




        public void Setup(bool active, SHUU_ObjectPool<AudioSource> _pool = null)
        {
            pool = _pool;


            thisSource = this.GetComponent<AudioSource>();
            
            if (active) Invoke(nameof(CheckAudio), 1f);
        }


        private void CheckAudio()
        {
            if (!thisSource.isPlaying)
            {
                DestroySource();
            }

            Invoke(nameof(CheckAudio), 1f);
        }


        public void DestroySource()
        {
            if (pool == null) Destroy(gameObject);
            else pool.Return(thisSource);
        }
    }
}

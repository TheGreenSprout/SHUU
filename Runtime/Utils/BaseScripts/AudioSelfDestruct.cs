using UnityEngine;

namespace SHUU.Utils.BaseScripts
{

#region XML doc
/// <summary>
/// Automatically destroys an audio instance when it's done playing.
/// </summary>
#endregion
public class AudioSelfDestruct : MonoBehaviour
{
    private AudioSource thisSource;




    private void Start()
    {
        thisSource = this.GetComponent<AudioSource>();
        
        Invoke(nameof(CheckAudio), 1f);
    }


    void CheckAudio()
    {
        if (!thisSource.isPlaying)
        {
            Destroy(gameObject);
        }

        Invoke(nameof(CheckAudio), 1f);
    }
}

}

using UnityEngine;

public class AudioEnsurePlay : MonoBehaviour
{
    private void Start() {
        GetComponent<AudioSource>().enabled = true;
    }
}

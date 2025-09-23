using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueSounds", menuName = "SHUU/Dialogue/TalkingSounds")]
public class DialogueSounds : ScriptableObject
{
    public List<AudioClip> sounds = new List<AudioClip>();

    [Range(0f, 1f)]
    public float volume = 1f;

    public void SetClips(List<AudioClip> clips)
    {
        sounds = new List<AudioClip>(clips); // Ensure a copy is made
    }
}
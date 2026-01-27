using UnityEngine;

namespace SHUU.Utils.SettingsSytem
{
    [CreateAssetMenu(fileName = "SettingsData", menuName = "SHUU/SettingsData")]
    public class SettingsData : ScriptableObject
    {
        [Header("Gameplay")]
        public float cameraSensitivity = 5f;



        [Header("Audio")]
        public float masterVolume = 1f;
        public float musicVolume = 1f;
        public float sfxVolume = 1f;



        [Header("Graphics")]
        public bool fullscreen = true;
        public int resolutionIndex = 0;
        public int qualityIndex = 2;
    }
}

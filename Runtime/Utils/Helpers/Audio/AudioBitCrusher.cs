/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/




using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioBitCrusher : MonoBehaviour
{
    #region Variables
    [Range(1, 25)] [SerializeField] private int sampleRate = 1;
    [Range(0.1f, 16f)] [SerializeField] private float sampleDepth = 1.0f;
    [Range(2, 32)] [SerializeField] private int bitDepth = 8;


    private AudioSource audioSource;
    #endregion



    
    #region Main
    private void Awake() => audioSource = GetComponent<AudioSource>();
    #endregion



    #region Logic
    private void OnAudioFilterRead(float[] data, int channels)
    {
        float avgl, avgr;

        float stepSize = 1.0f / (Mathf.Pow(2, bitDepth - 1) - 1);

        for (int i = 0; i < data.Length - sampleRate * channels; i += sampleRate * channels)
        {
            avgl = 0.0f;
            avgr = 0.0f;


            for (int j = 0; j < sampleRate * channels; j += channels)
            {
                avgl += data[i + j];

                if (channels > 1) avgr += data[i + j + 1];
            }


            avgl /= sampleRate;
            avgr /= sampleRate;

            avgl *= sampleDepth;
            avgr *= sampleDepth;

            avgl = Mathf.Round(avgl / stepSize) * stepSize;
            avgr = Mathf.Round(avgr / stepSize) * stepSize;

            avgl /= sampleDepth;
            avgr /= sampleDepth;

            avgl = Mathf.Clamp(avgl, -1f, 1f);
            avgr = Mathf.Clamp(avgr, -1f, 1f);


            for (int j = 0; j < sampleRate * channels; j += channels)
            {
                data[i + j] = avgl;

                if (channels > 1) data[i + j + 1] = avgr;
            }
        }
        #endregion
    }
}

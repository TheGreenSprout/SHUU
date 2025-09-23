using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioBitCrusher : MonoBehaviour
{
    [Range(1, 25)]
    public int sampleRate = 1; // Sample rate in Hz

    [Range(0.1f, 16f)]
    public float sampleDepth = 1.0f; // Sample depth as a float

    [Range(2, 32)]
    public int bitDepth = 8; // Bit depth


    private AudioSource audioSource;




    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.spatialBlend = 1.0f; // Ensure the audio source is set to 3D
    }


    void OnAudioFilterRead(float[] data, int channels)
    {
        float avgl, avgr;


        // Calculate the step size based on bit depth
        float stepSize = 1.0f / (Mathf.Pow(2, bitDepth - 1) - 1);


        for (int i = 0; i < data.Length - sampleRate * channels; i += sampleRate * channels)
        {
            avgl = 0.0f;
            avgr = 0.0f;


            for (int j = 0; j < sampleRate * channels; j += channels)
            {
                avgl += data[i + j];

                if (channels > 1)
                {
                    avgr += data[i + j + 1];
                }
            }


            avgl /= (float)sampleRate;
            avgr /= (float)sampleRate;


            // Apply sample depth
            avgl *= sampleDepth;
            avgr *= sampleDepth;


            // Quantize based on bit depth
            avgl = Mathf.Round(avgl / stepSize) * stepSize;
            avgr = Mathf.Round(avgr / stepSize) * stepSize;


            // Normalize back
            avgl /= sampleDepth;
            avgr /= sampleDepth;


            for (int j = 0; j < sampleRate * channels; j += channels)
            {
                data[i + j] = avgl;
                if (channels > 1)
                {
                    data[i + j + 1] = avgr;
                }
            }
        }
    }
}

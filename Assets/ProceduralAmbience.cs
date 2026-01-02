using UnityEngine;

// Genera un sottofondo "Dark Ambient" usando Rumore Browniano.
// Questo riempie il silenzio e aggiunge atmosfera senza usare file mp3.

[RequireComponent(typeof(AudioSource))]
public class ProceduralAmbience : MonoBehaviour
{
    [Range(0f, 1f)]
    public float volume = 0.2f;

    private float lastValue = 0.0f;
    private System.Random rng; // Use System.Random for thread safety

    void Start()
    {
        // Initialize System.Random instance
        rng = new System.Random();

        AudioSource source = GetComponent<AudioSource>();
        source.loop = true;
        source.spatialBlend = 0.0f; // 2D (Ambientale, ovunque uguale)
        source.Play();
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        // OnAudioFilterRead runs on a separate thread, so we must use System.Random
        // which is thread-safe enough for this context, or at least independent of Unity's main thread API.
        
        for (int i = 0; i < data.Length; i += channels)
        {
            // Generazione Brown Noise (integrando rumore bianco)
            // Generate random double between 0.0 and 1.0, map to -1.0 to 1.0
            float white = (float)(rng.NextDouble() * 2.0 - 1.0);
            
            // Formula filtro passa-basso semplice
            lastValue = (lastValue + (0.05f * white)) / 1.02f;
            
            // Compensazione volume (il brown noise tende a scappare)
            lastValue = Mathf.Clamp(lastValue, -1f, 1f);

            for (int c = 0; c < channels; c++)
            {
                data[i + c] = lastValue * volume;
            }
        }
    }
}
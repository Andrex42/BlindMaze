using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ProceduralBeacon : MonoBehaviour
{
    // ... (Codice precedente invariato per la sintesi) ...
    [Header("Sintesi Base")]
    [Range(100, 1000)]
    public float baseFrequency = 440.0f;
    public float volume = 0.5f;

    [Header("Dinamica Sonar")]
    public Transform player;
    public float minPulseSpeed = 1.0f;
    public float maxPulseSpeed = 12.0f;
    public float maxDistance = 50.0f;

    private double phase = 0.0;
    private double lfoPhase = 0.0; 
    private double sampleRate;
    private volatile float currentPulseSpeed = 1.0f;

    void Start()
    {
        sampleRate = AudioSettings.outputSampleRate;
        AudioSource audioSource = GetComponent<AudioSource>();
        
        // --- CONFIGURAZIONE BINAURALE/SPAZIALE ---
        audioSource.spatialBlend = 1.0f; // 100% 3D (Cruciale!)
        audioSource.dopplerLevel = 0.0f; // No Doppler per evitare distorsioni di pitch indesiderate
        audioSource.spread = 0;          // 0 = Puntiforme (Direzione precisa), 360 = Suono ovunque
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic; // Calo volume naturale
        audioSource.minDistance = 1.0f;
        audioSource.maxDistance = 50.0f;
        
        // Se hai attivato Oculus/Steam Audio nelle impostazioni, spunta "Spatialize" qui sotto:
        audioSource.spatialize = true; 
        
        audioSource.loop = true;
        audioSource.Play();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    // ... (Update e OnAudioFilterRead restano uguali a prima) ...
    void Update()
    {
        if (player == null) return;
        float dist = Vector3.Distance(transform.position, player.position);
        float t = 1.0f - Mathf.Clamp01(dist / maxDistance);
        currentPulseSpeed = Mathf.Lerp(minPulseSpeed, maxPulseSpeed, t);
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        double sampleDuration = 1.0 / sampleRate;
        for (int i = 0; i < data.Length; i += channels)
        {
            lfoPhase += (currentPulseSpeed * 2.0 * Mathf.PI) * sampleDuration;
            if (lfoPhase > 2.0 * Mathf.PI) lfoPhase -= 2.0 * Mathf.PI;
            float amplitudeLFO = Mathf.Sin((float)lfoPhase);
            if (amplitudeLFO < 0) amplitudeLFO = 0; 
            float currentVol = volume * amplitudeLFO;

            phase += (baseFrequency * 2.0 * Mathf.PI) * sampleDuration;
            if (phase > 2.0 * Mathf.PI) phase -= 2.0 * Mathf.PI;
            float signal = Mathf.Sin((float)phase);

            for (int j = 0; j < channels; j++)
            {
                data[i + j] = signal * currentVol;
            }
        }
    }
}